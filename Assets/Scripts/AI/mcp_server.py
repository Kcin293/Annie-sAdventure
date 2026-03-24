import asyncio
from mcp.server import Server
from mcp.server.stdio import stdio_server
from mcp.types import Tool, TextContent
from typing import Any
from level_service import analyze_level,generate_variant
server = Server("annie-adventure")

@server.list_tools()
async def list_tools() -> list[Tool]:
    return [
        Tool(
            name="analyze_level",
            description="Analizza un livello di gioco a partire da un file JSON",
            inputSchema={
                "type": "object",
                "properties": {
                    "json_path": {
                        "type": "string",
                        "description": "Percorso del file JSON del livello"
                    }
                },
                "required": ["json_path"]
            }
        ),
         Tool(
    name="generate_variant",
    description="Genera una variante di livello platformer. sections è una lista di sezioni, ognuna con type e params.",
    inputSchema={
            "type": "object", 
            "properties": {
                "sections": {
                    "type": "array",
                    "description": "Lista di sezioni. Ogni sezione ha type (ground/drop/staircase/column) e params",
                    "items": {
                        "type": "object",
                        "properties": {
                            "type": {"type": "string"},
                            "params": {"type": "object"}
                        }
                    }
                },
                "base_params": {
                    "type": "object",
                    "description": "Parametri base del livello. Può essere vuoto {}",
                    "default": {}
                }
            },
            "required": ["sections"]
        }
    )

    ]

@server.call_tool()
async def call_tool(name: str, arguments: dict) -> list[TextContent]:
    if name == "analyze_level":
        result = analyze_level(json_path=arguments["json_path"])
        return [TextContent(type="text", text=str(result))]
    elif name == "generate_variant":
        result = generate_variant(
            sections=arguments["sections"],
            base_params=arguments.get("base_params", {})
        )
        return [TextContent(type="text", text=result)]

async def main():
    async with stdio_server() as (read, write):
        await server.run(read, write, server.create_initialization_options())

if __name__ == "__main__":
    asyncio.run(main())