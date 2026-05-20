#!/usr/bin/env python3

from __future__ import annotations

import math
import re
import subprocess
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable


REPO_ROOT = Path("/Users/garretcoffman/Documents/VSMods/VSTemporalReverser")
MOD_SOURCE = REPO_ROOT / "VSTemporalReverser" / "ItemTemporalReverser.cs"
VANILLA_ITEMTYPES = Path("/Applications/Vintage Story 1.22.app/assets/survival/itemtypes/wearable/seraph")
VANILLA_TEXTURE_ROOT = Path("/Applications/Vintage Story 1.22.app/assets/survival/textures")
LINEN_ROOT = VANILLA_TEXTURE_ROOT / "block/cloth/linen"
OUTPUT_PATH = REPO_ROOT / "docs" / "restored-clothing-color-analysis.md"


@dataclass(frozen=True)
class EntryRule:
    pattern: str
    bases: tuple[str, ...]


@dataclass(frozen=True)
class ClothingSource:
    category: str
    source_file: Path
    texture_rules: tuple[EntryRule, ...]


def run_magick_histogram(image_path: Path) -> list[tuple[int, int, int, int, int]]:
    result = subprocess.run(
        ["magick", str(image_path), "-format", "%c", "histogram:info:-"],
        check=True,
        text=True,
        capture_output=True,
    )
    rows: list[tuple[int, int, int, int, int]] = []
    pattern = re.compile(r"\s*(\d+): \((\d+),(\d+),(\d+),(\d+)\)")
    for line in result.stdout.splitlines():
        match = pattern.match(line)
        if not match:
            continue
        count, r, g, b, a = map(int, match.groups())
        rows.append((count, r, g, b, a))
    return rows


def extract_array_items(source_text: str, array_name: str) -> list[str]:
    match = re.search(rf"{array_name}\s*=\s*\[(.*?)\];", source_text, re.S)
    if not match:
        return []
    return re.findall(r'"([^"]+)"', match.group(1))


def extract_clothing_pool() -> list[str]:
    source_text = MOD_SOURCE.read_text(encoding="utf-8")
    items = extract_array_items(source_text, "RandomShelfClothingItems")
    return sorted(item for item in items if item.startswith("clothes-"))


def extract_clothes_category(text: str) -> str | None:
    match = re.search(r'clothescategory\s*:\s*"([^"]+)"', text)
    return match.group(1) if match else None


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


def parse_texture_rules(text: str) -> tuple[EntryRule, ...]:
    section = extract_named_object(text, "texturesByType")
    if section is None:
        return ()
    rules: list[EntryRule] = []
    for key, obj_text in iter_top_level_entries(section):
        bases = tuple(dict.fromkeys(re.findall(r'base\s*:\s*"([^"]+)"', obj_text)))
        if bases:
            rules.append(EntryRule(key, bases))
    return tuple(rules)


def read_category_files() -> dict[str, ClothingSource]:
    result: dict[str, ClothingSource] = {}
    for path in sorted(VANILLA_ITEMTYPES.glob("*.json")):
        text = path.read_text(encoding="utf-8")
        category = extract_clothes_category(text)
        if category is None:
            continue
        result[category] = ClothingSource(
            category=category,
            source_file=path,
            texture_rules=parse_texture_rules(text),
        )
    return result


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


def resolve_texture_paths(source: ClothingSource | None, category: str, variant: str) -> list[Path]:
    if source is None:
        return []
    resolved: list[Path] = []
    for rule in source.texture_rules:
        if not pattern_matches_variant(rule.pattern, variant):
            continue
        for base in rule.bases:
            if base == "block/transparent":
                continue
            texture_rel = apply_variant(base, category, variant) + ".png"
            path = VANILLA_TEXTURE_ROOT / texture_rel
            if path.exists():
                resolved.append(path)
    unique: list[Path] = []
    seen: set[Path] = set()
    for path in resolved:
        if path not in seen:
            seen.add(path)
            unique.append(path)
    return unique


def weighted_average_rgb(rows: list[tuple[int, int, int, int, int]]) -> tuple[float, float, float]:
    total = 0.0
    r_sum = g_sum = b_sum = 0.0
    for count, r, g, b, a in rows:
        if a < 10:
            continue
        weight = count * (a / 255.0)
        total += weight
        r_sum += r * weight
        g_sum += g * weight
        b_sum += b * weight
    if total <= 0:
        return (0.0, 0.0, 0.0)
    return (r_sum / total, g_sum / total, b_sum / total)


def cloth_reference_colors() -> dict[str, tuple[float, float, float]]:
    refs: dict[str, tuple[float, float, float]] = {}
    for color in ["plain", "blue", "red", "yellow", "green", "purple", "pink", "orange", "brown", "gray", "black", "white"]:
        file_name = "normal1.png" if color == "plain" else f"{color}.png"
        rows = run_magick_histogram(LINEN_ROOT / file_name)
        refs[color] = weighted_average_rgb(rows)
    return refs


def rgb_distance(a: tuple[float, float, float], b: tuple[float, float, float]) -> float:
    return math.sqrt(sum((x - y) ** 2 for x, y in zip(a, b)))


def classify_pixels(rows: list[tuple[int, int, int, int, int]], refs: dict[str, tuple[float, float, float]]) -> dict[str, float]:
    totals = {color: 0.0 for color in refs}
    for count, r, g, b, a in rows:
        if a < 10:
            continue
        rgb = (float(r), float(g), float(b))
        nearest = min(refs.items(), key=lambda kv: rgb_distance(rgb, kv[1]))[0]
        totals[nearest] += count * (a / 255.0)
    grand_total = sum(totals.values()) or 1.0
    return {color: weight / grand_total for color, weight in totals.items() if weight > 0}


def combine_texture_classifications(texture_paths: list[Path], refs: dict[str, tuple[float, float, float]]) -> dict[str, float]:
    combined = {color: 0.0 for color in refs}
    used = 0
    for path in texture_paths:
        rows = run_magick_histogram(path)
        breakdown = classify_pixels(rows, refs)
        if not breakdown:
            continue
        used += 1
        for color, share in breakdown.items():
            combined[color] += share
    if used == 0:
        return {}
    return {color: weight / used for color, weight in combined.items() if weight > 0}


def recommendation(sorted_colors: list[tuple[str, float]]) -> str:
    if not sorted_colors:
        return "no usable texture colors found"
    if len(sorted_colors) == 1 or sorted_colors[1][1] < 0.22:
        return f"`2x cloth-{sorted_colors[0][0]}`"
    return f"`1x cloth-{sorted_colors[0][0]}` + `1x cloth-{sorted_colors[1][0]}`"


def build_report() -> str:
    pool = extract_clothing_pool()
    sources = read_category_files()
    refs = cloth_reference_colors()

    lines = [
        "# Restored Clothing Color Analysis",
        "",
        "This report maps restored clothing crate textures to the nearest vanilla linen cloth colors by pixel comparison.",
        "",
        "Method:",
        "- resolve the actual wearable texture PNG(s) used by the restored clothing piece",
        "- compare each non-transparent pixel to the vanilla linen swatches",
        "- total the closest cloth-color matches",
        "",
        "This is a first-pass art heuristic, not a lore-perfect material parser.",
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

        texture_paths = resolve_texture_paths(sources.get(category), category, variant)
        breakdown = combine_texture_classifications(texture_paths, refs)
        sorted_colors = sorted(breakdown.items(), key=lambda kv: kv[1], reverse=True)

        lines.append(f"### `{item_code}`")
        lines.append("")
        if not texture_paths:
            lines.append("- Texture analysis: no direct cloth texture path resolved")
            lines.append("")
            continue

        lines.append(f"- Texture file(s): {', '.join(f'`{p}`' for p in texture_paths)}")
        if sorted_colors:
            top = ", ".join(f"`{color}` {share * 100:.1f}%" for color, share in sorted_colors[:4])
            lines.append(f"- Closest cloth colors: {top}")
            lines.append(f"- Suggested restore cloth: {recommendation(sorted_colors)}")
        else:
            lines.append("- Closest cloth colors: none")
        lines.append("")

    return "\n".join(lines).rstrip() + "\n"


def main() -> None:
    OUTPUT_PATH.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT_PATH.write_text(build_report(), encoding="utf-8")
    print(f"Wrote {OUTPUT_PATH}")


if __name__ == "__main__":
    main()
