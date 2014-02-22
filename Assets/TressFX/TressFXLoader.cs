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
	public TextAsset[] hairs;

	/// <summary>
	/// This function will generate the vertice and strand index buffers used for initializing TressFX.
	/// </summary>
	public void Start ()
	{
		// Declare lists for vertices and strand indices which will get passed to TressFX
		List<Vector3> vertices = new List<Vector3>();
		List<StrandIndex> strandIndices = new List<StrandIndex>();

		// Load all hair files
		for (int i = 0; i < this.hairs.Length; i++)
		{
			Debug.Log ("Loading hair " + i);
			this.LoadHairTFX(this.hairs[i].text, vertices, strandIndices, i);
		}

		// Tress fx loaded!
		TressFX tressFx = this.GetComponent<TressFX>();
		if (tressFx != null)
		{
			tressFx.Initialize(vertices.ToArray(), strandIndices.ToArray());
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
	private void LoadHairTFX(string hairData, List<Vector3> verticeList, List<StrandIndex> strandIndices, int hairId) //, List<Vector3> normalList)
	{
		// Start parsing hair file
		string[] hairLines = hairData.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
		
		// Properties
		int numStrands = 0; 
		int numVertices = 0;

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
				int strandNum = int.Parse(stringTokens[1]);
				int numStrandVertices = int.Parse(stringTokens[3]);

				// Used for corruption check
				// If a strand or just one vertex of it is corrupted it will get ignored
				bool corrupted = false;

				// Read all vertices
				for (int j = 0; j < numStrandVertices; j++)
				{
					// String tokens
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

					// Build strand index for the current strand vertice
					StrandIndex index = new StrandIndex();
					index.hairId = hairId;
					index.vertexId = j;
					index.vertexCountInStrand = numStrandVertices;

					strandIndices.Add (index);

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
}
