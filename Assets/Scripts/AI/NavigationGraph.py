import networkx as nx

# =============================
# NAVIGATION GRAPH
# =============================

class NavigationGraph:

    def __init__(self):
        self.G = nx.DiGraph()

    def build(self, sim):

        for tile in sim.nav_nodes:
            self.G.add_node(tile)

        self.add_walk_edges(sim)
        self.add_jump_edges(sim)
        self.add_fall_edges(sim)
        self.add_grapple_edges(sim)
        self.add_jump_grapple_edges(sim)
        self.add_enemy_grapple_edges(sim)
        self.add_flower_jump_edges(sim)

    def add_walk_edges(self, sim):

        for (x, y) in sim.nav_nodes:

            for dx in [-1, 1]:

                target = (x + dx, y)

                if sim.is_walkable(target):

                    cost = 1
                    if (x, y) in sim.enemy_set or target in sim.enemy_set:
                        cost += 20  # evita di camminare sulla posizione del nemico

                    self.G.add_edge(
                        (x, y),
                        target,
                        action="walk",
                        cost=cost
                    )

    # -------------------------

    def add_jump_edges(self, sim):

        for node in sim.nav_nodes:

            x, y = node

            for dx in range(-sim.jump_distance, sim.jump_distance + 1):
                for dy in range(1, sim.jump_height + 1):

                    target = (x + dx, y + dy)

                    if target in sim.nav_nodes:

                        if sim.can_jump(node, target) and sim.line_of_sight(node, target):

                            self.G.add_edge(
                                node,
                                target,
                                action="jump",
                                cost=4
                            )
    # -------------------------

    def add_fall_edges(self, sim):

        for (x, y) in sim.nav_nodes:

            for dx in [-1, 1]:

                # cade solo dal bordo: il tile adiacente non ha pavimento
                if (x + dx, y) in sim.nav_nodes:
                    continue

                # scende lungo la colonna (x+dx) fino al prossimo nav_node
                for drop in range(1, 64):

                    landing = (x + dx, y - drop)

                    if landing in sim.solid:
                        break

                    if landing in sim.nav_nodes:
                        self.G.add_edge(
                            (x, y),
                            landing,
                            action="fall",
                            cost=1 + drop
                        )
                        break

    # -------------------------

    def on_same_platform(self, sim, a, b):
        """True se a e b sono alla stessa quota con pavimento continuo tra loro."""
        if a[1] != b[1]:
            return False
        x1, x2 = sorted([a[0], b[0]])
        for x in range(x1, x2 + 1):
            if (x, a[1]) not in sim.nav_nodes:
                return False
        return True

    # -------------------------

    def add_grapple_edges(self, sim):

        for node in sim.nav_nodes:

            anchors = sim.possible_anchors(node)

            for anchor in anchors:

                landings = sim.grapple_landings(node, anchor)

                for tile in landings:

                    if self.on_same_platform(sim, node, tile):
                        continue

                    self.G.add_edge(
                        node,
                        tile,
                        action="grapple",
                        cost=6,
                        anchor=anchor
                    )

    # -------------------------

    def add_jump_grapple_edges(self, sim):

        for node in sim.nav_nodes:

            x, y = node

            for dx in range(-sim.jump_distance, sim.jump_distance + 1):
                for dy in range(1, sim.jump_height + 1):

                    mid = (x + dx, y + dy)

                    if not sim.can_jump(node, mid):
                        continue

                    anchors = sim.possible_anchors(mid, min_y=y)

                    for anchor in anchors:

                        if not sim.line_of_sight(mid, anchor):
                            continue

                        landings = sim.grapple_landings(mid, anchor, allow_jump=True)

                        for tile in landings:

                            if self.on_same_platform(sim, node, tile):
                                continue

                            self.G.add_edge(
                                node,
                                tile,
                                action="jump_grapple",
                                cost=6,
                                anchor=anchor
                            )

    # -------------------------

    def add_enemy_grapple_edges(self, sim):
        """Grapple verso un nemico: il rampino lo colpisce, il giocatore atterra sulla sua posizione."""

        for node in sim.nav_nodes:

            for enemy in sim.enemy_set:

                if sim.dist(node, enemy) > sim.grapple_range:
                    continue

                if not sim.line_of_sight(node, enemy, ignore={enemy}):
                    continue

                self.G.add_node(enemy)
                self.G.add_edge(
                    node,
                    enemy,
                    action="grapple_enemy",
                    cost=6,
                    anchor=enemy
                )

    # -------------------------

    def add_flower_jump_edges(self, sim):
        """Da ogni posizione nemica colpita, il giocatore può fare un flower jump.
        L'altezza estrema (255) permette di scavalcare qualsiasi muro: no LoS check."""

        for enemy in sim.enemy_set:

            ex, ey = enemy
            self.G.add_node(enemy)

            for dx in range(-sim.flower_jump_distance, sim.flower_jump_distance + 1):
                for dy in range(0, sim.flower_jump_height + 1):

                    target = (ex + dx, ey + dy)

                    if target not in sim.nav_nodes:
                        continue

                    self.G.add_edge(
                        enemy,
                        target,
                        action="flower_jump",
                        cost=4
                    )