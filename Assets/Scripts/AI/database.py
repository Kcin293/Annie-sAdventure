from pymongo import MongoClient
import os
from datetime import datetime

client = MongoClient(os.getenv("MONGO_URL", "mongodb://localhost:27017"))
db = client["annie_adventure"]
levels_collection = db["levels"]

def save_level(prompt: str, level_data: dict, report: dict, iterations: int):
    document = {
        "prompt": prompt,
        "level_data": level_data,
        "report": report,
        "iterations": iterations,
        "generated_at": datetime.now().isoformat()
    }
    result = levels_collection.insert_one(document)
    return str(result.inserted_id)

def get_all_levels():
    levels = list(levels_collection.find({}, {"_id": 0}))
    return levels