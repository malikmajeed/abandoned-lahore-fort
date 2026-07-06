#!/usr/bin/env python3
"""Generate expanded maze-style level.json with rooms + winding corridors."""
import json
from pathlib import Path

def rect_path(x1, y1, x2, y2):
    pts = []
    x, y = x1, y1
    pts.append([x, y])
    while x != x2:
        x += 1 if x2 > x else -1
        pts.append([x, y])
    while y != y2:
        y += 1 if y2 > y else -1
        pts.append([x, y])
    return pts

def merge_path(paths):
    out = []
    seen = set()
    for p in paths:
        for pt in p:
            t = tuple(pt)
            if t not in seen:
                seen.add(t)
                out.append(list(t))
    return out

# --- layout ---
LEVEL = {
    "tileSize": 64,
    "gridWidth": 42,
    "gridHeight": 50,
    "start": {"x": 7, "y": 7, "room": "entry_hall"},
    "rooms": [
        {"id": "entry_hall", "name": "Entry Hall", "x": 4, "y": 4, "w": 7, "h": 6, "guard": False},
        {"id": "guard_room", "name": "Guard Room", "x": 3, "y": 14, "w": 9, "h": 8, "guard": True},
        {"id": "key_vault_a", "name": "Key Vault A", "x": 18, "y": 12, "w": 5, "h": 5, "guard": False},
        {"id": "locked_room", "name": "Locked Room", "x": 3, "y": 26, "w": 9, "h": 7, "guard": False},
        {"id": "hall_b", "name": "Hall B", "x": 17, "y": 26, "w": 9, "h": 7, "guard": True},
        {"id": "key_vault_b", "name": "Key Vault B", "x": 29, "y": 26, "w": 7, "h": 7, "guard": False},
        {"id": "puzzle_room", "name": "Puzzle Room", "x": 16, "y": 37, "w": 11, "h": 8, "guard": False},
    ],
    "doors": [
        {"id": "D1", "x": 7, "y": 10, "orientation": "vertical", "locked": False, "keyId": None,
         "leadsFrom": "entry_hall", "leadsTo": "guard_room"},
        {"id": "D2", "x": 17, "y": 16, "orientation": "horizontal", "locked": False, "keyId": None,
         "leadsFrom": "guard_room", "leadsTo": "key_vault_a"},
        {"id": "D3", "x": 7, "y": 22, "orientation": "vertical", "locked": True, "keyId": "K1",
         "leadsFrom": "guard_room", "leadsTo": "locked_room"},
        {"id": "D4", "x": 16, "y": 29, "orientation": "horizontal", "locked": False, "keyId": None,
         "leadsFrom": "locked_room", "leadsTo": "hall_b"},
        {"id": "D5", "x": 28, "y": 29, "orientation": "horizontal", "locked": False, "keyId": None,
         "leadsFrom": "key_vault_b", "leadsTo": "hall_b"},
        {"id": "D6", "x": 21, "y": 33, "orientation": "vertical", "locked": True, "keyId": "K2",
         "leadsFrom": "hall_b", "leadsTo": "puzzle_room"},
    ],
    "keys": [
        {"id": "K1", "unlocks": "D3", "room": "key_vault_a", "x": 20, "y": 14},
        {"id": "K2", "unlocks": "D6", "room": "key_vault_b", "x": 32, "y": 29},
    ],
    "chests": [
        {"id": "C1", "room": "puzzle_room", "x": 21, "y": 43, "loot": "final_treasure", "requiresPuzzle": True},
    ],
    "guards": [
        {"id": "G1", "room": "guard_room", "x": 7, "y": 18,
         "patrol": [[5, 16], [9, 16], [9, 20], [5, 20]]},
        {"id": "G2", "room": "hall_b", "x": 21, "y": 29,
         "patrol": [[19, 28], [23, 28], [23, 31], [19, 31]]},
    ],
    "puzzle": {"id": "P1", "room": "puzzle_room", "x": 21, "y": 40, "type": "tile_lock_sequence", "unlocksChest": "C1"},
    "decor_barrels": [
        {"x": 9, "y": 5, "room": "entry_hall"},
        {"x": 5, "y": 16, "room": "guard_room"},
        {"x": 5, "y": 30, "room": "locked_room"},
        {"x": 23, "y": 30, "room": "hall_b"},
        {"x": 18, "y": 39, "room": "puzzle_room"},
        {"x": 24, "y": 39, "room": "puzzle_room"},
        {"x": 14, "y": 21, "room": "corridor_east"},
        {"x": 14, "y": 36, "room": "corridor_south"},
        {"x": 35, "y": 36, "room": "corridor_far_east"},
    ],
    "exits": [{"id": "exit_gate", "x": 21, "y": 44, "leadsTo": "outside_win"}],
}

# Winding corridor network between rooms (1 tile wide paths)
corridor_paths = [
    rect_path(7, 11, 7, 14),           # entry -> guard (via D1)
    rect_path(7, 17, 16, 17),           # guard east spine
    rect_path(16, 17, 16, 16),          # turn up toward D2
    rect_path(7, 22, 7, 26),           # guard -> locked (via D3)
    rect_path(7, 29, 16, 29),          # locked -> hall connector (via D4)
    rect_path(28, 29, 26, 29),          # vault b -> hall (via D5)
    rect_path(21, 33, 21, 37),          # hall -> puzzle (via D6)
    # maze branches / dead ends
    rect_path(11, 17, 11, 21),
    rect_path(11, 21, 14, 21),
    rect_path(14, 21, 14, 18),
    rect_path(11, 29, 11, 33),
    rect_path(11, 33, 14, 33),
    rect_path(14, 33, 14, 36),
    rect_path(26, 29, 26, 33),
    rect_path(26, 33, 29, 33),
    rect_path(29, 33, 29, 36),
    rect_path(32, 29, 32, 33),
    rect_path(32, 33, 35, 33),
    rect_path(35, 33, 35, 36),
    rect_path(21, 37, 21, 44),         # puzzle room spine to exit
    rect_path(18, 40, 24, 40),          # puzzle room cross aisle
    rect_path(18, 37, 18, 40),
    rect_path(24, 37, 24, 40),
]

merged = merge_path(corridor_paths)
LEVEL["corridors"] = [{"id": "main_maze", "pathX": [p[0] for p in merged], "pathY": [p[1] for p in merged]}]

# Block shortcuts to force longer maze routes (never on doors/keys/chest/start/exit/puzzle)
protected = {
    (LEVEL["start"]["x"], LEVEL["start"]["y"]),
    (LEVEL["puzzle"]["x"], LEVEL["puzzle"]["y"]),
    (LEVEL["chests"][0]["x"], LEVEL["chests"][0]["y"]),
    (LEVEL["exits"][0]["x"], LEVEL["exits"][0]["y"]),
}
for d in LEVEL["doors"]:
    protected.add((d["x"], d["y"]))
for k in LEVEL["keys"]:
    protected.add((k["x"], k["y"]))

maze_blocks = [
    [12, 19], [13, 19], [12, 30], [13, 30],
    [16, 21], [17, 21], [16, 34], [17, 34],
    [27, 31], [28, 31], [33, 34], [34, 34],
    [10, 18], [10, 19], [10, 32],
]
LEVEL["mazeBlocks"] = [{"x": x, "y": y} for x, y in maze_blocks if (x, y) not in protected]

out = Path(__file__).resolve().parent.parent / "Assets" / "Resources" / "level.json"
out.write_text(json.dumps(LEVEL, indent=2) + "\n")
print(f"Wrote {out} ({LEVEL['gridWidth']}x{LEVEL['gridHeight']}, {len(merged)} corridor tiles, {len(LEVEL['mazeBlocks'])} maze blocks)")
