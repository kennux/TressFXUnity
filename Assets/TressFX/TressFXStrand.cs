using UnityEngine;
using System.Collections;

/// <summary>
/// Tress FX strand.
/// </summary>
public class TressFXStrand
{
	public TressFXVertex[] vertices;
	public TressFXTransform[] globalTransforms;
	public TressFXTransform[] localTransforms;
	public int hairId;
	public float strandLength;

	public TressFXStrand(int numVertices)
	{
		this.vertices = new TressFXVertex[numVertices];
		this.globalTransforms = new TressFXTransform[numVertices];
		this.localTransforms = new TressFXTransform[numVertices];
	}

	public Vector4 GetTressFXVector(int index)
	{
		return new Vector4(this.vertices[index].pos.x, this.vertices[index].pos.y, this.vertices[index].pos.z, this.vertices[index].invMass);
	}
}

/// <summary>
/// Tress FX transform.
/// Each vertex has a global and local transform.
/// </summary>
public struct TressFXTransform
{
	public Vector3 translation;
	public Quaternion rotation;

	public static TressFXTransform Multiply(TressFXTransform t1, TressFXTransform t2)
	{
		TressFXTransform ret = new TressFXTransform();
		ret.translation = t1.rotation * t2.translation + t1.translation;
		ret.rotation = t1.rotation * t2.rotation;

		return ret;
	}
}


/// <summary>
/// Tress FX vertex.
/// xyz = pos, z = invMass
/// </summary>
public struct TressFXVertex
{
	public Vector3 pos;
	public float invMass;
	public Vector4 texcoords;

	public Vector2 GetUvCoordinates()
	{
		return new Vector2(this.texcoords.x, this.texcoords.y);
	}
}