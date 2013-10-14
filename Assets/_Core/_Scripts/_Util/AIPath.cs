using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class AIPath : MonoBehaviour {
	public GameObject[] prefabs;
	
	public GameObject nodes;
	public GameObject startNode;
	public GameObject endNode;
	
	public float spawnTime = 5.0f;
	public int spawnAmt = 1;
	
	public float spawnRadius = 20.0f;
	public float nodeRadius = 10.0f;
	
	public List<AIPathNode> _visibleNodes = null;
	public List<AIPathNode> _spawnNodes = new List<AIPathNode>();
	
	public bool updateNodes = false;
	
	public int path_index = -1;
	public bool prewarm = false;
	
	public enum PATH_TYPE {
		Loop,
		Directional
	};
	
	public PATH_TYPE pathType;
	
	public float _timeSinceLastSpawn = 0.0f;
	
	public bool drawGizmos = false;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnDrawGizmos() {
		if (drawGizmos) {
			Transform[] ts = nodes.GetComponentsInChildren<Transform>();
			List<Transform> al = new List<Transform>();
			foreach (Transform t in ts) {
				if (t != nodes.transform) {
					al.Add(t);	
					
					AIPathNode pnode = t.GetComponent<AIPathNode>();
					if (pnode == null) {
						t.gameObject.AddComponent<AIPathNode>();
					}
					
					pnode._nodeType = AIPathNode.PATH_NODE_TYPE.MID;	
					pnode.SetParent();
					
					Gizmos.DrawIcon(t.position, "pulse.png");
				}
			}
			
			if (startNode != null) {
				AIPathNode pnode = startNode.GetComponent<AIPathNode>();
				if (pnode == null) {
					startNode.AddComponent<AIPathNode>();	
				}
				pnode._nodeType = AIPathNode.PATH_NODE_TYPE.START;
				pnode.SetParent();
			}
			
			if (endNode != null)
				if (startNode != null) {
				AIPathNode pnode = endNode.GetComponent<AIPathNode>();
				if (pnode == null)
					endNode.AddComponent<AIPathNode>();
				
				pnode._nodeType = AIPathNode.PATH_NODE_TYPE.END;
				pnode.SetParent();
			}
			
			ts = al.ToArray();
			
			if (ts.Length == 0) {
				Debug.DrawLine(startNode.transform.position, endNode.transform.position, Color.green);
			}
			else {
				Debug.DrawLine(startNode.transform.position, ts[0].position, Color.green);
				Debug.DrawLine(endNode.transform.position, ts[ts.Length-1].position, Color.green);
			}
			
			for (var i = 0; i < ts.Length - 1; i++)
			{
				Debug.DrawLine(ts[i].position, ts[i+1].position, Color.green);
			}
		}
	}
	
	public void initVisibleNodes() {
		_visibleNodes = new List<AIPathNode>();	
	}
	
	public void addVisibleNodes(AIPathNode n) {
		_visibleNodes.Add(n);	
	}
}
