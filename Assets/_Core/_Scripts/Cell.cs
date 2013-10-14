using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cell : MonoBehaviour
{
	public GameObject X;
	public GameObject O;
	public Grid grid;
	
	float maxUnits = 5;
	
	public float redUnits = 0;
	public float blueUnits = 0;
	
	//Red 0 ------ 10 Blue
	public float takeOverBar = 5.0f;
	
	Vector2 maxSize;
	
	exSprite Sprite;
	exSprite XSprite;
	exSprite OSprite;
	
	public float smallestSize = 0.0f;
	public Unit.Team team;
	
	public AudioClip WinCellAudio;
	
	GameObject[] smallX;
	GameObject[] smallO;
	
	public int number;
	
	// Use this for initialization
	void Start ()
	{
		exSprite sprite = GetComponent<exSprite>();
		maxSize = new Vector2(sprite.width - 2.0f, sprite.height - 2.0f);
		
		XSprite = X.GetComponent<exSprite>();
		OSprite = O.GetComponent<exSprite>();
		Sprite 	= GetComponent<exSprite>();
		
		XSprite.width = smallestSize;
		XSprite.height = smallestSize;
		
		OSprite.width = smallestSize;
		OSprite.height = smallestSize;
		
		exSprite[] sprites = GetComponentsInChildren<exSprite>();
		List<GameObject> xs = new List<GameObject>();
		List<GameObject> os = new List<GameObject>();
		foreach (exSprite s in sprites) {
			if (s.name == "SmallO") {
				os.Add(s.gameObject);
			}
			else if (s.name == "SmallX") {
				xs.Add(s.gameObject);
			}
		}
		
		smallX = xs.ToArray();
		smallO = os.ToArray();
		
		UpdateSmallSymbols();
		
		/*string nums = "";
		foreach (Node n in AdjacentNodes(grid)) {
			nums += n.number + ", ";
		}
		
		Debug.Log ("Adjancent Nodes for " + number + "[" + nums + "]");*/
	}
	
	public void AddUnit(Unit unit) {
		if (unit.team == Unit.Team.RED) {
			if (blueUnits > 0) {
				blueUnits--;	
			}
			else {
				redUnits++;
			}
		}
		else {
			if (redUnits > 0) {
				redUnits--;	
			}
			else {
				blueUnits++;
			}
		}
		
		blueUnits = Mathf.Min (Mathf.Max (0, blueUnits), maxUnits);
		redUnits = Mathf.Min (Mathf.Max (0, redUnits), maxUnits);
		
		UpdateSmallSymbols();
	}
	
	void UpdateSmallSymbols() {
		smallO[0].SetActive(redUnits > 0);
		smallO[1].SetActive(redUnits > 1);
		smallO[2].SetActive(redUnits > 2);
		smallO[3].SetActive(redUnits > 3);
		smallO[4].SetActive(redUnits > 4);
		
		smallX[0].SetActive(blueUnits > 0);
		smallX[1].SetActive(blueUnits > 1);
		smallX[2].SetActive(blueUnits > 2);
		smallX[3].SetActive(blueUnits > 3);
		smallX[4].SetActive(blueUnits > 4);	
	}
	
	void UpdateSymbols() {
		float scale = 0.0f;
		
		if (takeOverBar > 5.0f) { //BLue, X
			float norm = takeOverBar - 5f;
			scale = norm/5.0f;
			
			float XWidth = maxSize.x * scale;
			float XHeight = maxSize.y * scale;
			
			float OWidth = maxSize.x * 0.0f;
			float OHeight = maxSize.y * 0.0f;
			
			
			XSprite.width = XWidth;//Mathf.Max (XWidth, smallestSize);
			XSprite.height = XHeight;//Mathf.Max (XHeight, smallestSize);
			
			OSprite.width = OWidth;//Mathf.Max (OWidth, smallestSize);
			OSprite.height = OHeight;//Mathf.Max (OHeight, smallestSize);
		}
		else if (takeOverBar < 5.0f) {
			float norm = 5.0f - takeOverBar;
			scale = norm/5.0f;
			
			float XWidth = maxSize.x * 0.0f;
			float XHeight = maxSize.y * 0.0f;
			
			float OWidth = maxSize.x * scale;
			float OHeight = maxSize.y * scale;
			
			
			XSprite.width = XWidth;//Mathf.Max (XWidth, smallestSize);
			XSprite.height = XHeight;//Mathf.Max (XHeight, smallestSize);
			
			OSprite.width = OWidth;//Mathf.Max (OWidth, smallestSize);
			OSprite.height = OHeight;//Mathf.Max (OHeight, smallestSize);
		}
		
		Unit.Team t = OwningTeam();
				
		if (t == Unit.Team.BLUE) {
			if (Sprite.color.r != 57.0f/255.0f) {
				audio.PlayOneShot(WinCellAudio);	
			}
			Sprite.color = new Color(57.0f/255.0f,154.0f/255.0f, 196.0f/255.0f, 0.5f);
		}
		else if (t == Unit.Team.RED){
			Sprite.color = new Color(204.0f/255.0f,46.0f/255.0f, 26.0f/255.0f, 0.5f);	
		}
		else {
			Sprite.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
		}
	}
	
	public void Highlight (bool highlight, bool selected) {}
	
	void Update() {
		if (blueUnits > redUnits) {
			takeOverBar += 0.25f * Time.deltaTime;	
		}
		else if (redUnits > blueUnits) {
			takeOverBar -= 0.25f * Time.deltaTime;	
		}
		
		takeOverBar = Mathf.Min (Mathf.Max (0.0f, takeOverBar), 10.0f);
		
		UpdateSymbols();
	}
	
	public Unit.Team OwningTeam() {
		if (takeOverBar >= 10.0f) {
			team = Unit.Team.BLUE;
		}
		else if (takeOverBar <= 0.0f) {
			team = Unit.Team.RED;
		}
		else {
			team = Unit.Team.NEUTRAL;
		}
		return team;
	}
	
	public List<Node> AdjacentNodes(Grid grid) {
		Rect r = grid.cell2Bounds(number);
		
		int ne = grid.point2Node(new Vector2(r.xMax, r.yMax));
		int nw = grid.point2Node(new Vector2(r.xMin, r.yMax));
		int sw = grid.point2Node(new Vector2(r.xMin, r.yMin));
		int se = grid.point2Node(new Vector2(r.xMax, r.yMin));
		
		List<Node> returnedCells = new List<Node>();
		returnedCells.Add (grid.nodes[ne]);
		returnedCells.Add (grid.nodes[nw]);
		returnedCells.Add (grid.nodes[sw]);
		returnedCells.Add (grid.nodes[se]);
		
		return returnedCells;
	}
}

