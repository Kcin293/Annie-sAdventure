from langchain.tools import tool
from typing import Any
from level_service import analyze_level as analyze_level_service
from level_service import generate_variant as generate_variant_level_service
@tool
def analyze_level(json_path: str) -> dict:
    """
    Analizza un livello di gioco a partire da un file JSON e restituisce un report dettagliato.
    
    Args:
        json_path (str): Il percorso del file JSON contenente i dati del livello.
    
    Returns:
        dict: Un report con informazioni sulla completabilità, costo del percorso, zone isolate e impatto dei nemici.
    """
    return analyze_level_service(json_path=json_path)
    

@tool
def generate_variant(sections: Any, base_params: Any) -> str:
    """
    Genera un livello platformer.
    
    Args:
        sections: lista di sezioni del livello
        base_params: parametri base del livello
    """
    return generate_variant_level_service(sections=sections,base_params=base_params)

