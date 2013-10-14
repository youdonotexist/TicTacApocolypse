using UnityEngine;
using System.Collections;

public class HomeBase : MonoBehaviour
{
	//CurrentPath currentPath = null;
	TextMesh unitText = null;
	
	public int unitCount = 3;
	
	float unitAddDuration = 5.0f;
	float unitAddElapsed = 0.0f;
	
	public GameObject unitPrefab;
	
	public AudioClip Deploy;
	public AudioClip GeneratedUnit;
	
	public Grid grid;
	
	// Use this for initialization
	void Start ()
	{
		unitText = GetComponentInChildren<TextMesh>();
		unitText.renderer.material.color = Color.black;
		
		unitText.text = unitCount.ToString();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (unitAddElapsed > unitAddDuration) {
			unitCount++;
			unitAddElapsed = 0.0f;
			
			unitText.text = unitCount.ToString();
			
			audio.PlayOneShot(GeneratedUnit);
		}
		
		unitAddElapsed += Time.deltaTime;
	}
	
	public void DeployUnit (Path path) {
		if (unitCount > 0) {
			GameObject go = (GameObject) Instantiate(unitPrefab);
			Unit unit = go.GetComponent<Unit>();
			unit.FollowPath(path);
			unit.homeBase = this;
			unitText.text = (--unitCount).ToString();
			
			go.transform.parent = grid.transform;
			
			audio.PlayOneShot(Deploy);
		}
	}
}

