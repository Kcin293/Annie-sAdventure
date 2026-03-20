import json


class LevelBuilder:
    defaults = {
    "moveSpeed": 5.0,
    "maxJumpHeight": 1,
    "maxJumpDistance": 2,
    "grappleRange": 4.0,
    "enemies": [],
    "maxFlowerJumpHeight": 255,
    "maxFlowerJumpDistance": 3
}
    def build_ground(self, start_x, length, y, depth=3) -> list:
        """
        Ritorna lista di dict {"x": ..., "y": ...}
        """
        tiles = []
        for x in range(start_x, start_x + length+1):
            for d in range(depth+1):
                tiles.append({"x": x, "y": y - d})
        return tiles

    def build_drop(self, x, y_top, y_bottom,width =5, depth=3) -> list:
        """
        Ritorna lista di dict {"x": ..., "y": ...}
        """
        tiles = []
        for d in range(depth+1):
           for y in range(y_bottom, y_top + 1): 
                tiles.append({"x": x - d, "y": y})
        
        for d in range(depth + 1):
            for fx in range(x, x + width + 1):
                tiles.append({"x": fx, "y": y_bottom - d})

        return tiles
    
    def build_staircase(self, start_x, start_y, count, spacing_x, height_delta, width, depth):
        """piattaforme a scalinata, salgono o scendono"""
        tiles = []

        for i in range(count):
            x = start_x + i * spacing_x
            y = start_y + i * height_delta
            tiles += self.build_floating_platform(x, y, width, depth)
        return tiles
    
    def build_column(self, start_x, start_y, count, width, depth):
        """piattaforme sovrapposte sulla stessa x """
        tiles = []

        for i in range(count):
            x = start_x
            y = start_y + i * depth
            tiles += self.build_floating_platform(x, y, width, depth)
        return tiles
    
    def build_floating_platform(self, x, y, width, depth):
        """singola piattaforma isolata — mattone base"""
        tiles = []
        for d in range(depth + 1):
            for fx in range(x, x + width + 1):
                tiles.append({"x": fx, "y": y - d})
        return tiles

    def build(self, sections: list, base_params: dict) -> dict:
        tiles = []
        current_x = 0
        for section in sections:
            section_type = section["type"]
            
            match  section_type:
                case "ground":
                    p = {k: v for k, v in section["params"].items() if k != "start_x"}
                    tiles += self.build_ground(current_x, **p)
                    current_x += section["params"]["length"]

                case "drop":
                    p = {k: v for k, v in section["params"].items() if k != "x"}
                    tiles += self.build_drop(current_x, **p)
                    current_x += section["params"]["width"]

                case "staircase":
                    p = {k: v for k, v in section["params"].items() if k != "start_x"}
                    tiles += self.build_staircase(current_x, **p)
                    current_x += section["params"]["count"] * section["params"]["spacing_x"]

                case "column":
                    p = {k: v for k, v in section["params"].items() if k != "start_x"}
                    tiles += self.build_column(current_x, **p)
                    current_x += section["params"]["width"]
                
            
        
        # deduplicazione
        tiles = list({(t["x"], t["y"]): t for t in tiles}.values())
        if "playerStart" not in base_params:
            first_section = sections[0]
            if first_section["type"] == "ground":
                p = first_section["params"]
                base_params["playerStart"] = {
                    "x": 1,
                    "y": p["y"] + 2
                }
            else:
                first_tile = tiles[0]
                base_params["playerStart"] = {"x": first_tile["x"] + 1, "y": first_tile["y"] + 1}

        if "goalPosition" not in base_params:
            last_section = sections[-1]
            if last_section["type"] == "ground":
                p = last_section["params"]
                base_params["goalPosition"] = {
                    "x": p["start_x"] + p["length"] - 2,
                    "y": p["y"] + 1
                }
            elif last_section["type"] == "drop":
                p = last_section["params"]
                base_params["goalPosition"] = {
                    "x": p["x"] + p["width"] - 2,
                    "y": p["y_bottom"] + 1
                }
            else:
                last_tile = tiles[-1]
                base_params["goalPosition"] = {"x": last_tile["x"], "y": last_tile["y"] + 1}
        for key, value in self.defaults.items():
            if key not in base_params:
                base_params[key] = value
        return {
            **base_params,
            "solidTiles": tiles
        }
