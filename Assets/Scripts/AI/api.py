from pydantic import BaseModel
from fastapi import FastAPI
from agent import run_agent_with_feedback

app = FastAPI()
class AnalyzeRequest(BaseModel):
    prompt: str

@app.post("/analyze")
async def analyze(request: AnalyzeRequest):
    result, level_data, report, iterations = run_agent_with_feedback(request.prompt)
    return {"result": result}

