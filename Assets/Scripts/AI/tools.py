from langchain.tools import tool
from typing import Any
@tool
def analyze_level(json_path: str) -> dict:
    """
    Analizza un livello di gioco a partire da un file JSON e restituisce un report dettagliato.
    
    Args:
        json_path (str): Il percorso del file JSON contenente i dati del livello.
    
    Returns:
        dict: Un report con informazioni sulla completabilità, costo del percorso, zone isolate e impatto dei nemici.
    """
    import json
    import networkx as nx
    from LevelSimulator import LevelSimulator
    from NavigationGraph import NavigationGraph
    from Pathfinder import Pathfinder
    from LevelAnalyzer import LevelAnalyzer

    with open(json_path) as f:
        data = json.load(f)

    sim = LevelSimulator(data)
    graph = NavigationGraph()
    graph.build(sim)

    start_raw = (data["playerStart"]["x"], data["playerStart"]["y"])
    goal_raw = (data["goalPosition"]["x"], data["goalPosition"]["y"])
    
    start = Pathfinder.closest_node(graph, start_raw)
    goal = Pathfinder.closest_node(graph, goal_raw)
    path = Pathfinder.shortest_path(graph.G, start, goal)

    return LevelAnalyzer().analyze(sim, graph,path, start, goal)

@tool
def generate_variant(sections: Any, base_params: Any) -> str:
    """
    Genera un livello platformer.
    
    Args:
        sections: lista di sezioni del livello
        base_params: parametri base del livello
    """
    import json
    from LevelBuilder import LevelBuilder
    
    sections_data = json.loads(sections) if isinstance(sections, str) else sections
    params_data = json.loads(base_params) if isinstance(base_params, str) else base_params
    
    builder = LevelBuilder()
    return json.dumps(builder.build(sections_data, params_data))

