using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlueAI : MonoBehaviour {
	
	float decisionTick = 2.0f;
	float lastTick = 0.0f;
	
	public Grid grid = null;
	public HomeBase homeBase = null;
	
	Path path;
	List<Node> _MidNodes;
	Cell finalCell;
	
	public bool AIOn = true;
	
	public Unit.Team team = Unit.Team.BLUE;
	
	// Use this for initialization
	void Start () {
		_MidNodes = new List<Node>();
	}
	
	public void Renew() {
		_MidNodes = new List<Node>();
	}
	
	void Update() {
		if (AIOn) {
			if (lastTick > decisionTick) {
				Tick ();	
				lastTick = 0.0f;
			}
		
			lastTick += Time.deltaTime;
		}
	}
	
	void Tick() {
		
		int random = Random.Range(0, 10);
		
		if (true) { //Selective
			Cell contested = MostContestedNode();
			List<Node> adjNodes = contested.AdjacentNodes(grid);
			
			List<Node> path = new List<Node>();
			Node spawnNode = null;
			int iterations = 0;
			
			while (spawnNode == null) {
				//First look for a spawn node
				if (path.Count > 0) {
					for (int i = 0; i < adjNodes.Count; i++) {
						Node n = adjNodes[i];
						if(path.Count > 0 && n.nodeType == Node.NodeType.RedSpawn) {
							//We're home!
							path.Add(n);
							spawnNode = n;
							break;
						}
					}
				}
				
				if (spawnNode == null) {
					for (int i = 0; i < adjNodes.Count; i++) {
						Node n = adjNodes[i];
						if (n.nodeType == Node.NodeType.Mid) {
							if (!path.Contains(n)) {
								path.Add(n);
								adjNodes = n.AdjacentNodes(grid, false);
								break;
							}
						}
					}
				}
				
				iterations++;
				if (iterations > 10) {
					Debug.Log("Could not find node for this: ");
					return;
				}
			}
			
			path.Reverse();
			_MidNodes = path;
			finalCell = contested;	
			
		}
		else {
			if (_MidNodes.Count > 0 && Random.Range (0, 10) % 3 == 0) { //Reuse
				int dc = Random.Range(1, homeBase.unitCount - 1);
				for (int i = 0; i < dc; i++)
					homeBase.DeployUnit(new Path(_MidNodes, finalCell));
				
				return;
			}
			else {
				_MidNodes = new List<Node>();
			}
			
			//Pick a starting node
			List<Node> spawn = grid.nodesForTeam(team);
			Node node = spawn[Random.Range(0, spawn.Count - 1)];
			_MidNodes.Add(node);
			
			//Pick a Path
			int depth = Random.Range(1, 5);
			for (int i = 0; i < depth; i++) {
				Node last = _MidNodes[_MidNodes.Count-1];
				List<Node> nodes = last.AdjacentNodes(grid, false);
				nodes = removeAdded(nodes);
				if (nodes.Count > 0) {
					Node n = nodes[Random.Range(0, nodes.Count-1)];	
					_MidNodes.Add(n);
				}
			}
			
			//Pick a Cell
			Node lst = _MidNodes[_MidNodes.Count-1];
			List<Cell> cells = lst.AdjacentCells(grid);
			finalCell = cells[Random.Range (0, cells.Count-1)];
		}
		
		//Pick another cell, or start over
		int deployCount = Random.Range(1, homeBase.unitCount - 1);
		for (int i = 0; i < deployCount; i++)
			homeBase.DeployUnit(new Path(_MidNodes, finalCell));
		
		string nums = "";
		foreach (Node n in _MidNodes) {
			nums += n.number + ", ";
		}
		
		Debug.Log ("Creating path for AI [" + nums + "] with endcell: " + finalCell.number);
			
		decisionTick = Random.Range(0.5f, 5.0f);
	}
				
	List<Node> removeAdded(List<Node> adj) {
		foreach	(Node n in _MidNodes) {
			adj.Remove (n);	
		}
		return adj;
	}
	
	//Good cells: 8, 5, 2
	
	Cell MostContestedNode() {
		float contestedScore = 0;
		List<Cell> contested = new List<Cell>();
		for (int i = 0; i < grid.gridCount * grid.gridCount; i++) {
			Cell c = grid.cells[i];
			float localScore = 0;//c.blueUnits > c.redUnits ? c.blueUnits : 0;
			if (c.takeOverBar > 5) localScore += (c.takeOverBar);
			//if (c.takeOverBar < 5) localScore -= c.takeOverBar;
			if (i == 8 || i == 5 || i == 2 || i == 0 || i == 1) localScore += 2;
			if (i == 4) localScore += 1;
			if (localScore > contestedScore) {
				contested.Clear();
				contested.Add (c);
				contestedScore = localScore;
			}
			else if (localScore == contestedScore) {
				contested.Add (c);
			}
		}
		
		return contested[Random.Range(0, contested.Count - 1)];
	}
}
