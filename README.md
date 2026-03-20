# Annie's Adventure — AI-Powered 2D Platformer

**Annie's Adventure** is a 2D platformer built in Unity, extended with an AI backend for automated level analysis and procedural level generation. The project combines gameplay engineering with a real AI pipeline — from graph-based pathfinding to LLM-driven level design.

> © 2025 Riccardo Cruanes Rossini. All rights reserved.



## Gameplay

Annie's Adventure features:
- Responsive character movement with grappling-based traversal
- Modular enemy behaviors with distinct interaction patterns
- Flower jump mechanic enabling advanced vertical movement
- Clean state management and polished player feedback


## AI System

The project includes a full AI backend that analyzes and generates platformer levels autonomously.

### Architecture


Unity (Level Exporter)
        ↓ level_data.json
Python AI Backend
        ├── LevelSimulator      → models physics and tile geometry
        ├── NavigationGraph     → builds a navigable graph with walk/jump/fall/grapple edges
        ├── Pathfinder          → finds optimal paths using Dijkstra
        ├── LevelAnalyzer       → evaluates completability, difficulty, enemy impact
        ├── LevelBuilder        → procedurally generates levels from section grammar
        └── LangGraph Agent     → orchestrates analysis and generation with LLM tool calling
                ↓
        FastAPI REST API
                ↓
        Spring Boot Gateway
                ↓
        MongoDB (stores reports and generated levels)


### Key Features

**Level Analyzer** — given a level, the system automatically determines:
- whether the level is completable
- the optimal path cost (difficulty estimate)
- unreachable zones via graph component analysis
- whether enemies are mandatory or avoidable (graph surgery technique)

**Level Generator** — given a natural language prompt, the system:
- uses an LLM agent to generate a structured section description
- builds geometry procedurally via `LevelBuilder`
- validates completability with the analyzer
- iterates automatically until a valid level is produced
- saves results to MongoDB for future reference

**Unity Integration** — generated levels are exported as JSON and imported directly into Unity via a custom `LevelImporter`, including player spawn positioning.



## Tech Stack

| Area | Technology |
------
| Game Engine | Unity 2D |
| AI Backend | Python, LangChain, LangGraph |
| LLM | Ollama (local inference) |
| API | FastAPI |
| Gateway | Java, Spring Boot |
| Database | MongoDB |
| Infrastructure | Docker, docker-compose |



## Running the AI Backend

Prerequisites: Docker Desktop, Ollama


git clone https://github.com/your-repo/annie-adventure
docker compose up --build
docker exec -it ai-ollama-1 ollama pull qwen2.5:14b


The FastAPI docs are available at `http://localhost:8000/docs`

**Analyze a level:**

POST http://localhost:8080/analyze
{ "prompt": "Analyze level_data.json and tell me if it's too difficult" }


**Generate a level variant:**

POST http://localhost:8080/analyze
{ "prompt": "Generate a harder level" }

