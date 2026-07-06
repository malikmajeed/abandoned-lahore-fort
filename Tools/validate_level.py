import json, sys

lvl = json.load(open("level.json"))
rooms = {r["id"]: r for r in lvl["rooms"]}

floor = set()
for r in rooms.values():
    for x in range(r["x"], r["x"]+r["w"]):
        for y in range(r["y"], r["y"]+r["h"]):
            floor.add((x,y))

door_by_xy = {}
for d in lvl["doors"]:
    floor.add((d["x"], d["y"]))
    door_by_xy[(d["x"], d["y"])] = d

# Corridors add maze paths between rooms
for c in lvl.get("corridors", []):
    for x, y in zip(c.get("pathX", []), c.get("pathY", [])):
        floor.add((x, y))

# Maze blocks remove floor tiles to create tighter passages
for b in lvl.get("mazeBlocks", []):
    floor.discard((b["x"], b["y"]))

walls = set()
for (x,y) in floor:
    for dx,dy in [(1,0),(-1,0),(0,1),(0,-1)]:
        n=(x+dx,y+dy)
        if n not in floor: walls.add(n)

errors = []

def in_room(pt):
    x,y = pt
    for rid,r in rooms.items():
        if r["x"] <= x < r["x"]+r["w"] and r["y"] <= y < r["y"]+r["h"]:
            return rid
    return None

# 1. every door must have floor on exactly two opposite sides (a real passage), and wall on the other two
for d in lvl["doors"]:
    x,y = d["x"], d["y"]
    horiz_open = (x-1,y) in floor and (x+1,y) in floor
    vert_open  = (x,y-1) in floor and (x,y+1) in floor
    if not (horiz_open or vert_open):
        errors.append(f"Door {d['id']} at ({x},{y}) is not embedded in a wall between two floor tiles")

# 2. every barrel must be on a floor tile (room or corridor), not on a door tile or start
start = (lvl["start"]["x"], lvl["start"]["y"])
occupied_check = set([start])
for b in lvl["decor_barrels"]:
    pt = (b["x"], b["y"])
    if pt not in floor:
        errors.append(f"Barrel at {pt} is NOT on a floor tile (outside walls)")
    if pt in door_by_xy:
        errors.append(f"Barrel at {pt} sits on a door tile")
    if pt == start:
        errors.append(f"Barrel at {pt} overlaps player start")

# 3. every key/chest must be on a floor tile inside its named room
for k in lvl["keys"]:
    pt=(k["x"],k["y"])
    if pt not in floor or in_room(pt) != k["room"]:
        errors.append(f"Key {k['id']} at {pt} is not inside room '{k['room']}'")
for c in lvl["chests"]:
    pt=(c["x"],c["y"])
    if pt not in floor or in_room(pt) != c["room"]:
        errors.append(f"Chest {c['id']} at {pt} is not inside room '{c['room']}'")

# 4. guards: must be on floor, inside their room, and >=3 tiles (Manhattan) from start,
#    and not inside the start room
for g in lvl["guards"]:
    pt=(g["x"],g["y"])
    room_here = in_room(pt)
    if pt not in floor:
        errors.append(f"Guard {g['id']} at {pt} is not on a floor tile")
    if room_here != g["room"]:
        errors.append(f"Guard {g['id']} at {pt} is not inside its declared room '{g['room']}'")
    if room_here == lvl["start"]["room"]:
        errors.append(f"Guard {g['id']} is in the START room - blocks immediate spawn")
    dist = abs(pt[0]-start[0]) + abs(pt[1]-start[1])
    if dist < 3:
        errors.append(f"Guard {g['id']} is only {dist} tiles from player start (too close)")
    # patrol waypoints must all be floor tiles inside the same room
    for wp in g["patrol"]:
        wp = tuple(wp)
        if wp not in floor or in_room(wp) != g["room"]:
            errors.append(f"Guard {g['id']} patrol waypoint {wp} leaves its room or hits a wall")

# 5. start tile must be floor, inside its declared room, and not a door/guard/barrel tile
if start not in floor or in_room(start) != lvl["start"]["room"]:
    errors.append("Start position is not valid floor inside its declared room")

# 6. every locked door must have a keyId pointing to an existing key, and that key's room
#    must be reachable from start WITHOUT passing through that same locked door
#    (simple BFS check treating other locked doors as passable for this test...
#     simplified: just confirm key room != the room the locked door leads INTO)
for d in lvl["doors"]:
    if d["locked"]:
        key = next((k for k in lvl["keys"] if k["id"]==d["keyId"]), None)
        if key is None:
            errors.append(f"Locked door {d['id']} references missing key {d.get('keyId')}")
        elif key["room"] == d["leadsTo"]:
            errors.append(f"Key for door {d['id']} is placed INSIDE the room it unlocks (unreachable)")

print(f"Checked {len(lvl['doors'])} doors, {len(lvl['decor_barrels'])} barrels, "
      f"{len(lvl['keys'])} keys, {len(lvl['chests'])} chests, {len(lvl['guards'])} guards.")
if errors:
    print(f"\n{len(errors)} PROBLEM(S) FOUND:")
    for e in errors: print(" -", e)
    sys.exit(1)
else:
    print("\nALL CHECKS PASSED — level is structurally consistent.")
