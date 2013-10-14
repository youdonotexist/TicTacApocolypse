using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Highway: MonoBehaviour
{
	public Node Node1;
	public Node Node2;
	
	public Cell Cell1;
	public Cell Cell2;
	
	public Grid grid;
	
	public Material NeutralMat;
	public Material RedMat;
	public Material BlueMat;
	
	public Unit.Team team;
	
	Renderer _renderer;
	
	// Use this for initialization
	void Start ()
	{
		List<Cell> cell1 = Node1.AdjacentCells(grid);
		List<Cell> cell2 = Node2.AdjacentCells(grid);
		List<Cell> intersect = new List<Cell>();
		
		foreach (Cell c1 in cell1) {
			if (cell2.Contains(c1)) {
				intersect.Add(c1);	
			}
		}
		
		if (intersect.Count == 2) {
			Cell1 = intersect[0];
			Cell2 = intersect[1];
		}
		else {
			Debug.Log ("Something went horribly wrong");	
		}
		
		_renderer = GetComponentInChildren<Renderer>();
		MeshCollider c = gameObject.AddComponent<MeshCollider>();
		MeshFilter filter = gameObject.GetComponentInChildren<MeshFilter>();
		c.sharedMesh = filter.mesh;
		c.isTrigger = true;
	}
	
	// Update is called once per frame
	void Update ()
	{
		UpdateVisual();
	}
	
	void UpdateVisual() {
		Unit.Team t1 = Cell1.OwningTeam();
		Unit.Team t2 = Cell2.OwningTeam();
		
		if ( (t1 == Unit.Team.BLUE && t2 == Unit.Team.RED) || 
			 (t1 == Unit.Team.RED && t2 == Unit.Team.BLUE) ||
			 (t1 == Unit.Team.NEUTRAL && t2 == Unit.Team.NEUTRAL)) {
			_renderer.material = NeutralMat;
			team = Unit.Team.NEUTRAL;
		}
		else if (t1 == t2) {
			if (t1 == Unit.Team.RED) {
				_renderer.material = RedMat;
				team = Unit.Team.RED;
			}
			else {
				_renderer.material = BlueMat;	
				team = Unit.Team.BLUE;
			}
		}
		else {
			if (t1 == Unit.Team.BLUE || t2 == Unit.Team.BLUE) {
				_renderer.material = BlueMat;
				team = Unit.Team.BLUE;
			}
			if (t1 == Unit.Team.RED || t2 == Unit.Team.RED) {
				_renderer.material = RedMat;
				team = Unit.Team.RED;
			}
		}
	}
	
	void OnTriggerStay(Collider c) {
		Unit unit = c.GetComponent<Unit>();	
		if (unit != null) {
			if (unit.team != team && team != Unit.Team.NEUTRAL) {
				unit.SlowDown();	
			}
			else {
				unit.NormalSpeed();
			}
		}
	}
}

