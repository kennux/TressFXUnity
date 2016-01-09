using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using TressFXLib;

namespace TressFX
{
	public static class TressFXHairEditorExt
	{
		/// <summary>
		/// Opens the hair data (tfxb) at the given path.
		/// </summary>
		/// <param name="path">Path.</param>
		public static void LoadHairData(this TressFXHair ext, Hair hair)
		{
			#if UNITY_EDITOR
			EditorUtility.DisplayProgressBar("Importing TressFX Hair", "Loading information...", 0);
			HairSimulationData hairSimulationData = hair.hairSimulationData;
			
			// Load information variables
			ext.m_NumTotalHairVertices = hairSimulationData.vertexCount;
			ext.m_NumTotalHairStrands = hairSimulationData.strandCount;
			ext.m_NumOfVerticesPerStrand = hairSimulationData.maxNumVerticesPerStrand;
			ext.m_NumGuideHairVertices = hairSimulationData.guideHairVertexCount;
			ext.m_NumGuideHairStrands = hairSimulationData.guideHairStrandCount;
			ext.m_NumFollowHairsPerOneGuideHair = hairSimulationData.followHairsPerOneGuideHair;
			
			// Load actual hair data
			EditorUtility.DisplayProgressBar("Importing TressFX Hair", "Loading strandtypes...", 0);
			ext.m_pHairStrandType = hairSimulationData.strandTypes.ToArray();
			EditorUtility.DisplayProgressBar("Importing TressFX Hair", "Loading reference vectors...", 0.05f);
			ext.m_pRefVectors = Vector4Import(hairSimulationData.referenceVectors.ToArray());
			EditorUtility.DisplayProgressBar("Importing TressFX Hair", "Loading global rotations...", 0.15f);
			ext.m_pGlobalRotations = QuaternionsToVector4(QuaternionImport(hairSimulationData.globalRotations.ToArray()));
			EditorUtility.DisplayProgressBar("Importing TressFX Hair", "Loading local rotations...", 0.25f);
			ext.m_pLocalRotations = QuaternionsToVector4(QuaternionImport(hairSimulationData.localRotations.ToArray()));
			EditorUtility.DisplayProgressBar("Importing TressFX Hair", "Loading vertices...", 0.35f);
			ext.m_pVertices = Vector4Import(hairSimulationData.vertices.ToArray());
			EditorUtility.DisplayProgressBar("Importing TressFX Hair", "Loading tangents...", 0.4f);
			ext.m_pTangents = Vector4Import(hairSimulationData.tangents.ToArray());
			EditorUtility.DisplayProgressBar("Importing TressFX Hair", "Loading thickness coefficients...", 0.55f);
			ext.m_pThicknessCoeffs = hairSimulationData.thicknessCoefficients.ToArray();
			EditorUtility.DisplayProgressBar("Importing TressFX Hair", "Loading follow hair root offsets...", 0.65f);
			ext.m_pFollowRootOffset = Vector4Import(hairSimulationData.followRootOffsets.ToArray());
			EditorUtility.DisplayProgressBar("Importing TressFX Hair", "Loading rest lengths...", 0.7f);
			ext.m_pRestLengths = hairSimulationData.restLength.ToArray();
			
			// Determine how much hair strand types are available
			List<int> strandTypes = new List<int>();
			for (int i = 0; i < ext.m_pHairStrandType.Length; i++)
			{
				if (!strandTypes.Contains(ext.m_pHairStrandType[i]))
				{
					strandTypes.Add(ext.m_pHairStrandType[i]);
				}
			}
			
			if (ext.hairPartConfig == null || ext.hairPartConfig.Length != strandTypes.Count)
				ext.hairPartConfig = new HairPartConfig[strandTypes.Count];
			
			EditorUtility.DisplayProgressBar("Importing TressFX Hair", "Loading bounding sphere...", 0.75f);
			// Load bounding sphere
			ext.m_bSphere = new TressFXBoundingSphere (new Vector3(hair.boundingSphere.center.x, hair.boundingSphere.center.y, hair.boundingSphere.center.z), hair.boundingSphere.radius);
			
			EditorUtility.DisplayProgressBar("Importing TressFX Hair", "Loading indices...", 0.75f);
			
			// Read triangle indices
			ext.m_TriangleIndices = hair.triangleIndices;
			
			// Read line indices
			ext.m_LineIndices = hair.lineIndices;

			// Set texcoords
			ext.m_TexCoords = Vector4Import(hair.texcoords);
			
			EditorUtility.ClearProgressBar();
			
			// We are ready!
			Debug.Log ("Hair loaded. Vertices loaded: " + ext.m_NumTotalHairVertices + ", Strands: " + ext.m_NumTotalHairStrands + ", Triangle Indices: " + ext.m_TriangleIndices.Length + ", Line Indices: " + ext.m_LineIndices.Length);
			
			#endif
		}
		
		#if UNITY_EDITOR
		
		/// <summary>
		/// Quaternions to vector4 casting function.
		/// </summary>
		/// <returns>The to vector4.</returns>
		/// <param name="quaternions">Quaternions.</param>
		private static Vector4[] QuaternionsToVector4(Quaternion[] quaternions)
		{
			Vector4[] vectors = new Vector4[quaternions.Length];
			for (int i = 0; i < vectors.Length; i++)
				vectors [i] = new Vector4 (quaternions [i].x, quaternions [i].y, quaternions [i].z, quaternions [i].w);
			return vectors;
		}
		
		/// <summary>
		/// Vector3 import function.
		/// Casts the tressfx lib vectors to unity vectors.
		/// </summary>
		/// <returns>The import.</returns>
		/// <param name="vectors">Vectors.</param>
		public static Vector3[] Vector3Import(TressFXLib.Numerics.Vector3[] vectors)
		{
			Vector3[] returnVectors = new Vector3[vectors.Length];
			for (int i = 0; i < vectors.Length; i++)
				returnVectors [i] = new Vector3 (vectors [i].x, vectors [i].y, vectors [i].z);
			return returnVectors;
		}
		
		/// <summary>
		/// Vector4 import function.
		/// Casts the tressfx lib vectors to unity vectors.
		/// </summary>
		/// <returns>The import.</returns>
		/// <param name="vectors">Vectors.</param>
		public static Vector4[] Vector4Import(TressFXLib.Numerics.Vector4[] vectors)
		{
			Vector4[] returnVectors = new Vector4[vectors.Length];
			for (int i = 0; i < vectors.Length; i++)
				returnVectors [i] = new Vector4 (vectors [i].x, vectors [i].y, vectors [i].z, vectors [i].w);
			return returnVectors;
		}
		
		/// <summary>
		/// Quaternion import function.
		/// Casts the tressfx lib vectors to unity vectors.
		/// </summary>
		/// <returns>The import.</returns>
		/// <param name="vectors">Vectors.</param>
		public static Quaternion[] QuaternionImport(TressFXLib.Numerics.Quaternion[] quaternions)
		{
			Quaternion[] returnQuaternion = new Quaternion[quaternions.Length];
			for (int i = 0; i < quaternions.Length; i++)
				returnQuaternion [i] = new Quaternion (quaternions [i].x, quaternions [i].y, quaternions [i].z, quaternions [i].W);
			return returnQuaternion;
		}
		
		/// <summary>
		/// Creates a new asset.
		/// </summary>
		[MenuItem("Assets/Create/TressFX/Hair from TFXB")]
		public static void CreateAssetTFXB()
		{
			string hairfilePath = EditorUtility.OpenFilePanel ("Open TressFX Hair data", "", "tfxb");
			string hairfileName = System.IO.Path.GetFileNameWithoutExtension (hairfilePath);
			
			// Create new hair asset
			TressFXHair newHairData = ScriptableObjectUtility.CreateAsset<TressFXHair> (hairfileName);
			
			// Open hair data
			Hair hair = Hair.Import (HairFormat.TFXB, hairfilePath, TressFXEditorWindow.GetImportSettings ());

			hair.CreateUVs ();
			newHairData.LoadHairData (hair);
			
			EditorUtility.SetDirty (newHairData);
			AssetDatabase.SaveAssets ();
        }

        /// <summary>
        /// Creates a new asset.
        /// </summary>
        [MenuItem("Assets/Create/TressFX/Hair from ASE")]
        public static void CreateAssetASE()
        {
            string hairfilePath = EditorUtility.OpenFilePanel("Open ASE Hair data", "", "ase");
            string hairfileName = System.IO.Path.GetFileNameWithoutExtension(hairfilePath);

            // Create new hair asset
            TressFXHair newHairData = ScriptableObjectUtility.CreateAsset<TressFXHair>(hairfileName);

            // Open hair data
            Hair hair = Hair.Import(HairFormat.ASE, hairfilePath, TressFXEditorWindow.GetImportSettings());

            if (TressFXEditorWindow.normalizeVertexCountActive && TressFXEditorWindow.normalizeVertexCount > 2)
            {
                hair.NormalizeStrands(TressFXEditorWindow.normalizeVertexCount);
            }

            hair = hair.PrepareSimulationParamatersAssetConverter(TressFXEditorWindow.followHairCount, TressFXEditorWindow.maxRadiusAroundGuideHair, Application.dataPath + "/" + EditorPrefs.GetString("TressFXAssetConverterPath"));
            hair.CreateUVs();

            newHairData.LoadHairData(hair);

            EditorUtility.SetDirty(newHairData);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Creates a new asset.
        /// </summary>
        [MenuItem("Assets/Create/TressFX/Hair from OBJ")]
        public static void CreateAssetOBJ()
        {
            string hairfilePath = EditorUtility.OpenFilePanel("Open OBJ Hair data", "", "obj");
            string hairfileName = System.IO.Path.GetFileNameWithoutExtension(hairfilePath);

            // Create new hair asset
            TressFXHair newHairData = ScriptableObjectUtility.CreateAsset<TressFXHair>(hairfileName);

            // Open hair data
            Hair hair = Hair.Import(HairFormat.OBJ, hairfilePath, TressFXEditorWindow.GetImportSettings());

            if (TressFXEditorWindow.normalizeVertexCountActive && TressFXEditorWindow.normalizeVertexCount > 2)
            {
                hair.NormalizeStrands(TressFXEditorWindow.normalizeVertexCount);
            }

            hair = hair.PrepareSimulationParamatersAssetConverter(TressFXEditorWindow.followHairCount, TressFXEditorWindow.maxRadiusAroundGuideHair, Application.dataPath + "/" + EditorPrefs.GetString("TressFXAssetConverterPath"));
            hair.CreateUVs();

            newHairData.LoadHairData(hair);

            EditorUtility.SetDirty(newHairData);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}