using UnityEngine;
using System.IO;

/// <summary>
/// Tress FX loader helper class.
/// This class contains some helper functions for reading binary tressfx hair meshes with a binary reader.
/// </summary>
public class TressFXLoader
{
	/// <summary>
	/// Reads a 3-component vector (Vector3) from the given BinaryReader reader.
	/// </summary>
	/// <returns>The vector3.</returns>
	/// <param name="reader">Reader.</param>
	public static Vector3 ReadVector3(BinaryReader reader)
	{
		return new Vector3(reader.ReadSingle (), reader.ReadSingle (), reader.ReadSingle ());
	}

	/// <summary>
	/// Reads a 4-component vector (Vector4) from the given BinaryReader reader.
	/// </summary>
	/// <returns>The vector4.</returns>
	/// <param name="reader">Reader.</param>
	public static Vector4 ReadVector4(BinaryReader reader)
	{
		return new Vector4(reader.ReadSingle (), reader.ReadSingle (), reader.ReadSingle (), reader.ReadSingle ());
	}

	/// <summary>
	/// Reads one strand vertex from the BinaryReader reader.
	/// </summary>
	/// <returns>The strand vertex.</returns>
	/// <param name="reader">Reader.</param>
	public static TressFXStrandVertex ReadStrandVertex(BinaryReader reader)
	{
		return new TressFXStrandVertex (TressFXLoader.ReadVector3 (reader), TressFXLoader.ReadVector3 (reader), TressFXLoader.ReadVector4 (reader));
	}
	
	/// <summary>
	/// Reads a 3-component vector (Vector3) array from the given BinaryReader reader.
	/// </summary>
	/// <returns>The vector3 array.</returns>
	/// <param name="reader">Reader.</param>
	/// <param name="count">The count of elements to load.</param>
	public static Vector3[] ReadVector3Array(BinaryReader reader, int count)
	{
		Vector3[] returnArray = new Vector3[count];
		
		// Load
		for (int i = 0; i < count; i++)
			returnArray [i] = TressFXLoader.ReadVector3 (reader);
		
		return returnArray;
	}
	
	/// <summary>
	/// Reads a 4-component vector (Vector4) array from the given BinaryReader reader.
	/// </summary>
	/// <returns>The vector4 array.</returns>
	/// <param name="reader">Reader.</param>
	/// <param name="count">The count of elements to load.</param>
	public static Vector4[] ReadVector4Array(BinaryReader reader, int count)
	{
		Vector4[] returnArray = new Vector4[count];
		
		// Load
		for (int i = 0; i < count; i++)
			returnArray [i] = TressFXLoader.ReadVector4 (reader);
		
		return returnArray;
	}

	/// <summary>
	/// Reads multiple strand vertex from the BinaryReader reader.
	/// </summary>
	/// <returns>The strand vertex array.</returns>
	/// <param name="reader">Reader.</param>
	/// <param name="count">The count of elements to load.</param>
	public static TressFXStrandVertex[] ReadStrandVertexArray(BinaryReader reader, int count)
	{
		TressFXStrandVertex[] returnArray = new TressFXStrandVertex[count];
		
		// Load
		for (int i = 0; i < count; i++)
			returnArray [i] = TressFXLoader.ReadStrandVertex (reader);
		
		return returnArray;
	}
	
	/// <summary>
	/// Reads an integer array from BinaryReader reader.
	/// </summary>
	/// <returns>The integer array.</returns>
	/// <param name="reader">Reader.</param>
	/// <param name="count">The count of elements to load.</param>
	public static int[] ReadIntegerArray(BinaryReader reader, int count)
	{
		int[] returnArray = new int[count];
		
		// Load
		for (int i = 0; i < count; i++)
		{
			int value = reader.ReadInt32 ();
			
			returnArray [i] = value;
		}
		
		return returnArray;
	}
	
	/// <summary>
	/// Reads an float array from BinaryReader reader.
	/// </summary>
	/// <returns>The integer array.</returns>
	/// <param name="reader">Reader.</param>
	/// <param name="count">The count of elements to load.</param>
	public static float[] ReadFloatArray(BinaryReader reader, int count)
	{
		float[] returnArray = new float[count];
		
		// Load
		for (int i = 0; i < count; i++)
			returnArray [i] = reader.ReadSingle ();
		
		return returnArray;
	}
}
