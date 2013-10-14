using UnityEngine;
using System.Collections;

public class AIPathNode : MonoBehaviour
{
	
	public AIPath _parent;
	private Transform _transform;
	
	public enum PATH_NODE_TYPE {
		START,
		END,
		MID
	}
	
	public Transform Transform{ 
		get {
			if (_transform == null)
				_transform = gameObject.transform;
			
			return _transform;
		}
	}
	
	public PATH_NODE_TYPE _nodeType = PATH_NODE_TYPE.MID;
	
	// Use this for initialization
	void Start ()
	{
		
	}

	// Update is called once per frame
	void Update ()
	{
		
	}
	
	void OnBecameVisible() {
		Debug.Log("Worked..!");	
	}
	
	public void SetParent() {
		if (_nodeType == AIPathNode.PATH_NODE_TYPE.MID) {
			Transform parent = transform.parent.parent;
			AIPath p = parent.GetComponent<AIPath>();
			if (p != null)
				_parent = p;
		}
		else {
			Transform parent = transform.parent;
			AIPath p = parent.GetComponent<AIPath>();
			if (p != null) {
				_parent = p;
			}
			else {
				Debug.Log("No Path?");	
			}
		}
	}
}

