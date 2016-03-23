#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using TressFXLib;

namespace TressFX
{
	/// <summary>
	/// Tress FX settings editor window.
	/// </summary>
	public class TressFXEditorWindow : EditorWindow
	{
		/// <summary>
		/// Gets or sets the import scale.
		/// Synced to editor prefs.
		/// </summary>
		/// <value>The import scale.</value>
		public static Vector3 importScale
		{
			get
			{
				if (_importScale == new Vector3 (2322342, 324345436, 5567567))
				{
					if (EditorPrefs.HasKey("TRESSFX_ImportScale_X") && EditorPrefs.HasKey("TRESSFX_ImportScale_Y") && EditorPrefs.HasKey("TRESSFX_ImportScale_Z"))
						_importScale = new Vector3(EditorPrefs.GetFloat("TRESSFX_ImportScale_X"),EditorPrefs.GetFloat("TRESSFX_ImportScale_Y"),EditorPrefs.GetFloat("TRESSFX_ImportScale_Z"));
					else
						_importScale = Vector3.one * 100f; // 100,100,100 is standard
				}

				return _importScale;
			}
			set
			{
				_importScale = value;
				EditorPrefs.SetFloat("TRESSFX_ImportScale_X", _importScale.x);
				EditorPrefs.SetFloat("TRESSFX_ImportScale_Y", _importScale.y);
				EditorPrefs.SetFloat("TRESSFX_ImportScale_Z", _importScale.z);
			}
		}
		private static Vector3 _importScale = new Vector3(2322342,324345436,5567567);
		
		/// <summary>
		/// Gets or sets the normalize vertex count.
		/// </summary>
		/// <value>The normalize vertex count.</value>
		public static int normalizeVertexCount
		{
			get
			{
				if (_normalizeVertexCount == -1)
				{
					_normalizeVertexCount = EditorPrefs.GetInt("NormalizeVertexCount", 16);
				}
				
				return _normalizeVertexCount;
			}
			set
			{
				_normalizeVertexCount = value;
				EditorPrefs.SetInt("NormalizeVertexCount", _normalizeVertexCount);
			}
		}
		private static int _normalizeVertexCount = -1;
		
		/// <summary>
		/// Gets or sets the follow hair count.
		/// </summary>
		/// <value>The normalize vertex count.</value>
		public static int followHairCount
		{
			get
			{
				if (_followHairCount == -1)
				{
					_followHairCount = EditorPrefs.GetInt("FollowHairCount", 4);
				}
				
				if (_followHairCount < 0)
					_followHairCount = 0;
				
				return _followHairCount;
			}
			set
			{
				_followHairCount = value;
				EditorPrefs.SetInt("FollowHairCount", _followHairCount);
			}
		}
		private static int _followHairCount = -1;
		
		/// <summary>
		/// Gets or sets the max radius around guide hairs.
		/// </summary>
		/// <value>The normalize vertex count.</value>
		public static float maxRadiusAroundGuideHair
		{
			get
			{
				if (_maxRadiusAroundGuideHair == -1)
				{
					_maxRadiusAroundGuideHair = EditorPrefs.GetFloat("MaxRadiusAroundGuideHair", 0.5f);
				}
				
				return _maxRadiusAroundGuideHair;
			}
			set
			{
				_maxRadiusAroundGuideHair = value;
				EditorPrefs.SetFloat("MaxRadiusAroundGuideHair", _maxRadiusAroundGuideHair);
			}
		}
		private static float _maxRadiusAroundGuideHair = -1;
		
		/// <summary>
		/// Gets or sets the normalize vertex activation flag.
		/// </summary>
		/// <value>The normalize vertex count.</value>
		public static bool normalizeVertexCountActive
		{
			get
			{
				if (!_normalizeVertexCountActiveInitialized)
				{
					_normalizeVertexCountActive = EditorPrefs.GetBool("NormalizeVertexCountActive", false);
					
					_normalizeVertexCountActiveInitialized = true;
				}
				
				return _normalizeVertexCountActive;
			}
			set
			{
				_normalizeVertexCountActive = value;
				EditorPrefs.SetBool("NormalizeVertexCountActive", _normalizeVertexCountActive);
			}
		}
		private static bool _normalizeVertexCountActive = false;
		private static bool _normalizeVertexCountActiveInitialized = false;

		/// <summary>
		/// Gets the import settings for the current settings.
		/// </summary>
		/// <returns>The import settings.</returns>
		public static HairImportSettings GetImportSettings()
		{
			HairImportSettings importSettings = new HairImportSettings ();
			importSettings.scale = new TressFXLib.Numerics.Vector3(importScale.x, importScale.y, importScale.z);
			return importSettings;
		}
		
		// Add menu named "My Window" to the Window menu
		[MenuItem ("Window/TressFX Settings")]
		static void Init () {
			// Get existing open window or if none, make a new one:
			TressFXEditorWindow window = (TressFXEditorWindow)EditorWindow.GetWindow (typeof (TressFXEditorWindow));
			window.Show();
		}
		
		void OnGUI ()
		{
            EditorGUILayout.LabelField("Import Settings", EditorStyles.boldLabel);

			// Import scale field
			importScale = EditorGUILayout.Vector3Field ("Import scale", importScale);
			GUILayout.Space (20);

			// ASE-Import section
			GUILayout.Label ("Strand normalization (OBJ/ASE only)", EditorStyles.boldLabel);

			normalizeVertexCountActive = GUILayout.Toggle (normalizeVertexCountActive, "Normalize vertex count");
			if (normalizeVertexCountActive)
			{
				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Normalize vertex count to");
				normalizeVertexCount = EditorGUILayout.IntField(normalizeVertexCount);
				GUILayout.EndHorizontal();
			}

            GUILayout.Space(20);
            GUILayout.Label("Follow hair generation (OBJ/ASE only)", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal ();

			GUILayout.Label ("Follow hairs per one guidance hairs");
			followHairCount = EditorGUILayout.IntField ("", followHairCount);

			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			
			GUILayout.Label ("Follow hair max radius around guidance hairs");
			maxRadiusAroundGuideHair = EditorGUILayout.FloatField ("", maxRadiusAroundGuideHair);
			
			GUILayout.EndHorizontal ();
		}
	}
}
#endif