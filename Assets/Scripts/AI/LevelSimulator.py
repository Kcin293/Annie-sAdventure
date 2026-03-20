import math
import numpy as np


class LevelSimulator:
    def __init__(self, data):

        self.jump_height = math.ceil(data["maxJumpHeight"])
        self.jump_distance = math.ceil(data["maxJumpDistance"])
        self.grapple_range = math.ceil(data["grappleRange"])

        self.solid = {(int(t["x"]), int(t["y"])) for t in data["solidTiles"]}

        self.flower_jump_height = math.ceil(data.get("maxFlowerJumpHeight", 0))
        self.flower_jump_distance = math.ceil(data.get("maxFlowerJumpDistance", 0))

        self.enemies = []
        for e in data.get("enemies", []):
            pos = e.get("position")
            if pos:
                self.enemies.append((int(pos["x"]), int(pos["y"])))

        self.enemy_set = set(self.enemies)

        self.nav_nodes = set()
        self.floor_tiles = set()
        self.wall_tiles = set()
        self.ceiling_tiles = set()

        for (x, y) in self.solid:

            # nodo navigabile sopra il pavimento
            if (x, y + 1) not in self.solid:
                self.nav_nodes.add((x, y + 1))
            
            if (x, y + 1) not in self.solid:
                self.floor_tiles.add((x, y))

            if (x, y - 1) not in self.solid:
                self.ceiling_tiles.add((x, y))

            if (x + 1, y) not in self.solid or (x - 1, y) not in self.solid:
                self.wall_tiles.add((x, y))

    # -------------------------
    # POSIZIONE REALE PLAYER
    # -------------------------

    def player_pos(self, tile):
        x, y = tile
        return (x, y)

    # -------------------------

    def is_walkable(self, tile):
        return tile in self.nav_nodes

    def dist(self, a, b):
        return np.linalg.norm(np.array(a) - np.array(b))

    def can_jump(self, a, b):

        ax, ay = a
        bx, by = b

        dx = abs(bx - ax)
        dy = by - ay

        if dx > self.jump_distance:
            return False

        if dy > self.jump_height:
            return False

        return True

    # -------------------------
    # LINE OF SIGHT
    # -------------------------

    def line_of_sight(self, a, b, ignore=None):

        x0, y0 = a
        x1, y1 = b

        dx = abs(x1 - x0)
        dy = abs(y1 - y0)

        steps = int(max(dx, dy) * 4) + 1

        for i in range(steps):

            t = i / steps

            x = x0 + (x1 - x0) * t
            y = y0 + (y1 - y0) * t

            tile = (round(x), round(y))

            if ignore and tile in ignore:
                continue

            if tile in self.solid:
                return False

        return True

    # -------------------------
    # GRAPPLE
    # -------------------------

    def possible_anchors(self, pos, min_y=None):
        if min_y is None:
            min_y = pos[1]
        anchors = []

        for (x,y) in self.wall_tiles:

            if y < min_y:
                continue
            anchor = (x, y)

            if self.dist(pos, anchor) <= self.grapple_range:
                anchors.append(anchor)

        for (x,y) in self.ceiling_tiles:
            if y < min_y:
                continue
            anchor = (x, y)

            if self.dist(pos, anchor) <= self.grapple_range:
                anchors.append(anchor)

        return anchors

    def grapple_landings(self, pos, anchor, allow_jump=False):

        ax, ay = anchor
        r = self.dist(pos, anchor)
        anchor_tile = {(ax, ay)}

        tiles = set()

        if not self.line_of_sight(pos, anchor, ignore=anchor_tile):
            return []

        # angolo misurato dalla verticale verso il basso (0° = pende dritto)
        # sin dà l'offset orizzontale, -cos dà l'offset verso il basso
        for angle in range(-80, 81, 10):

            rad = math.radians(angle)

            x = ax + r * math.sin(rad)
            y = ay - r * math.cos(rad)

            swing = (round(x), round(y))

            # la posizione di swing non deve essere dentro un solido
            if swing in self.solid:
                continue

            if not self.line_of_sight(anchor, swing, ignore=anchor_tile):
                continue

            # atterraggio diretto sull'arco
            if swing in self.nav_nodes:
                tiles.add(swing)

            # salto dalla posizione di swing
            if allow_jump:
                sx, sy = swing
                for dx in range(-self.jump_distance, self.jump_distance + 1):
                    for dy in range(0, self.jump_height + 1):
                        tile = (sx + dx, sy + dy)
                        if tile in self.nav_nodes and self.can_jump(swing, tile):
                            if self.line_of_sight(swing, tile):
                                tiles.add(tile)

        return list(tiles)




