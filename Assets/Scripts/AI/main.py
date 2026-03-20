import matplotlib.pyplot as plt
import json
from LevelSimulator import LevelSimulator
from NavigationGraph import NavigationGraph
from Pathfinder import Pathfinder
from LevelAnalyzer import LevelAnalyzer
# =============================
# VISUALIZATION
# =============================

def visualize_graph(sim, graph, path=None, start=None, goal=None):

    plt.figure(figsize=(12, 8))

    nx = [x for x, y in sim.nav_nodes]
    ny = [y for x, y in sim.nav_nodes]
    plt.scatter(nx, ny, color="black", s=50)

    fx = [x for x, y in sim.floor_tiles]
    fy = [y for x, y in sim.floor_tiles]
    plt.scatter(fx, fy, color="saddlebrown", s=50)

    cx = [x for x, y in sim.ceiling_tiles]
    cy = [y for x, y in sim.ceiling_tiles]
    plt.scatter(cx, cy, color="gray", s=50)

    wx = [x for x, y in sim.wall_tiles]
    wy = [y for x, y in sim.wall_tiles]
    plt.scatter(wx, wy, color="brown", s=50)

    # nemici
    if sim.enemies:
        ex = [e[0] for e in sim.enemies]
        ey = [e[1] for e in sim.enemies]
        plt.scatter(ex, ey, marker="D", color="red", s=150, zorder=5, label="nemici")

    anchors_used = set()

    if path:

        for i in range(len(path) - 1):

            (x1, y1), action, anchor = path[i]
            (x2, y2), _, _ = path[i + 1]

            if action == "walk":
                plt.plot([x1, x2], [y1, y2], color="blue")

            elif action == "jump":
                plt.plot([x1, x2], [y1, y2], "--", color="red")

            elif action == "fall":
                plt.plot([x1, x2], [y1, y2], ":", color="cyan")

            elif action == "grapple":
                plt.plot([x1, x2], [y1, y2], ":", color="green")
                if anchor:
                    anchors_used.add(anchor)

            elif action == "jump_grapple":
                plt.plot([x1, x2], [y1, y2], "--", color="purple")
                if anchor:
                    anchors_used.add(anchor)

            elif action == "grapple_enemy":
                plt.plot([x1, x2], [y1, y2], "-.", color="orange")
                if anchor:
                    anchors_used.add(anchor)

            elif action == "flower_jump":
                plt.plot([x1, x2], [y1, y2], "--", color="magenta", linewidth=2)

    if anchors_used:
        ax = [a[0] for a in anchors_used]
        ay = [a[1] for a in anchors_used]
        plt.scatter(ax, ay, marker="*", color="orange", s=200, zorder=5)

    if start:
        plt.scatter(start[0], start[1], color="green", s=200, zorder=6, label="start")

    if goal:
        plt.scatter(goal[0], goal[1], marker="X", color="purple", s=200, zorder=6, label="goal")

    from matplotlib.lines import Line2D
    legend_elements = [
        Line2D([0],[0], color="blue",    label="walk"),
        Line2D([0],[0], color="red",     linestyle="--", label="jump"),
        Line2D([0],[0], color="cyan",    linestyle=":",  label="fall"),
        Line2D([0],[0], color="green",   linestyle=":",  label="grapple"),
        Line2D([0],[0], color="purple",  linestyle="--", label="jump_grapple"),
        Line2D([0],[0], color="orange",  linestyle="-.", label="grapple_enemy"),
        Line2D([0],[0], color="magenta", linestyle="--", label="flower_jump"),
    ]
    plt.legend(handles=legend_elements, loc="upper left", fontsize=8)

    plt.gca().set_aspect("equal", adjustable="box")
    plt.show()

def show_debug_info(sim, graph):
    print(f"jump_height={sim.jump_height}, jump_distance={sim.jump_distance}, grapple_range={sim.grapple_range}")
    print(f"nav_nodes: {len(sim.nav_nodes)}")
    print(f"floor_tiles: {len(sim.floor_tiles)}")
    print(f"ceiling_tiles: {len(sim.ceiling_tiles)}  esempi: {list(sim.ceiling_tiles)[:5]}")
    print(f"wall_tiles:    {len(sim.wall_tiles)}  esempi: {list(sim.wall_tiles)[:5]}")
    
    # test anchor da un nodo campione
    sample_node = next(iter(sim.nav_nodes))
    anchors = sim.possible_anchors(sample_node)
   
    print(f"Anchor da {sample_node}: {anchors[:5]} (tot={len(anchors)})")
    print("Edges:", graph.G.number_of_edges())

    from collections import Counter
    c = Counter()

    for _,_,d in graph.G.edges(data=True):
        c[d["action"]] += 1

    print(c)

    print("Start raw:", start_raw)
    print("Goal raw:", goal_raw)
    print("Start node:", start)
    print("Goal node:", goal)
    visualize_graph(sim, graph, path, start, goal)
    print("PATH:")
    print(path)



# =============================
# MAIN
# =============================

if __name__ == "__main__":

    with open("level_data.json") as f:
        data = json.load(f)

    sim = LevelSimulator(data)


    graph = NavigationGraph()
    graph.build(sim)

    start_raw = (data["playerStart"]["x"], data["playerStart"]["y"])
    goal_raw = (data["goalPosition"]["x"], data["goalPosition"]["y"])
    
    start = Pathfinder.closest_node(graph, start_raw)
    goal = Pathfinder.closest_node(graph, goal_raw)
    path = Pathfinder.shortest_path(graph.G, start, goal)
    show_debug_info(sim, graph)
    #print(LevelAnalyzer().analyze(sim, graph,path, start, goal))

