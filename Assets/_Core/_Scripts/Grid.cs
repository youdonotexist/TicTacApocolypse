using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

public class Grid : MonoBehaviour {
	// Update is called once per frame
	public int gridCount = 3;
	public int gridSize = 100;
	public int nodeCount;
	
	public Material neutralLineMaterial;
	public bool Rebuild;
	
	public LayerMask NodeLayer;
	public LayerMask CellLayer;
	
	public GameObject X;
	public GameObject O;
	public GameObject XNodePrefab;
	public GameObject ONodePrefab;
	public GameObject CellPrefab;
	public GameObject OBasePrefab;
	public GameObject XBasePrefab;
	public GameObject HighwayPrefab;
	
	public Dictionary<int, Cell> cells;
	public Dictionary<int, Node> nodes;
	
	public HomeBase redBase;
	public HomeBase blueBase;
	
	public UILabel winLabel;
	public GameObject winContainer;
	
	List<Node> blueSpawnNodes;
	List<Node> redSpawnNodes;
	
	public AudioClip NodeAudio;
	
	enum GamePhase {
		SYMBOL_PLACEMENT,
		BATTLE
	};
	
	public Unit.Team team = Unit.Team.RED;
	
	GamePhase gamePhase = GamePhase.SYMBOL_PLACEMENT;
	
	void Start() {
		//MakeGrid();
	}
	
	List<Node> _MidNodes;
	Cell _CurrentCell;
	
	VectorLine _DrawnPath;
	
	List<Node> _CurrentlyAdjacentNodes;
	List<Cell> _CurrentlyAdjacentCells;
	
	Path _currentPath;
	
	bool PointMatchesTeam(Node node) {
		if (node.nodeType == Node.NodeType.RedSpawn && team == Unit.Team.RED) {
			return true;	
		}
		else if (node.nodeType == Node.NodeType.BlueSpawn && team == Unit.Team.BLUE) {
			return true;	
		}
		
		return false;
		
	}
	
	void Update () {
		if (_MidNodes == null)
			return;
		
		CheckForWin();
		
		if (Input.GetMouseButton(0)) {
			Ray world = Camera.mainCamera.ScreenPointToRay(Input.mousePosition);
					
				Node n = obj.collider.GetComponent<Node>();
				if (_currentPath != null) {
					if (PointMatchesTeam(n)) {
						Destroy(_DrawnPath.vectorObject);
						_currentPath = null;
						_DrawnPath = null;
						ClearAll();	
					}
					else {
						return;	
					}
				}
						
				if (_MidNodes.Count == 0) {
					if (PointMatchesTeam(n)) {
						_MidNodes.Add(n);
						n.SetHighlighted(true, true);
						HighlightAdjacentNodes(n.AdjacentNodes(this, true));
						audio.PlayOneShot(NodeAudio);
					}
				}
				else if (n.nodeType == Node.NodeType.Mid || n.nodeType == Node.NodeType.BlueSpawn || n.nodeType == Node.NodeType.RedSpawn) {
					if (!_MidNodes.Contains(n)) {
						//Can I walk to it?
						Node lastNode = _MidNodes[_MidNodes.Count-1];
						List<Node> adjacent = lastNode.AdjacentNodes(this, true);
						if (adjacent.Contains(n)) {
							_MidNodes.Add(n);
							n.SetHighlighted(true, true);
							HighlightAdjacentNodes(n.AdjacentNodes(this, true));
							
							audio.PlayOneShot(NodeAudio);
							
							UpdateDrawnPath();
						}
					}	
				}
			}
		}
		else {
			//check to see if we have everything
			if (_MidNodes.Count > 1) {
				_currentPath = new Path(_MidNodes, null);
				Node node = _MidNodes[_MidNodes.Count - 1];
				
				HighlightAdjacentNodes(new List<Node>());
				HighlightAdjacentCells(node.AdjacentCells(this));
			}
			else {
				ClearAll();
			}
		}
		
		if (Input.GetMouseButtonDown(0)) {
			if (_MidNodes.Count > 1) {
				Ray world = Camera.mainCamera.ScreenPointToRay(Input.mousePosition);
				RaycastHit obj;
				if (Physics.Raycast(world, out obj, Mathf.Infinity, CellLayer)) {
					Node lastNode = _MidNodes[_MidNodes.Count - 1];
					List<Cell> adjacent = lastNode.AdjacentCells(this);
					Cell cell = obj.collider.GetComponent<Cell>();
					
					if (_CurrentCell != null && adjacent.Contains(cell)) {
						_CurrentCell.Highlight(true, false);	
					}
					
					if (adjacent.Contains(cell)) {
						_CurrentCell = cell;
						_CurrentCell.Highlight(true, true);	
						
						if (team == Unit.Team.RED) {
							redBase.DeployUnit(new Path(_MidNodes, _CurrentCell));
						}
						else {
							blueBase.DeployUnit(new Path(_MidNodes, _CurrentCell));
						}
					}
				}
			}
		}
		
		
		//Rebuild Grid
		if (Rebuild) {
			Rebuild = false;
			
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; i++) {
				Transform t = transform.GetChild(0);
				t.parent = null;
				Destroy(t.gameObject);
			}
			
			MakeGrid();
		}
	}
	
	void ClearAll() {
		if (_MidNodes != null) {
			foreach (Node n in _MidNodes) {
				n.SetHighlighted(false,false);	
			}
			_MidNodes = new List<Node>();
			HighlightAdjacentNodes(new List<Node>());
			HighlightAdjacentCells(new List<Cell>());
		}
				
		if (_CurrentCell != null) {
			_CurrentCell.Highlight(false, false);
			_CurrentCell = null;
		}
	}
	
	void HighlightAdjacentNodes(List<Node> adjacent) {
		foreach (Node n in _CurrentlyAdjacentNodes) {
			if (!_MidNodes.Contains(n))
				n.SetHighlighted(false, false);					
		}	
		
		foreach(Node n in adjacent) {
			if (!_MidNodes.Contains(n))
				n.SetHighlighted(true, false);				
		}
		
		_CurrentlyAdjacentNodes = adjacent;
	}
	
	void HighlightAdjacentCells(List<Cell> adjacent) {
		foreach (Cell c in _CurrentlyAdjacentCells) {
			c.Highlight(false, false);					
		}	
		
		foreach(Cell c in adjacent) {
			c.Highlight(true, false);				
		}
		
		_CurrentlyAdjacentCells = adjacent;
	}
			
	public void MakeGrid () {
	    cells = new Dictionary<int, Cell>();
		nodes = new Dictionary<int, Node>();
		_CurrentlyAdjacentNodes = new List<Node>();
		_CurrentlyAdjacentCells = new List<Cell>();
		_MidNodes = new List<Node>();
		redSpawnNodes = null;
		blueSpawnNodes = null;
		
		nodeCount = (gridCount + 1);
		
		//Nodes
		for (int i = 0; i < nodeCount * nodeCount; i++) {
			Vector2 pt 	= node2WorldPoint(i);
			GameObject go = (GameObject) GameObject.Instantiate(XNodePrefab, new Vector3(pt.x, pt.y, 0.0f), XNodePrefab.transform.rotation);
			go.transform.parent = transform;
			exSprite sprite = go.GetComponent<exSprite>();
			Node node = go.GetComponent<Node>();
			node.SetNodeNumber(this, i);
			nodes.Add(i, node);
			
			go.transform.parent = transform;
			
			
			if (node.nodeType == Node.NodeType.BlueSpawn) { //1, 2, 7, 11
				//sprite.color = new Color(57.0f/255.0f,154.0f/255.0f, 196.0f/255.0f);
				sprite.SetSprite(sprite.atlas, sprite.atlas.GetIndexByName("node-blue"), true);
			}
			else if (node.nodeType == Node.NodeType.RedSpawn) {
				//sprite.color = new Color(201.0f/255.0f,41.0f/255.0f, 46.0f/255.0f);
				sprite.SetSprite(sprite.atlas, sprite.atlas.GetIndexByName("node-red"), true);
			}
			else if (node.nodeType == Node.NodeType.Mid) {
				sprite.SetSprite(sprite.atlas, sprite.atlas.GetIndexByName("node-neutral"), true);
				//sprite.color = new Color(6.0f/255.0f,153.0f/255.0f, 69.0f/255.0f);
			}
			else {
				//sprite.color = Color.clear;
				sprite.gameObject.SetActive(false);
			}
		}
		
		//Horizonts
		for (int i = 0; i < nodeCount * nodeCount; i++) {
			
			bool no = (i + 1) % (gridCount + 1) == 0;
			bool beginning = i < nodeCount;
			bool ending = i > (nodeCount * nodeCount) - nodeCount - 1;
			
			if ( no || beginning || ending )
				continue;
			Vector2 start 	= node2WorldPoint(i);
			Vector2 end		= node2WorldPoint(i+1);
			
			Vector3[] points = {
				new Vector3(start.x, start.y, 0.0f),
				new Vector3(end.x, end.y, 0.0f)
			};
			
			VectorLine vl = new VectorLine(i.ToString(), points, neutralLineMaterial, 20.0f);
			vl.Draw3D();
			
			Node n1 = nodes[i];
			Node n2 = nodes[i+1];
			
			GameObject highway = (GameObject) Instantiate(HighwayPrefab);
			Highway hwy = highway.GetComponent<Highway>();
			hwy.Node1 = n1;
			hwy.Node2 = n2;
			hwy.grid = this;
			
			vl.vectorObject.transform.parent = highway.transform;
			highway.transform.parent = transform;
		}
		
		//Vertics
		for (int i = 0; i < (nodeCount * nodeCount) - 4; i++) {
			
			//bool no = (i + 1) % (gridCount + 1) == 0;
			bool beginning = i % nodeCount == 0;
			bool ending = i % nodeCount == (nodeCount - 1);
			
			if (beginning || ending) {
				continue;	
			}
			
			Vector2 start 	= node2WorldPoint(i);
			Vector2 end		= node2WorldPoint(i+nodeCount);
			
			Vector3[] points = {
				new Vector3(start.x, start.y, 0.0f),
				new Vector3(end.x, end.y, 0.0f)
			};
			
			VectorLine vl = new VectorLine(i.ToString(), points, neutralLineMaterial, 20.0f);
			vl.Draw3D();
			
			Node n1 = nodes[i];
			Node n2 = nodes[i+nodeCount];
			
			GameObject highway = (GameObject) Instantiate(HighwayPrefab);
			Highway hwy = highway.GetComponent<Highway>();
			hwy.Node1 = n1;
			hwy.Node2 = n2;
			hwy.grid = this;
			
			vl.vectorObject.transform.parent = highway.transform;
			highway.transform.parent = transform;
		}
		
		// Cells
		for (int i = 0; i < gridCount * gridCount; i++) {
			Rect rect = cell2Bounds(i);
			
			//Draw and Create
			GameObject go = (GameObject) GameObject.Instantiate(CellPrefab, new Vector3(rect.center.x, rect.center.y, 1.0f), CellPrefab.transform.rotation);
			Cell cell = go.GetComponent<Cell>();
			cell.grid = this;
			cell.name = "Cell_"+i;
			cell.number = i;
			
			exSprite sprite = go.GetComponent<exSprite>();
			sprite.customSize = true;
			sprite.width = Mathf.Min(sprite.width, gridSize);
			sprite.height = Mathf.Min(sprite.height, gridSize);
			sprite.Commit();
			cells.Add (i, cell);
			
			go.transform.parent = transform;
		}
		
		Rect ORect = cell2Bounds( (gridCount * gridCount) - gridCount);
		Rect XRect = cell2Bounds( gridCount - 1 );
		
		FocusCameraOnGameObject(Camera.mainCamera, gameObject);
		
		GameObject OBase = (GameObject) Instantiate(OBasePrefab, new Vector3(ORect.xMin, ORect.yMax, 0.0f), OBasePrefab.transform.rotation);
		GameObject XBase = (GameObject) Instantiate(XBasePrefab, new Vector3(XRect.xMax, XRect.yMin, 0.0f), XBasePrefab.transform.rotation);
		
		redBase = OBase.GetComponent<HomeBase>();
		blueBase = XBase.GetComponent<HomeBase>();
		
		redBase.grid = this;
		blueBase.grid = this;
		
		OBase.transform.parent = transform;
		XBase.transform.parent = transform;
		
		BlueAI blue = GameObject.Find("BlueAI").GetComponent<BlueAI>();
		blue.homeBase = team == Unit.Team.RED ? blueBase : redBase;
		blue.enabled = true;
		blue.AIOn = true;
		blue.Renew();
	}
	
	public void CheckForWin() {
		Cell c0 = cells[0], c1 = cells[1], c2 = cells[2],
			  c3 = cells[3], c4 = cells[4], c5 = cells[5],
			  c6 = cells[6], c7 = cells[7], c8 = cells[8];
		
		if (CheckTripletForWin(c6, c7, c8)) { GameFinished(c6); return; }
		if (CheckTripletForWin(c3, c4, c5)) { GameFinished(c3); return; }
		if (CheckTripletForWin(c0, c1, c2)) { GameFinished(c0); return; }
		if (CheckTripletForWin(c6, c3, c0)) { GameFinished(c0); return; }
		if (CheckTripletForWin(c7, c4, c1)) { GameFinished(c7); return; }
		if (CheckTripletForWin(c8, c5, c2)) { GameFinished(c8); return; }
		if (CheckTripletForWin(c6, c5, c2)) { GameFinished(c6); return; }
		if (CheckTripletForWin(c8, c4, c0)) { GameFinished(c8); return; }
		
	}
		
	void GameFinished(Cell c) {
		BlueAI blue = GameObject.Find("BlueAI").GetComponent<BlueAI>();
		if (blue.enabled) {
			string team 	= c.OwningTeam() == Unit.Team.RED ? "Red" : "Blue";
			winLabel.text 		= team + " Wins!";
			
			winLabel.color 	= c.OwningTeam() == Unit.Team.RED ? 
									new Color(201.0f/255.0f,41.0f/255.0f, 46.0f/255.0f) : 
									new Color(57.0f/255.0f,154.0f/255.0f, 196.0f/255.0f);
			
			blue.AIOn = false;
			blue.Renew();
			blue.enabled = false;
			
			DestroyEverything();
			winContainer.SetActive(true);
		}
	}
	
	void DestroyEverything() {
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; i++) {
			Transform t = transform.GetChild(0);
			t.parent = null;
			Destroy(t.gameObject);
		}	
	}
	
	bool CheckTripletForWin(Cell c1, Cell c2, Cell c3) {		
		return c1.OwningTeam() == c2.OwningTeam() && 
			   c2.OwningTeam() == c3.OwningTeam() && 
			   c1.OwningTeam() == c3.OwningTeam() &&
			   c1.OwningTeam() != Unit.Team.NEUTRAL;
		 
	}
	
	void UpdateDrawnPath() {
		if (_DrawnPath == null && _MidNodes.Count > 0) {
			List<Vector3> trans = new List<Vector3>();
			foreach (Node n in _MidNodes){
				trans.Add(n.transform.position);	
			}
			
			_DrawnPath = new VectorLine("test", trans.ToArray(), Color.yellow, null, 2.0f, LineType.Continuous);
		}
		
		if (_DrawnPath != null) {
			List<Vector3> trans = new List<Vector3>();
			foreach (Node n in _MidNodes){
				trans.Add(n.transform.position);	
			}
			
			_DrawnPath.Resize(trans.ToArray());
			_DrawnPath.Draw3D();
			
			Vector3 pos = _DrawnPath.vectorObject.transform.position;
			pos.z = -1.0f;
			_DrawnPath.vectorObject.transform.position = pos;
		}
	}
	
	public Vector2 node2WorldPoint (int node) {
		float x = (node % nodeCount) * gridSize;
		float y = gridSize * Mathf.Floor(node/nodeCount);
		return new Vector2(x, y);
	}
	
	public Vector2 node2GridPoint (int node) {
		//float x = (node % nodeCount) * gridSize;
		//float y = gridSize * Mathf.Floor(node/nodeCount);
		return new Vector2(node%nodeCount, Mathf.Floor(node/nodeCount));
	}
	
	public int point2Node (Vector2 point) {
		float x = point.x / gridSize;
		float y = point.y / gridSize;
		return (int)  (x + (y * nodeCount));
	}
	
	public int point2Cell (Vector2 point) {
		float x = Mathf.Floor(point.x/gridSize);	
		float y = Mathf.Floor(point.y/gridSize);
		return (int) (x + (y * gridCount));
	}
	
	public Rect cell2Bounds (int cell) {
		float x = (gridSize * (cell%gridCount));
		float y = (gridSize * ( (cell/gridCount) - 1));
		
		//float x2 = x + gridSize;
		float y2 = y + gridSize;
		
		return new Rect(x, y2, gridSize, gridSize);
	}
	
	Bounds CalculateBounds(GameObject go) {
        Bounds b = new Bounds(go.transform.position, Vector3.zero);
        Object[] rList = go.GetComponentsInChildren(typeof(Renderer));
        foreach (Renderer r in rList) {
            b.Encapsulate(r.bounds);
        }
        return b;
    }
 
    void FocusCameraOnGameObject(Camera c, GameObject go) {
        Bounds b = CalculateBounds(go);
        Vector3 max = b.size;
        //c.orthographicSize = gridSize;
		c.orthographicSize = max.y * 0.8f;
	
		Vector3 pos = c.transform.position;
		pos.x = (gridCount * gridSize) / 2.0f;
		pos.y = (gridCount * gridSize) / 2.0f;
		c.transform.position = pos;
    }
	
	public List<Node> nodesForTeam(Unit.Team team) {
		if (team == Unit.Team.RED && redSpawnNodes != null) {
			return redSpawnNodes;
		}
		else if (team == Unit.Team.BLUE && blueSpawnNodes != null){
			return blueSpawnNodes;				
		}
		
		List<Node> teamNodes = new List<Node>();
		for (int i = 0; i < nodeCount * nodeCount; i++) {
			Node n = nodes[i];
			if (n.nodeType == Node.NodeType.RedSpawn && team == Unit.Team.RED) {
				teamNodes.Add(n);	
			}
			else if (n.nodeType == Node.NodeType.BlueSpawn && team == Unit.Team.BLUE) {
				teamNodes.Add(n);
			}
		}
		
		if (team == Unit.Team.RED) {
			redSpawnNodes = teamNodes;	
		}
		else {
			blueSpawnNodes = teamNodes;				
		}
		
		return teamNodes;
	}
}
