import networkx as nx

# =============================
# PATHFINDER
# =============================

class Pathfinder:

    @staticmethod
    def shortest_path(graph, start, goal):

        try:

            path = nx.shortest_path(
                graph,
                start,
                goal,
                weight="cost"
            )

        except nx.NetworkXNoPath:

            distances, paths = nx.single_source_dijkstra(graph, start, weight="cost")

            if not paths:
                return None

            closest = min(
                paths.keys(),
                key=lambda n: (n[0] - goal[0]) ** 2 + (n[1] - goal[1]) ** 2
            )

            path = paths[closest]

        actions = []

        for i in range(len(path) - 1):

            data = graph.get_edge_data(path[i], path[i + 1])

            actions.append(
                (path[i], data.get("action"), data.get("anchor"))
            )

        actions.append((path[-1], None, None))

        return actions

    @staticmethod
    def closest_node(graph, pos):

        x, y = pos

        return min(
            graph.G.nodes,
            key=lambda n: (n[0]-x)**2 + (n[1]-y)**2
        )
