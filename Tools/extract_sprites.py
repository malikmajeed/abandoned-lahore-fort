#!/usr/bin/env python3
"""Extract sprites from The Forgotten Fort spritesheet (48px grid)."""
from PIL import Image
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1] / "Assets" / "Sprites"
SHEET = ROOT / "spritesheet.png"
CELL = 48


def crop(img, row, col, w=1, h=1):
    x, y = col * CELL, row * CELL
    return img.crop((x, y, x + w * CELL, y + h * CELL))


def save(img, path):
    path.parent.mkdir(parents=True, exist_ok=True)
    img.save(path)


def extract():
    img = Image.open(SHEET).convert("RGBA")
    chars = ROOT / "Characters"
    tiles = ROOT / "Tiles"
    objs = ROOT / "Objects"
    ui = ROOT / "UI"

    # Player walk: row2=down, row3=up, row5=left, row4 cols1-3=right
    player_rows = {"down": 2, "up": 3, "left": 5, "right": 4}
    for direction, row in player_rows.items():
        start_col = 0 if direction != "right" else 1
        for f in range(4):
            save(crop(img, row, start_col + f), chars / f"player_walk_{direction}_{f}.png")
            if f == 0:
                save(crop(img, row, start_col), chars / f"player_idle_{direction}.png")

    # Guard: row4 col0=idle down, row5 col0?, rows 6-9 for other anims
    guard_idle = {"down": (4, 0), "up": (6, 0), "left": (7, 0), "right": (6, 1)}
    for direction, (row, col) in guard_idle.items():
        save(crop(img, row, col), chars / f"guard_idle_{direction}.png")

    guard_walk_rows = {"down": 8, "up": 9, "left": 10, "right": 8}
    for direction, row in guard_walk_rows.items():
        start_col = 0 if direction != "right" else 1
        if row >= img.height // CELL:
            continue
        for f in range(4):
            col = start_col + f
            if col >= img.width // CELL:
                continue
            save(crop(img, row, col), chars / f"guard_walk_{direction}_{f}.png")

    # Guard special poses row 11
    if img.height > 11 * CELL:
        save(crop(img, 11, 0), chars / "guard_suspicious_left.png")
        save(crop(img, 11, 1), chars / "guard_suspicious_right.png")
        save(crop(img, 11, 2), chars / "guard_chase.png")
        save(crop(img, 11, 3), chars / "guard_triumph.png")

    # Environment tiles - right side cols 10-20
    for i in range(6):
        save(crop(img, 2, 10 + i), tiles / f"floor_{i}.png")
    for i in range(8):
        row, col = divmod(i, 4)
        save(crop(img, 3 + row, 10 + col), tiles / f"wall_{i}.png")
    for i in range(4):
        save(crop(img, 5, 10 + i, w=1, h=2), tiles / f"jharokha_{i}.png")

    # Objects row 7-9 cols 10+
    save(crop(img, 7, 10), objs / "key_roshanai.png")
    save(crop(img, 7, 11), objs / "key_akbari.png")
    save(crop(img, 7, 12), objs / "key_hazuri.png")
    save(crop(img, 7, 13), objs / "key_generic.png")
    save(crop(img, 8, 10), objs / "chest_closed.png")
    save(crop(img, 8, 11), objs / "chest_open.png")
    save(crop(img, 8, 12), objs / "chest_royal.png")
    for i in range(4):
        save(crop(img, 9, 10 + i), objs / f"torch_{i}.png")
    save(crop(img, 10, 10), objs / "door_closed.png")
    save(crop(img, 10, 11), objs / "door_locked_gold.png")
    save(crop(img, 10, 12), objs / "door_locked_green.png")
    save(crop(img, 10, 13), objs / "door_open.png")
    for i in range(3):
        save(crop(img, 11, 10 + i), objs / f"barrel_{i}.png")
    for i in range(3):
        save(crop(img, 12, 10 + i), objs / f"mosaic_{i}.png")

    # UI icons row 8 cols 18+
    save(crop(img, 8, 18), ui / "heart_full.png")
    save(crop(img, 8, 19), ui / "heart_empty.png")
    save(crop(img, 9, 18), ui / "icon_key.png")
    save(crop(img, 9, 19), ui / "icon_chest.png")

    print("Sprites extracted with 48px grid")


if __name__ == "__main__":
    extract()
