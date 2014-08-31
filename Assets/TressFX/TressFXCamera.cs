using UnityEngine;
using System.Collections;

/// <summary>
/// Tress FX camera script.
/// Do not attach this to any gameobject by yourself.
/// It is internally attached to the main camera.
/// </summary>
public class TressFXCamera : MonoBehaviour
{
	public void OnPostRender()
	{
		Graphics.ClearRandomWriteTargets ();

	}
}
