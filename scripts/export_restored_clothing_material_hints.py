#!/usr/bin/env python3

from __future__ import annotations

import re
from collections import Counter
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable


REPO_ROOT = Path("/Users/garretcoffman/Documents/VSMods/VSTemporalReverser")
MOD_SOURCE = REPO_ROOT / "VSTemporalReverser" / "ItemTemporalReverser.cs"
VANILLA_ITEMTYPES = Path("/Applications/Vintage Story 1.22.app/assets/survival/itemtypes/wearable/seraph")
VANILLA_CLOTHES_RECIPES = Path("/Applications/Vintage Story 1.22.app/assets/survival/recipes/grid/clothes")
OUTPUT_PATH = REPO_ROOT / "docs" / "restored-clothing-material-hints.md"


@dataclass(frozen=True)
class EntryRule:
    pattern: str
    bases: tuple[str, ...]


@dataclass(frozen=True)
class ClothingSource:
    category: str
    source_file: Path
    shape_rules: tuple[EntryRule, ...]
    texture_rules: tuple[EntryRule, ...]


@dataclass(frozen=True)
class RecipeIngredient:
    code_template: str
    count: int


@dataclass(frozen=True)
class RecipeInfo:
    output_template: str
    source_file: Path
    ingredients: tuple[RecipeIngredient, ...]


def extract_array_items(source_text: str, array_name: str) -> list[str]:
    match = re.search(rf"{array_name}\s*=\s*\[(.*?)\];", source_text, re.S)
    if not match:
        return []
    return re.findall(r'"([^"]+)"', match.group(1))


def extract_clothing_pool() -> list[str]:
    source_text = MOD_SOURCE.read_text(encoding="utf-8")
    items = extract_array_items(source_text, "RandomShelfClothingItems")
    return sorted(item for item in items if item.startswith("clothes-"))


def read_category_files() -> dict[str, ClothingSource]:
    result: dict[str, ClothingSource] = {}
    for path in sorted(VANILLA_ITEMTYPES.glob("*.json")):
        text = path.read_text(encoding="utf-8")
        category = extract_clothes_category(text)
        if category is None:
            continue

        shape_rules = tuple(parse_rules_from_section(text, "shapeByType"))
        if not shape_rules:
            top_level_shape = extract_top_level_shape_base(text)
            if top_level_shape is not None:
                shape_rules = (EntryRule("*", (top_level_shape,)),)

        texture_rules = tuple(parse_rules_from_section(text, "texturesByType"))
        result[category] = ClothingSource(category, path, shape_rules, texture_rules)

    return result


def extract_clothes_category(text: str) -> str | None:
    match = re.search(r'clothescategory\s*:\s*"([^"]+)"', text)
    return match.group(1) if match else None


def extract_top_level_shape_base(text: str) -> str | None:
    for pattern in [
        r'(?m)^\s*"shape"\s*:\s*\{\s*base\s*:\s*"([^"]+)"',
        r'(?m)^\s*shape\s*:\s*\{\s*base\s*:\s*"([^"]+)"',
    ]:
        match = re.search(pattern, text)
        if match:
            return match.group(1)
    return None


def parse_rules_from_section(text: str, section_name: str) -> list[EntryRule]:
    section = extract_named_object(text, section_name)
    if section is None:
        return []

    rules: list[EntryRule] = []
    for key, obj_text in iter_top_level_entries(section):
        bases = tuple(dict.fromkeys(re.findall(r'base\s*:\s*"([^"]+)"', obj_text)))
        if bases:
            rules.append(EntryRule(key, bases))
    return rules


def extract_named_object(text: str, name: str) -> str | None:
    match = re.search(rf"\b{name}\s*:\s*\{{", text)
    if not match:
        return None

    start = match.end() - 1
    end = find_matching_brace(text, start)
    return text[start + 1:end]


def find_matching_brace(text: str, start_index: int) -> int:
    depth = 0
    in_string = False
    escaped = False
    quote = ""
    for idx in range(start_index, len(text)):
        char = text[idx]
        if in_string:
            if escaped:
                escaped = False
            elif char == "\\":
                escaped = True
            elif char == quote:
                in_string = False
            continue

        if char in ('"', "'"):
            in_string = True
            quote = char
            continue

        if char == "{":
            depth += 1
        elif char == "}":
            depth -= 1
            if depth == 0:
                return idx

    raise ValueError("Unmatched brace")


def iter_top_level_entries(section_text: str) -> Iterable[tuple[str, str]]:
    idx = 0
    length = len(section_text)
    while idx < length:
        while idx < length and section_text[idx] in " \t\r\n,":
            idx += 1
        if idx >= length:
            break

        if section_text[idx] == '"':
            key_end = section_text.find('"', idx + 1)
            key = section_text[idx + 1:key_end]
            idx = key_end + 1
        else:
            key_match = re.match(r"([A-Za-z0-9@.*_+\-|()]+)\s*:", section_text[idx:])
            if not key_match:
                break
            key = key_match.group(1)
            idx += key_match.end() - 1

        colon_index = section_text.find(":", idx)
        if colon_index < 0:
            break
        idx = colon_index + 1

        while idx < length and section_text[idx].isspace():
            idx += 1
        if idx >= length or section_text[idx] != "{":
            continue

        obj_end = find_matching_brace(section_text, idx)
        yield key, section_text[idx:obj_end + 1]
        idx = obj_end + 1


def item_variant(item_code: str) -> tuple[str, str]:
    parts = item_code.split("-", 2)
    return parts[1], parts[2]


def pattern_matches_variant(pattern: str, variant: str) -> bool:
    pattern = pattern.strip().strip('"')
    if pattern == "*":
        return True
    if pattern.startswith("@") and "(" in pattern and ")" in pattern:
        inner = pattern[pattern.find("(") + 1:pattern.rfind(")")]
        return variant in [choice.strip() for choice in inner.split("|") if choice.strip()]
    if pattern.startswith("*-") and pattern.endswith("*"):
        return pattern[2:-1] in variant
    if pattern.startswith("*-"):
        return variant == pattern[2:]
    return pattern.endswith(variant)


def apply_variant(base: str, category: str, variant: str) -> str:
    return base.replace("{category}", category).replace(f"{{{category}}}", variant)


def resolve_matches(rules: Iterable[EntryRule], category: str, variant: str) -> list[str]:
    resolved: list[str] = []
    for rule in rules:
        if pattern_matches_variant(rule.pattern, variant):
            for base in rule.bases:
                resolved.append(apply_variant(base, category, variant))
    return list(dict.fromkeys(resolved))


def iter_brace_objects(text: str) -> Iterable[str]:
    idx = 0
    while idx < len(text):
        start = text.find("{", idx)
        if start < 0:
            break
        end = find_matching_brace(text, start)
        yield text[start:end + 1]
        idx = end + 1


def load_clothing_recipes() -> list[RecipeInfo]:
    recipes: list[RecipeInfo] = []
    for path in sorted(VANILLA_CLOTHES_RECIPES.glob("*.json")):
        text = path.read_text(encoding="utf-8")
        for obj in iter_brace_objects(text):
            output_match = re.search(r'output\s*:\s*\{[^}]*code\s*:\s*"([^"]+)"', obj, re.S)
            if not output_match:
                continue

            output_template = output_match.group(1)
            ingredients_text = extract_named_object(obj, "ingredients")
            if not ingredients_text:
                continue

            pattern_match = re.search(r'ingredientPattern\s*:\s*"([^"]+)"', obj)
            symbol_counts = Counter(ch for ch in pattern_match.group(1) if ch not in {",", "_"}) if pattern_match else Counter()

            ingredients: list[RecipeIngredient] = []
            for symbol, entry_text in iter_top_level_entries(ingredients_text):
                if re.search(r'\bisTool\s*:\s*true', entry_text) or re.search(r'\btool\s*:\s*true', entry_text):
                    continue
                code_match = re.search(r'code\s*:\s*"([^"]+)"', entry_text)
                if not code_match:
                    continue
                qty_match = re.search(r'quantity\s*:\s*(\d+)', entry_text)
                quantity = int(qty_match.group(1)) if qty_match else 1
                count = quantity * max(1, symbol_counts.get(symbol, 1))
                ingredients.append(RecipeIngredient(code_match.group(1), count))

            recipes.append(RecipeInfo(output_template, path, tuple(ingredients)))

    return recipes


def template_match(item_code: str, template: str) -> dict[str, str] | None:
    item_parts = item_code.split("-")
    template_parts = template.split("-")
    if len(item_parts) != len(template_parts):
        return None

    captures: dict[str, str] = {}
    for actual, expected in zip(item_parts, template_parts):
        if expected == "*":
            captures.setdefault("wildcard0", actual)
            continue
        if expected.startswith("{") and expected.endswith("}"):
            captures[expected[1:-1]] = actual
            continue
        if actual.lower() != expected.lower():
            return None
    return captures


def substitute_template(template: str, captures: dict[str, str]) -> str:
    result = template
    for key, value in captures.items():
        result = result.replace("{" + key + "}", value)
    if "*" in result and "wildcard0" in captures:
        result = result.replace("*", captures["wildcard0"])
    return result


def match_recipe(item_code: str, recipes: list[RecipeInfo]) -> tuple[RecipeInfo, list[str]] | None:
    for recipe in recipes:
        captures = template_match(item_code, recipe.output_template)
        if captures is None:
            continue
        ingredients = [f"{ing.count}x {substitute_template(ing.code_template, captures)}" for ing in recipe.ingredients]
        return recipe, ingredients
    return None


def infer_materials(item_code: str, recipe_ingredients: list[str], textures: list[str]) -> tuple[str, str, str]:
    text = " ".join([item_code, *recipe_ingredients, *textures]).lower()

    primary: list[str] = []
    accent: list[str] = []
    notes: list[str] = []

    def add_once(target: list[str], value: str) -> None:
        if value not in target:
            target.append(value)

    cloth_colors = [
        "black", "blue", "brown", "gray", "green", "orange", "pink",
        "plain", "purple", "red", "white", "yellow"
    ]
    for color in cloth_colors:
        if f"cloth-{color}" in text or f"linen/{color}" in text or f"kirtle{color}" in text or f"apron{color}" in text:
            add_once(primary, f"{color} cloth")

    if "cloth-" in text and not primary:
        add_once(primary, "cloth")
    if "linen" in text:
        add_once(primary, "linen/cloth")
    if "hide" in text or "rawhide" in text:
        add_once(primary, "hide")
    if "leather" in text:
        add_once(primary, "leather")
    if "fur" in text or "pelt" in text:
        add_once(primary, "fur/pelt")
    if "flaxtwine" in text or "linen-rope" in text:
        add_once(accent, "flax twine")
    if "drygrass" in text or "grass" in text or "straw" in text:
        add_once(primary, "dry grass/straw")
    if "bamboo" in text:
        add_once(primary, "bamboo")

    if any(token in text for token in ["shirt", "tunic", "blouse", "robe", "gown", "pants", "breeches", "leggings", "braies", "hose", "scarf", "sash", "wimple", "apron", "cape", "poncho", "coat", "hood", "hat"]):
        add_once(primary, "cloth")
    if any(token in text for token in ["gloves", "wristguard", "boots", "shoes", "slippers", "sandals", "belt", "strap", "waistband", "wrap"]):
        add_once(primary, "leather/cloth mix")
    if "patchwork" in text:
        add_once(primary, "mixed cloth")
    if any(token in text for token in ["furh", "fur-", "ruralhunter", "hunter", "arctic", "reindeer"]):
        add_once(accent, "fur/leather trim")

    if "gold" in text:
        add_once(accent, "gold")
    if "silver" in text:
        add_once(accent, "silver")
    if any(token in text for token in ["bronze", "iron", "steel", "metalcap", "bronzebelt", "chain"]):
        add_once(accent, "metal trim/hardware")

    if "clockmaker" in text:
        notes.append("clockmaker piece; likely more tailored/fitted than generic cloth")
    if "apron" in text and "leather" not in text:
        notes.append("apron silhouette; cloth or linen is likely the base material")
    if "belt" in text or "strap" in text or "waistband" in text:
        notes.append("belt-like piece; leather is a strong candidate unless the art suggests woven cloth")
    if any(token in text for token in ["popinjay", "midsummer"]):
        notes.append("decorative outfit piece; bright dyed cloth is a likely restore base")
    if not recipe_ingredients:
        notes.append("not directly craftable from vanilla clothes recipes; material is inferred from name/texture only")

    return (
        ", ".join(primary) if primary else "unknown",
        ", ".join(accent) if accent else "none obvious",
        "; ".join(notes) if notes else "",
    )


def build_report() -> str:
    pool = extract_clothing_pool()
    categories = read_category_files()
    recipes = load_clothing_recipes()

    lines = [
        "# Restored Clothing Material Hints",
        "",
        "Generated from the restored clothing crate pool in `RandomShelfClothingItems`, the vanilla clothes recipe files, and the wearable texture/shape definitions.",
        "",
        "Columns:",
        "- `Craftable`: whether the restored item matches a vanilla clothes grid recipe",
        "- `Recipe materials`: exact vanilla recipe inputs when matched",
        "- `Inferred primary`: best guess at the base cloth/hide/leather/fur material",
        "- `Inferred accent`: likely trim or special material like twine or gold",
        "",
    ]

    current_category = None
    for item_code in pool:
        category, variant = item_variant(item_code)
        if category != current_category:
            if current_category is not None:
                lines.append("")
            lines.append(f"## {category}")
            lines.append("")
            current_category = category

        source = categories.get(category)
        textures = resolve_matches(source.texture_rules, category, variant) if source else []
        shapes = resolve_matches(source.shape_rules, category, variant) if source else []
        matched_recipe = match_recipe(item_code, recipes)
        recipe_materials = matched_recipe[1] if matched_recipe else []
        primary, accent, notes = infer_materials(item_code, recipe_materials, textures + shapes)

        lines.append(f"### `{item_code}`")
        lines.append("")
        lines.append(f"- Craftable: `{'yes' if matched_recipe else 'no'}`")
        if matched_recipe:
            lines.append(f"- Recipe source: `{matched_recipe[0].source_file}`")
            lines.append(f"- Recipe materials: {', '.join(f'`{entry}`' for entry in recipe_materials)}")
        else:
            lines.append("- Recipe materials: none matched in vanilla clothes recipes")
        lines.append(f"- Inferred primary: `{primary}`")
        lines.append(f"- Inferred accent: `{accent}`")
        if textures:
            lines.append(f"- Texture base(s): {', '.join(f'`{value}`' for value in textures)}")
        if shapes:
            lines.append(f"- Shape base(s): {', '.join(f'`{value}`' for value in shapes)}")
        if notes:
            lines.append(f"- Notes: {notes}")
        lines.append("")

    return "\n".join(lines).rstrip() + "\n"


def main() -> None:
    OUTPUT_PATH.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT_PATH.write_text(build_report(), encoding="utf-8")
    print(f"Wrote {OUTPUT_PATH}")


if __name__ == "__main__":
    main()
