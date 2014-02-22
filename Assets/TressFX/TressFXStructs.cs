using UnityEngine;
using System.Collections;

public struct HairStrand
{
	public HairStrand(int vertexCount)
	{
		this.initialPosition = new Vector3[vertexCount];
		this.realPosition = new Vector3[vertexCount];
	}

	public Vector3[] initialPosition;
	public Vector3[] realPosition;
}
