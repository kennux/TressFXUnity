using UnityEngine;
using System.Collections;

public class TressFXPostRender : MonoBehaviour
{
	public Shader hairPostShader;
	private Material postRenderMaterial;

	private RenderTexture screenTexture;

	public void Start()
	{
		this.postRenderMaterial = new Material (this.hairPostShader);
	}

	public void OnDestroy()
	{
		UnityEngine.Object.DestroyImmediate(this.postRenderMaterial);
	}

	public void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		Debug.Log (123);
		/*RenderTexture renderTexture = RenderTexture.GetTemporary( Screen.width, Screen.height, 24 );

		RenderTexture.active = renderTexture;
		GL.Clear (false, true, Color.white);

		foreach (TressFXRender t in TressFXRender.instances)
		{
			t.RenderHair();
		}

		// this.postRenderMaterial.SetTexture ("_MainTex", src);
		this.postRenderMaterial.SetTexture ("_HairTex", renderTexture);

		Graphics.Blit (src, dest, this.postRenderMaterial);

		RenderTexture.active = null;
		RenderTexture.ReleaseTemporary (renderTexture);*/
	}
}
