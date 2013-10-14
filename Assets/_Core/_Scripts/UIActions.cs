using UnityEngine;
using System.Collections;

public class UIActions : MonoBehaviour
{

	void OnClick() {
		if (UICamera.currentTouch.current.name == "Play") {
	    	Grid grid = GameObject.Find("Grid").GetComponent<Grid>();
			GameObject UI = GameObject.Find("Anchor - Center");
			UI.SetActive(false);
			grid.MakeGrid();
		}
		else {
			Grid grid = GameObject.Find("Grid").GetComponent<Grid>();
			GameObject UI = GameObject.Find("WinAnchor");	
			UI.SetActive(false);
			grid.MakeGrid();
		}
   }
}

