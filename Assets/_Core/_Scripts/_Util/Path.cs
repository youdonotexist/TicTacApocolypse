using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Path
{	
	public List<Node> points;
	public Cell cell;
	
	//Start Path
	int currentIndex = 0;
	
	public Path(List<Node> nodes, Cell last) {
		points = nodes;
		cell = last;
	}
	
	public Node[] GetTimestampPoints() {
		Node[] pts = points.ToArray();
		return points.ToArray();	
	}
	
	public Vector3[] GetPoints() {
		List<Vector3> v3Points = new List<Vector3>();
		foreach (Node pt in points) {
			v3Points.Add(pt.transform.position);
		}
		
		return v3Points.ToArray();
	}
	
	public Node Begin() {
		if (points.Count > 0) {
			return points[currentIndex];	
		}
		
		return null;
	}
	
	public Node Next() {
		if (points.Count > 0 && currentIndex + 1 < points.Count) {
			return points[++currentIndex];
		}
		
		return null;
	}
	
	public bool AtEnd() {
		return currentIndex >= points.Count - 1;	
	}
}

