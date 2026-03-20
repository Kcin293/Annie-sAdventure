from langchain.agents import create_agent
from langchain_ollama import ChatOllama
from tools import analyze_level, generate_variant
import os
from langchain_core.messages import SystemMessage
import json
from database import save_level

system_prompt = """Sei un assistente per game designer.
Puoi analizzare livelli e generare varianti.

Per generare una variante usa il tool generate_variant con sections e base_params.

SEZIONI DISPONIBILI:
- ground: {start_x, length, y, depth}
- drop: {x, y_top, y_bottom, width, depth}
- staircase: {start_x, start_y, count, spacing_x, height_delta, width, depth}
- column: {start_x, start_y, count, width, depth}

Ogni sezione ha formato: {"type": "...", "params": {...}}

VINCOLI:
- la distanza orizzontale tra piattaforme non può superare 2 unità
- le sezioni non devono sovrapporsi — ogni sezione inizia dove finisce la precedente
- il drop deve avere y_top > y_bottom (scende verso il basso)

base_params DEVE essere sempre:
{
    "playerStart": {"x": start_x_primo_ground + 1, "y": y_primo_ground + 2},
    "goalPosition": {"x": ultima_sezione_x + 5, "y": ultima_sezione_y + 1},
}
IMPORTANTE: dopo aver chiamato generate_variant, chiama SEMPRE analyze_level 
sul file "generated_level_data.json" per verificare che il livello sia completabile.
Se analyze_level dice che il livello non è completabile, 
chiama SUBITO generate_variant con una versione corretta.
Non descrivere le modifiche — applicale direttamente chiamando il tool.

"""
llm = ChatOllama(model="qwen2.5:14b", base_url=os.getenv("OLLAMA_HOST", "http://localhost:11434"))
tools = [analyze_level, generate_variant]

agent = create_agent(llm, tools,system_prompt=system_prompt)

def run_agent_with_feedback(prompt, max_iterations=3):
    for i in range(max_iterations):
        result = agent.invoke({
            "messages": [("human", prompt)]
        })
        
        level_data = None
        report = None
        
        for message in result["messages"]:
            if message.type == "tool" and message.name == "generate_variant":
                if message.content and message.content.strip():
                    try:
                        level_data = json.loads(message.content)
                        with open("generated_level_data.json", "w") as f:
                            json.dump(level_data, f, indent=2)
                    except json.JSONDecodeError as e:
                        print(f"Errore parsing JSON: {e}")
            
            if message.type == "tool" and message.name == "analyze_level":
                if message.content and message.content.strip():
                    try:
                        report = json.loads(message.content)
                    except:
                        pass
        
        # se il livello è completabile → fermati
        if report and report.get("completable"):
            print(f"Livello completabile trovato in {i+1} iterazioni!")
            if level_data:
                save_level(prompt, level_data, report, i+1)
            return result["messages"][-1].content, level_data, report, i+1
        
        # altrimenti aggiorna il prompt con feedback
        prompt = f"{prompt}. Il livello precedente non era completabile, riprova."
        if level_data:
                save_level(prompt, level_data, report, i+1)


    return result["messages"][-1].content, level_data, report, max_iterations
