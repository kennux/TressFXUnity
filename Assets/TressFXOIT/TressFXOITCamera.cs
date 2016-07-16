using UnityEngine;
using System.Collections;

namespace TressFX
{
    public class TressFXOITCamera : MonoBehaviour
    {

        // Cached components:
        public new Camera camera
        {
            get
            {
                if (this._camera == null)
                    this._camera = this.GetComponent<Camera>();
                return this._camera;
            }
        }
        private Camera _camera;

        [Header("Configuration")]
        /// <summary>
        /// How many layers per fragment are allowed?
        /// </summary>
        public int layersPerPixel = 24;

        [Header("Shaders")]
        public Shader evaluationShader;
        private Material evaluationMaterial;
        
        const int stride = 16;

        // Rendering resources:
        private RenderTexture debugTexture;
        private ComputeBuffer headBuffer;
        private ComputeBuffer fragmentBuffer;
        private uint[] headClearData;
        private int initWidth = -1, initHeight = -1;

#if UNITY_EDITOR
        public void OnValidate()
        {
            if (this.evaluationShader == null)
                this.evaluationShader = Resources.Load<Shader>("TressFXOIT/Evaluation");
        }
#endif

        public virtual void Start()
        {
            this.evaluationMaterial = new Material(this.evaluationShader);
        }

        public virtual void Update()
        {
            // Update rendering resources
            // Head texture
            if (this.headBuffer == null || this.initWidth != Screen.width || this.initHeight != Screen.height)
            {
                if (this.headBuffer != null)
                {
                    this.headBuffer.Release();
                    this.fragmentBuffer.Release();
                    Destroy(this.debugTexture);
                }

                this.headBuffer = new ComputeBuffer(Screen.width * Screen.height, 4, ComputeBufferType.GPUMemory);
                this.fragmentBuffer = new ComputeBuffer(Screen.width * Screen.height * this.layersPerPixel, stride, ComputeBufferType.Counter);
                this.debugTexture = new RenderTexture(Screen.width, Screen.height, 32, RenderTextureFormat.ARGB32);
                this.debugTexture.Create();

                this.headClearData = new uint[Screen.width * Screen.height];
                for (int i = 0; i < this.headClearData.Length; i++)
                    this.headClearData[i] = 0xFFFFFFFF; // Write nullpointers

                // Update init width and height
                this.initWidth = Screen.width;
                this.initHeight = Screen.height;
            }
        }

        /// <summary>
        /// Clears the head texture
        /// </summary>
        public virtual void OnPreRender()
        {
            // Clear head
            this.headBuffer.SetData(this.headClearData);
            this.camera.depthTextureMode |= DepthTextureMode.Depth;

            // Set RandomWrite
            Graphics.ClearRandomWriteTargets();
            Graphics.SetRandomWriteTarget(6, this.headBuffer);
            Graphics.SetRandomWriteTarget(7, this.fragmentBuffer);
        }

        /// <summary>
        /// OIT Rendering main function.
        /// </summary>
        public virtual void OnRenderObject()
        {
            if (Camera.current != this.camera)
                return;

            // Render all fill passes
            foreach (var renderer in TressFXOITRenderer.renderers)
            {
                if (renderer.enabled && renderer.gameObject.activeInHierarchy)// && renderer.IsVisible(this.camera))
                {
                    renderer.RenderFillPass();
                }
            }

            // Render shadows

            // Prepare all lights
            foreach (var light in TressFXOITLight.lights)
                light.ClearShadowMaps();

            // Get shadow focus
            float multiplicator = (1f / (float)TressFXOITRenderer.renderers.Count);
            Vector3 focus = Vector3.zero;
            foreach (var renderer in TressFXOITRenderer.renderers)
            {
                focus += renderer.worldspaceBounds.center * multiplicator;
            }

            // Set shadow focus
            foreach (var light in TressFXOITLight.lights)
                light.shadowFocusPosition = focus;

            // Render all shadows
            TressFXOITLight[] shadowLights = new TressFXOITLight[4];
            int[] shadowLightIndices = new int[4];
            TressFXOITLight selfShadowLight = null;
            int shadowLightCount = 0;

            for (int i = 0; i < TressFXOITLight.lights.Count; i++)
            {
                var light = TressFXOITLight.lights[i];

                if (shadowLightCount < shadowLights.Length && light.shadows)
                {
                    light.RenderShadows();
                    shadowLightIndices[shadowLightCount] = i;
                    shadowLights[shadowLightCount] = light;
                    shadowLightCount++;
                }

                // Render self-shadows
                if (selfShadowLight == null && light.selfShadowing)
                {
                    foreach (var renderer in TressFXOITRenderer.renderers)
                    {
                        // Self-shadowing
                        light.RenderSelfShadows(renderer);
                    }

                    light.SetSelfShadowParameters(this.evaluationMaterial);
                    selfShadowLight = light;
                }
            }

            // Clear random write
            Graphics.ClearRandomWriteTargets();
            Graphics.SetRenderTarget(null);

            // Gather light information
            Vector4[] positions = new Vector4[TressFXOITLight.lights.Count];
            Vector4[] datas = new Vector4[TressFXOITLight.lights.Count];
            Vector4[] colors = new Vector4[TressFXOITLight.lights.Count];

            for (int i = 0; i < TressFXOITLight.lights.Count; i++)
            {
                TressFXOITLight.lights[i].GetLightInfo(out positions[i], out datas[i], out colors[i]);
            }

            // Set shadow data
            Matrix4x4[] shadowMatrices = new Matrix4x4[4];
            Matrix4x4[] shadowData = new Matrix4x4[4];
            for (int i = 0; i < shadowLights.Length; i++)
            {
                if (shadowLights[i] == null)
                    continue;

                // Set light -> shadow mapping index
                datas[shadowLightIndices[i]].x = i;

                // Set data
                this.evaluationMaterial.SetTexture("_ShadowMap" + i, shadowLights[i].shadowMap);
                shadowLights[i].GetShadowInfo(out shadowData[i], out shadowMatrices[i]);
            }

            this.evaluationMaterial.SetMatrixArray("_ShadowData", shadowData);
            this.evaluationMaterial.SetMatrixArray("_ShadowMatrices", shadowMatrices);
            this.evaluationMaterial.SetInt("_ShadowLightCount", shadowLightCount);

            // Prepare evaluation material
            this.evaluationMaterial.SetBuffer("SRV_fragmentHead", this.headBuffer);
            this.evaluationMaterial.SetBuffer("SRV_fragmentData", this.fragmentBuffer);

            // TODO
            this.evaluationMaterial.SetFloat("_HairWidthMultiplier", TressFXOITRenderer.renderers[0].hairMaterial.GetFloat("_HairWidthMultiplier"));
            this.evaluationMaterial.SetFloat("_HairWidth", TressFXOITRenderer.renderers[0].hairMaterial.GetFloat("_HairWidth"));

            // Set light information
            this.evaluationMaterial.SetVectorArray("_LightPositions", positions);
            this.evaluationMaterial.SetVectorArray("_LightDatas", datas);
            this.evaluationMaterial.SetVectorArray("_LightColors", colors);
            this.evaluationMaterial.SetInt("_LightCount", positions.Length);
            this.evaluationMaterial.SetFloat("_SelfShadowStrength", TressFXOITRenderer.renderers[0].selfShadowStrength);
            this.evaluationMaterial.SetInt("_SelfShadows", selfShadowLight == null ? 0 : 1);
            
            this.evaluationMaterial.SetTexture("TextureFakePoint", TressFXOITLight.fakePointTexture);

            // Evaluate collected data
            this.evaluationMaterial.SetPass(0);
            Graphics.DrawMeshNow(OITHelper.fsqMesh, Matrix4x4.identity);
        }

        public virtual void OnDestroy()
        {
            if (this.headBuffer != null)
                this.headBuffer.Release();
            if (this.fragmentBuffer != null)
                this.fragmentBuffer.Release();
        }
    }
}