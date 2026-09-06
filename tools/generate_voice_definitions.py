#!/usr/bin/env python3
"""Generates voice_definitions.xml from a voice event JSON map."""

import json
import sys
from pathlib import Path

JSON_PATH = Path("/home/harry/.claude/projects/-mnt-c-users-harry-source-repos-namidaka-crpg/d44b1af4-8d96-43a9-9c19-b7dcf35309f7/tool-results/bobnnvngp.txt")
XML_PATH = Path("/mnt/c/users/harry/source/repos/namidaka/crpg/src/Module.Server/ModuleData/voice_definitions.xml")


def to_pascal(parts):
    return "".join(word.capitalize() for part in parts for word in part.split("_"))


def type_name(key):
    """voice/crpg/attack/male/archers -> AttackArchers"""
    parts = key.split("/")
    gender_idx = next(i for i, p in enumerate(parts) if p in ("male", "female"))
    category = parts[2:gender_idx]
    subcat = parts[gender_idx + 1:]
    return to_pascal(category + subcat)


def fmod(key, gender):
    parts = key.split("/")
    gender_idx = next(i for i, p in enumerate(parts) if p in ("male", "female"))
    parts[gender_idx] = gender
    return "event:/" + "/".join(parts)


def voice_block(vtype, path, anim="grunt"):
    return f'    <voice type="{vtype}" path="{path}" face_anim="{anim}" />'


def definition(name, voices):
    lines = [
        f'  <voice_definition',
        f'    name="{name}"',
        f'    sound_and_collision_info_class="human"',
        f'    only_for_npcs="false"',
        f'    min_pitch_multiplier="0.9"',
        f'    max_pitch_multiplier="1.1">',
    ] + voices + ["  </voice_definition>", ""]
    return "\n".join(lines)


data = json.loads(JSON_PATH.read_text())

# Collect unique (type_name, male_path, female_path) from male keys only
seen = {}
for key in data:
    parts = key.split("/")
    gender_idx = next((i for i, p in enumerate(parts) if p in ("male", "female")), None)
    if gender_idx is None or parts[gender_idx] != "male":
        continue
    vtype = type_name(key)
    if vtype not in seen:
        seen[vtype] = (fmod(key, "male"), fmod(key, "female"))

voice_types = sorted(seen.keys())

# Build voice lines per gender
whistle_m = '    <voice type="Whistle" path="event:/voice/crpg/male/whistle" face_anim="grunt" />'
whistle_f = '    <voice type="Whistle" path="event:/voice/crpg/male/whistle" face_anim="grunt" />'

male_voices = [whistle_m] + [voice_block(vt, seen[vt][0]) for vt in voice_types]
female_voices = [whistle_f] + [voice_block(vt, seen[vt][1]) for vt in voice_types]

# Declarations
decl_lines = ["  <voice_type_declarations>", '    <voice_type name="Whistle" />']
for vt in voice_types:
    decl_lines.append(f'    <voice_type name="{vt}" />')
decl_lines.append("  </voice_type_declarations>")

# Definitions
defs = []
for i in range(1, 9):
    defs.append(definition(f"male_{i:02d}", male_voices))
for i in range(1, 6):
    defs.append(definition(f"female_{i:02d}", female_voices))

xml = "\n".join(
    ["<voice_definitions>", "\n".join(decl_lines), ""]
    + defs
    + ["</voice_definitions>", ""]
)

XML_PATH.write_text(xml, encoding="utf-8")
print(f"Written {len(voice_types)+1} voice types, {8} male + {5} female definitions to {XML_PATH}")
