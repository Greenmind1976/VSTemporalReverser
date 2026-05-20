#!/usr/bin/env python3

from __future__ import annotations

import re
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable


REPO_ROOT = Path("/Users/garretcoffman/Documents/VSMods/VSTemporalReverser")
MOD_SOURCE = REPO_ROOT / "VSTemporalReverser" / "ItemTemporalReverser.cs"
VANILLA_ITEMTYPES = Path("/Applications/Vintage Story 1.22.app/assets/survival/itemtypes/wearable/seraph")
OUTPUT_PATH = REPO_ROOT / "docs" / "restored-clothing-textures.md"


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

        result[category] = ClothingSource(
            category=category,
            source_file=path,
            shape_rules=shape_rules,
            texture_rules=texture_rules,
        )

    return result


def extract_clothes_category(text: str) -> str | None:
    match = re.search(r'clothescategory\s*:\s*"([^"]+)"', text)
    return match.group(1) if match else None


def extract_top_level_shape_base(text: str) -> str | None:
    match = re.search(r'(?m)^\s*"shape"\s*:\s*\{\s*base\s*:\s*"([^"]+)"', text)
    if match:
        return match.group(1)

    match = re.search(r'(?m)^\s*shape\s*:\s*\{\s*base\s*:\s*"([^"]+)"', text)
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

    raise ValueError("Unmatched brace while parsing clothing asset section")


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
    if len(parts) < 3 or parts[0] != "clothes":
        raise ValueError(f"Unexpected clothing item code: {item_code}")
    return parts[1], parts[2]


def pattern_matches_variant(pattern: str, variant: str) -> bool:
    pattern = pattern.strip()
    if pattern == "*":
        return True

    if pattern.startswith('"') and pattern.endswith('"'):
        pattern = pattern[1:-1]

    if pattern.startswith("@") and "(" in pattern and ")" in pattern:
        inner = pattern[pattern.find("(") + 1:pattern.rfind(")")]
        choices = [choice.strip() for choice in inner.split("|") if choice.strip()]
        return variant in choices

    if pattern.startswith("*-") and pattern.endswith("*"):
        return pattern[2:-1] in variant

    if pattern.startswith("*-"):
        return variant == pattern[2:]

    if "{":
        return True

    return pattern.endswith(variant)


def apply_variant(base: str, category: str, variant: str) -> str:
    replacements = {
        "{category}": category,
        f"{{{category}}}": variant,
    }
    for placeholder, value in replacements.items():
        base = base.replace(placeholder, value)
    return base


def resolve_matches(rules: Iterable[EntryRule], category: str, variant: str) -> list[str]:
    resolved: list[str] = []
    for rule in rules:
        if not pattern_matches_variant(rule.pattern, variant):
            continue
        for base in rule.bases:
            resolved.append(apply_variant(base, category, variant))

    return list(dict.fromkeys(resolved))


def build_report() -> str:
    pool = extract_clothing_pool()
    categories = read_category_files()

    lines: list[str] = []
    lines.append("# Restored Clothing Texture Report")
    lines.append("")
    lines.append("Generated from the restored clothing crate pool in `RandomShelfClothingItems` and the vanilla wearable itemtype definitions.")
    lines.append("")

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
        if source is None:
            lines.append(f"### `{item_code}`")
            lines.append("")
            lines.append("- Source itemtype: not found")
            lines.append("")
            continue

        shapes = resolve_matches(source.shape_rules, category, variant)
        textures = resolve_matches(source.texture_rules, category, variant)

        lines.append(f"### `{item_code}`")
        lines.append("")
        lines.append(f"- Source itemtype: `{source.source_file}`")
        lines.append(f"- Variant: `{variant}`")
        if shapes:
            lines.append(f"- Shape base(s): {', '.join(f'`{shape}`' for shape in shapes)}")
        else:
            lines.append("- Shape base(s): none detected")
        if textures:
            lines.append(f"- Texture base(s): {', '.join(f'`{texture}`' for texture in textures)}")
        else:
            lines.append("- Texture base(s): none detected")
        lines.append("")

    return "\n".join(lines).rstrip() + "\n"


def main() -> None:
    OUTPUT_PATH.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT_PATH.write_text(build_report(), encoding="utf-8")
    print(f"Wrote {OUTPUT_PATH}")


if __name__ == "__main__":
    main()
