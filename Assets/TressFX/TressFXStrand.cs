using UnityEngine;
using System.Collections;

/// <summary>
/// Tress FX strand.
/// </summary>
public class TressFXStrand
{
	public TressFXVertex[] vertices;
	public int hairId;

	public TressFXStrand(int numVertices)
	{
		this.vertices = new TressFXVertex[numVertices];
	}

	public Vector4 GetTressFXVector(int index)
	{
		return new Vector4(this.vertices[index].pos.x, this.vertices[index].pos.y, this.vertices[index].pos.z, this.vertices[index].invMass);
	}
}

public struct TressFXVertex
{
	public Vector3 pos;
	public float invMass;
}