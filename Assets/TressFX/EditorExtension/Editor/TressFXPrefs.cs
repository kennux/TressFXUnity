using UnityEngine;
using UnityEditor;

public static class TressFXPrefs 
{
	private static string assetConverterPath;

	[PreferenceItem("Tress FX")]
	public static void PrefGUI()
	{
        if (assetConverterPath == null || assetConverterPath.Equals(""))
		{
			assetConverterPath = EditorPrefs.GetString("TressFXAssetConverterPath", "TressFX/assetconverter.exe");
		}

		assetConverterPath = EditorGUILayout.TextField("Asset Converter Path", assetConverterPath);

		if (GUI.changed)
		{
			EditorPrefs.SetString("TressFXAssetConverterPath", assetConverterPath);
		}
	}
}
