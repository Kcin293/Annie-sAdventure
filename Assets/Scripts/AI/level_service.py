from typing import Any


def analyze_level(json_path: str) -> dict:    
    import json
    import networkx as nx
    from LevelSimulator import LevelSimulator
    from NavigationGraph import NavigationGraph
    from Pathfinder import Pathfinder
    from LevelAnalyzer import LevelAnalyzer

    with open(json_path) as f:
        data = json.load(f)
    # defaults se mancanti
    defaults = {
        "maxJumpHeight": 1,
        "maxJumpDistance": 2,
        "grappleRange": 4.0,
        "maxFlowerJumpHeight": 255,
        "maxFlowerJumpDistance": 3,
        "enemies": []
    }
    for key, value in defaults.items():
        if key not in data:
            data[key] = value

    sim = LevelSimulator(data)
    graph = NavigationGraph()
    graph.build(sim)

    start_raw = (data["playerStart"]["x"], data["playerStart"]["y"])
    goal_raw = (data["goalPosition"]["x"], data["goalPosition"]["y"])
    
    start = Pathfinder.closest_node(graph, start_raw)
    goal = Pathfinder.closest_node(graph, goal_raw)
    path = Pathfinder.shortest_path(graph.G, start, goal)

    return LevelAnalyzer().analyze(sim, graph,path, start, goal)


def generate_variant(sections: Any, base_params: Any) -> str:
    import json
    from LevelBuilder import LevelBuilder
    
    sections_data = json.loads(sections) if isinstance(sections, str) else sections
    params_data = json.loads(base_params) if isinstance(base_params, str) else base_params
    
    builder = LevelBuilder()
    return json.dumps(builder.build(sections_data, params_data))

