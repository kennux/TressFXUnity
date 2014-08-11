using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// This class is able to load tressfx mesh files.
/// Currently only text based amd tressfx mesh files are supported.
/// Binary format is planned!
/// </summary>
[RequireComponent(typeof(TressFX))]
public class TressFXLoader : MonoBehaviour
{
	/// <summary>
	/// The hair meshes.
	/// </summary>
	public TressFXHairData[] hairs;

	/// <summary>
	/// This function will generate the vertice and strand index buffers used for initializing TressFX.
	/// </summary>
	public void Start ()
	{
		// Declare lists for vertices and strand indices which will get passed to TressFX
		List<TressFXStrand> strands = new List<TressFXStrand>();

		int vertexCount = 0;

		// Load all hair files
		for (int i = 0; i < this.hairs.Length; i++)
		{
			Debug.Log ("Loading hair " + i);
			vertexCount += this.LoadHairTFX(this.hairs[i].hairModel.text, strands, i);
		}

		// Tress fx loaded!
		TressFX tressFx = this.GetComponent<TressFX>();
		if (tressFx != null)
		{
			tressFx.Initialize(strands.ToArray(), vertexCount, this.hairs.Length);
		}

	}

	/// <summary>
	/// Loads the hair tfx (text) file.
	/// This will generate the hair vertices and strandindices and store them in the passed lists.
	/// </summary>
	/// <param name="hairData">Hair mesh data (text from tfx file)</param>
	/// <param name="verticeList">The list where the vertices should go.</param>
	/// <param name="strandIndices">The list where the StrandIndices should go.</param>
	/// <param name="hairId">The HairID (starts at 0)</param>
	private int LoadHairTFX(string hairData, List<TressFXStrand> strandsList, int hairId)// string hairData, List<Vector4> verticeList, List<StrandIndex> strandIndices, List<int> verticesOffsets, int hairId) //, List<Vector3> normalList)
	{
		int vertexCount = 0;

		// Start parsing hair file
		string[] hairLines = hairData.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
		
		// Wrong line endings?
		if (hairLines.Length < 2)
		{
			hairLines = hairData.Split(new string[] { "\n" }, StringSplitOptions.None);
		}
		
		// Properties
		int numStrands = 0;

		// Read every line of the data
		int i = 0;
		while (i < hairLines.Length)
		{
			string[] stringTokens = hairLines[i].Split(' ');

			// Hair definition
			if (stringTokens[0] == "numStrands")
			{
				// Strands definition
				numStrands = int.Parse(stringTokens[1]);
			}
			// Strand definition
			else if (stringTokens[0] == "strand")
			{
				// Parse informations about the strand
				// int strandNum = int.Parse(stringTokens[1]);
				int numStrandVertices = int.Parse(stringTokens[3]);
				float texcoordX = float.Parse (stringTokens[5]);

				TressFXStrand strand = new TressFXStrand(numStrandVertices);

				// Used for corruption check
				// If a strand or just one vertex of it is corrupted it will get ignored
				bool corrupted = false;

				int j;

				// Read all vertices
				for (j = 0; j < numStrandVertices; j++)
				{
					// String tokens
					string[] vertexData = hairLines[i+1].Split (' ');
					
					// Strand corrupted?
					if (vertexData[0] == "-1.#INF" || vertexData[1] == "-1.#INF" || vertexData[2] == "-1.#INF")
					{
						corrupted = true;
						break;
					}

					// invMass = 1 means strand movable, 0 means not movable
					float invMass = 1.0f;

					if (j == 0 || j == 1 || (j == numStrandVertices-1 && this.hairs[hairId].makeBothEndsImmovable))
					{
						invMass = 0.0f;
					}

					// Set TressFX Strand data
					strand.vertices[j] = new TressFXVertex();
					strand.vertices[j].pos = new Vector3(float.Parse(vertexData[0]),		// X
					                                     float.Parse(vertexData[1]),		// Y
					                                     float.Parse(vertexData[2]));		// Z
					strand.vertices[j].pos.Scale (this.transform.lossyScale);

					// Load UVs
					strand.vertices[j].texcoords.x = texcoordX;
					strand.vertices[j].texcoords.y = (1.0f / (float)numStrandVertices) * (float)(j+1);

					strand.vertices[j].invMass = invMass;
					strand.hairId = hairId;

					i++;
				}
				
				// Corrupted strands
				if (corrupted)
				{
					// Delete this strand
					numStrands--;
				}
				else
				{
					vertexCount += j;
					strandsList.Add (strand);
				}
			}
			
			i++;
		}

		return vertexCount;
	}
}
