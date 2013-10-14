using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Node : MonoBehaviour
{
	public int number;
	
	void Update() {}
	
	public enum NodeType {
		RedSpawn,
		BlueSpawn,
		Corner,
		Mid
	}
	
	public NodeType nodeType;
	
	public GameObject highlight;
	
	void Start() {
		exSprite high = highlight.GetComponent<exSprite>();	
		high.color = Color.clear;
	}
	
	public List<Cell> AdjacentCells(Grid grid) {
		Vector2 worldPoint = grid.node2WorldPoint(number);
		
		Vector2 CellNE = new Vector3(worldPoint.x + (grid.gridSize * 0.5f), worldPoint.y + (grid.gridSize * 0.5f));
		Vector2 CellNW = new Vector3(worldPoint.x - (grid.gridSize * 0.5f), worldPoint.y + (grid.gridSize * 0.5f));
		Vector2 CellSW = new Vector3(worldPoint.x - (grid.gridSize * 0.5f), worldPoint.y - (grid.gridSize * 0.5f));
		Vector2 CellSE = new Vector3(worldPoint.x + (grid.gridSize * 0.5f), worldPoint.y - (grid.gridSize * 0.5f));
		
		int cellNE = grid.point2Cell(CellNE);
		int cellNW = grid.point2Cell(CellNW);
		int cellSW = grid.point2Cell(CellSW);
		int cellSE = grid.point2Cell(CellSE);
		
		Cell ne = null, nw = null, sw = null, se = null;
		grid.cells.TryGetValue(cellNE, out ne);
		grid.cells.TryGetValue(cellNW, out nw);
		grid.cells.TryGetValue(cellSW, out sw);
		grid.cells.TryGetValue(cellSE, out se);
		
		List<Cell> adj = new List<Cell>();
		if (ne != null) adj.Add(ne);
		if (nw != null) adj.Add(nw);
		if (sw != null) adj.Add(sw);
		if (se != null) adj.Add(se);
		
		return adj;
	}
	
	public void SetHighlighted (bool on, bool selected) {
		if (!on) {
			exSprite high = highlight.GetComponent<exSprite>();	
			high.color = Color.clear;
		}
		else {
			if (selected) {
				exSprite high = highlight.GetComponent<exSprite>();	
				high.color = Color.clear;
			}
			else {
				exSprite high = highlight.GetComponent<exSprite>();	
				high.color = Color.yellow;
			}
		}
		
	}
	
	public List<Node> AdjacentNodes(Grid grid, bool removeSpawn) {
		List<Node> nodes = new List<Node>();
		Node nUp = null, nDown = null, nRight = null, nLeft = null;
		grid.nodes.TryGetValue(number + 1, out nRight);
		grid.nodes.TryGetValue(number - 1, out nLeft);
		grid.nodes.TryGetValue(number + grid.nodeCount, out nUp);
		grid.nodes.TryGetValue(number - grid.nodeCount, out nDown);
		if (nUp != null) nodes.Add (nUp);
		if (nDown != null) nodes.Add (nDown);
		if (nLeft != null) nodes.Add (nLeft);
		if (nRight != null) nodes.Add (nRight);
		
		if ((nodeType == NodeType.BlueSpawn || nodeType == NodeType.RedSpawn)) {
			//Remove other spawn nodes
			List<Node> removed = new List<Node>();
			if (removeSpawn) {
				foreach (Node n in nodes) {
					if (n.nodeType == NodeType.BlueSpawn || n.nodeType == NodeType.RedSpawn || n.nodeType == NodeType.Corner) {
						removed.Add(n);	
					}
				}
			}
			
			foreach (Node n in removed) {
				nodes.Remove(n);
			}
			
			return nodes;
		}
		else if (nodeType == NodeType.Mid) {
			List<Node> removed = new List<Node>();
			if (removeSpawn) {
				foreach (Node n in nodes) {
					if (n.nodeType == NodeType.BlueSpawn || n.nodeType == NodeType.RedSpawn || n.nodeType == NodeType.Corner) {
						removed.Add(n);	
					}
				}
			}
			foreach (Node n in removed) {
				nodes.Remove(n);
			}
			
			return nodes;
		}
		else {
			return new List<Node>();	
		}
	}
	
	public void SetNodeNumber(Grid grid, int num) {
		number = num;
		
		Vector2 gridPoint = grid.node2GridPoint(number);
		if (gridPoint.x % grid.nodeCount != 0 && 
			gridPoint.y % grid.nodeCount != 0 &&
			gridPoint.x % grid.nodeCount != grid.nodeCount - 1 &&
			gridPoint.y % grid.nodeCount != grid.nodeCount - 1) {
			nodeType = NodeType.Mid;
		}
		else if (gridPoint.x % grid.nodeCount == 0 || 
				 gridPoint.y % grid.nodeCount == 0 ||
				 gridPoint.x % grid.nodeCount == grid.nodeCount-1 || 
				 gridPoint.y % grid.nodeCount == grid.nodeCount-1) {
			if ( (gridPoint.x % grid.nodeCount == 0 && gridPoint.y % grid.nodeCount == 0) ||
				 (gridPoint.x % grid.nodeCount == grid.nodeCount-1 && gridPoint.y % grid.nodeCount == grid.nodeCount-1) ||
				 (gridPoint.x % grid.nodeCount == 0 && gridPoint.y % grid.nodeCount == grid.nodeCount-1) ||
				 (gridPoint.x % grid.nodeCount == grid.nodeCount-1 && gridPoint.y % grid.nodeCount == 0) ){
				nodeType = NodeType.Corner;	
				highlight.SetActive(false);
			}
			else {
				Vector2 blueSpawn = new Vector2(grid.nodeCount - 1, 0);
				if (blueSpawn.x == gridPoint.x || blueSpawn.y == gridPoint.y) {
					nodeType = NodeType.BlueSpawn;	
				}
				else {
					nodeType = NodeType.RedSpawn;	
				}
			}
		}
	}
}

