import networkx as nx

from Pathfinder import Pathfinder
class LevelAnalyzer:
    def analyze(self, sim, graph, path, start, goal) -> dict:
        reportData = {}
        completable = self.is_completable(graph, start, goal)
        reportData["completable"] = completable
        reportData["path_cost"] = self.path_cost(graph.G, path) if reportData["completable"] else float('inf')
        reportData["isolated_zones"] = self.isolated_zones(graph) 
        reportData["enemy_impact"] = self.enemy_impact(sim, graph, path, start, goal, completable)
        report = {
            "completable": reportData["completable"],
            "path_cost": reportData["path_cost"],
            "isolated_zones": len(reportData["isolated_zones"]) -1 # tolgo la zona principale che contiene start e goal
        }
        report["enemy_impact"] = {
        "label": "Mandatory" if reportData["enemy_impact"]["mandatory"] else "Non mandatory",
        "cost_with": reportData["enemy_impact"]["cost_with_enemies"],
        "cost_without": reportData["enemy_impact"]["cost_without_enemies"],
        "delta": None if reportData["enemy_impact"]["cost_without_enemies"] == float('inf') 
                else reportData["enemy_impact"]["cost_with_enemies"] - reportData["enemy_impact"]["cost_without_enemies"]        
        }
        return report

    def is_completable(self, graph, start, goal):
        try:
            nx.shortest_path(graph.G, start, goal)
            return True
        except nx.NetworkXNoPath:
            return False

    def path_cost(self, G, path):
        cost = 0
        for i in range(len(path) - 1):
            data = G.get_edge_data(path[i][0], path[i + 1][0])
            cost += data.get("cost", 1) if data is not None else 1
        return cost
    
    def isolated_zones(self, graph):
        return list(nx.weakly_connected_components(graph.G))
    
    def enemy_impact(self, sim, graph, path, start, goal, completable):
        # Rimuovi nodi nemici
        cost_with_enemies = self.path_cost(graph.G, path) if completable else float('inf')
        G = graph.G.copy()
        for enemy in sim.enemies:
            if enemy in G:
                G.remove_node(enemy)

        # Ricalcola path
       
        path = Pathfinder.shortest_path(G, start, goal)
        if path[-1][0] != goal:
            cost_without_enemies = float('inf')
        else:
            cost_without_enemies = self.path_cost(G, path)
        

        return {
            "cost_with_enemies": cost_with_enemies,
            "cost_without_enemies": cost_without_enemies,
            "mandatory": True if cost_without_enemies == float('inf') and cost_with_enemies != float('inf') else False
        }