using UnityEngine;
using System.Collections.Generic;
using System;

public class TressFXLoader : MonoBehaviour
{
	public TextAsset[] hairs;

	// Use this for initialization
	public void Start ()
	{
		List<Vector3> vertices = new List<Vector3>();
		List<int> strandIndices = new List<int>();

		for (int i = 0; i < this.hairs.Length; i++)
		{
			Debug.Log ("Loading hair " + i);
			this.LoadHair(this.hairs[i].text, vertices, strandIndices);
		}
		
		/*for (i = 0; i < meshCount; i++)
		{
			Mesh hairMesh = new Mesh();
			hairMesh.vertices = meshVertices;
			hairMesh.subMeshCount = 1;
			
			// Inidices
			int[] indices = new int[meshVertices.Length];
			Color32[] colors = new Color32[meshVertices.Length];
			
			for (int j = 0; j < meshVertices.Length; ++j)
			{
				allVertices.Add (meshVertices[j]);
				indices[j] = j;
			}
			
			hairMesh.SetIndices (indices, MeshTopology.LineStrip, 0);
			hairMesh.normals = meshNormals;
			hairMesh.colors32 = colors;
			
			// Generate mesh gameobject
			GameObject hairObject = new GameObject();
			hairObject.AddComponent<MeshFilter>().mesh = hairMesh;
			hairObject.AddComponent<MeshRenderer>().material = this.renderer.sharedMaterial;
			hairObject.transform.parent = this.transform;
			hairObject.transform.name = "Hair Mesh";
		}*/

		// Tress fx loaded!
		TressFX tressFx = this.GetComponent<TressFX>();
		if (tressFx != null)
		{
			tressFx.Initialize(vertices.ToArray(), strandIndices.ToArray());
		}

	}

	private void LoadHair(string hairData, List<Vector3> verticeList, List<int> strandIndices) //, List<Vector3> normalList)
	{
		// Start parsing hair file
		string[] hairLines = hairData.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
		
		// Properties
		int numStrands = 0; 
		int numVertices = 0;

		int i = 0;
		while (i < hairLines.Length)
		{
			string[] stringTokens = hairLines[i].Split(' ');
			
			if (stringTokens[0] == "numStrands")
			{
				// Strands definition
				numStrands = int.Parse(stringTokens[1]);
			}
			else if (stringTokens[0] == "strand")
			{
				int strandNum = int.Parse(stringTokens[1]);
				int numStrandVertices = int.Parse(stringTokens[3]);

				bool corrupted = false;

				// Read all vertices
				for (int j = 0; j < numStrandVertices; j++)
				{
					string[] vertexData = hairLines[i+1].Split (' ');
					
					// Strand corrupted?
					if (vertexData[0] == "-1.#INF" || vertexData[1] == "-1.#INF" || vertexData[2] == "-1.#INF")
					{
						corrupted = true;
						break;
					}
					
					// Cast/Parse vertex position from string to float
					Vector3 vertexPosition = new Vector3(float.Parse(vertexData[0]),		// X
					                                     float.Parse(vertexData[1]),		// Y
					                                     float.Parse(vertexData[2]));		// Z
					
					// Add to vertice list
					verticeList.Add (vertexPosition);
					strandIndices.Add (j);

					i++;
				}
				
				// Corrupted strands
				if (corrupted)
				{
					// Delete this strand
					numStrands--;
				}
			}
			
			i++;
		}
	}

	private Color32 VertIndexToColor(int vertexIndex)
	{
		byte[] indexBytes = BitConverter.GetBytes(vertexIndex);
		return new Color32(indexBytes[0], indexBytes[1], indexBytes[3], indexBytes[4]);
	}
}
