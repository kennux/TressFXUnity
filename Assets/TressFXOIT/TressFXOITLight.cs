using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace TressFX
{
    [RequireComponent(typeof(Light))]
    public class TressFXOITLight : MonoBehaviour
    {
        public enum SelfShadowMode { Off, Lines, Triangles }
        #region Instances

        public static List<TressFXOITLight> lights
        {
            get
            {
                if (lightListDirty)
                {
                    _sortedLights = _lights.OrderBy(s => s.importance).ToList();
                }

                return _sortedLights;
            }
        }

        /// <summary>
        /// Collection of all lights.
        /// This collection is getting sorted in <see cref="lights"/> if <see cref="lightListDirty"/> is true.
        /// </summary>
        private static List<TressFXOITLight> _sortedLights;

        /// <summary>
        /// Collection of all lights. Not sorted!
        /// When a new light gets registered, <see cref="lightListDirty"/> is set to false.
        /// </summary>
        public static List<TressFXOITLight> _lights = new List<TressFXOITLight>();
        private static bool lightListDirty;

        #endregion

        /// <summary>
        /// Will this light generate a self shadow map?
        /// </summary>
        public bool selfShadowing
        {
            get
            {
                return selfShadowMode != SelfShadowMode.Off;
            }
        }

        [Header("General shadows")]
        public float focusDistance = 250;
        public float sceneCaptureDistance = 250;

        [Header("Self-Shadows")]
        public SelfShadowMode selfShadowMode;
        public int selfShadowMapSize = 1024;
        public float fiberSpacing = 0.005f;

        [Header("Shadows")]
        /// <summary>
        /// True means this light renders shadow maps.
        /// </summary>
        public bool shadows;
        public int shadowMapSize = 1024;
        public float shadowBias = 0.01f;
        [Range(0,1)]
        public float shadowStrength = 1;

        [Header("Config")]
        /// <summary>
        /// The importance of this light.
        /// Higher means the renderer will give it a higher priority.
        /// </summary>
        public int importance;

        /// <summary>
        /// The light.
        /// </summary>
        private new Light light;

        [Header("Directional shadows")]
        public float blockerSearchDistance = 24;
        public float fallbackFilterWidth = 6;
        public float blockerDistanceScale = 1;
        public float lightNearSize = 4f;
        public float lightFarSize = 22f;


        /// <summary>
        /// A material used for rendering hair depth.
        /// This is used to render shadow maps.
        /// </summary>
        public static Material depthPassMaterial
        {
            get
            {
                if (_depthPassMaterial == null)
                {
                    _depthPassMaterial = new Material(Shader.Find("Hidden/TressFX/DepthMask"));
                }
                return _depthPassMaterial;
            }
        }
        private static Material _depthPassMaterial;

        public static Shader sceneShadowReplacementShader
        {
            get
            {
                if (_sceneShadowReplacementShader == null)
                    _sceneShadowReplacementShader = Shader.Find("Hidden/TressFX/SceneShadowShader");
                return _sceneShadowReplacementShader;
            }
        }
        private static Shader _sceneShadowReplacementShader;

        /// <summary>
        /// "Fake-point" texture which is 1x1 of size and has a single alpha channel.
        /// </summary>
        public static Texture2D fakePointTexture
        {
            get
            {
                if (_fakePointText == null)
                {
                    // Init
                    _fakePointText = new Texture2D(1, 1, TextureFormat.Alpha8, false, true);
                    _fakePointText.filterMode = FilterMode.Point;
                    _fakePointText.SetPixel(0, 0, new Color(0f, 0f, 0f, 0f));
                    _fakePointText.Apply(false, true);
                }

                return _fakePointText;
            }
        }
        private static Texture2D _fakePointText;

        /// <summary>
        /// This camera is used for rendering shadow maps of this light.
        /// Null if no shadows are active.
        /// </summary>
        private Camera shadowMappingCamera;

        [HideInInspector]
        public RenderTexture selfShadowMap;

        [HideInInspector]
        public RenderTexture shadowMap;

        [Header("Debug")]
        // Shadow focus
        public Vector3 shadowFocusPosition;

        public Matrix4x4 GetShadowMatrix()
        {
            var m_shadowSpaceMatrix = new Matrix4x4();
            var isD3D9 = SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D9;
            var isD3D = isD3D9 || SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D11;
            float to = isD3D9 ? 0.5f / (float)selfShadowMapSize : 0f;
            float zs = isD3D ? 1f : 0.5f, zo = isD3D ? 0f : 0.5f;
            float db = -this.shadowBias; // TODO: Real bias
            m_shadowSpaceMatrix.SetRow(0, new Vector4(0.5f, 0.0f, 0.0f, 0.5f + to));
            m_shadowSpaceMatrix.SetRow(1, new Vector4(0.0f, 0.5f, 0.0f, 0.5f + to));
            m_shadowSpaceMatrix.SetRow(2, new Vector4(0.0f, 0.0f, zs, zo + db));
            m_shadowSpaceMatrix.SetRow(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));

            var shadowViewMat = this.shadowMappingCamera.worldToCameraMatrix;
            var shadowProjMat = GL.GetGPUProjectionMatrix(this.shadowMappingCamera.projectionMatrix, false);
            return m_shadowSpaceMatrix * shadowProjMat * shadowViewMat;
        }

        /// <summary>
        /// Sets the self shadowmapping parameters of this light to the specified evaluation material.
        /// </summary>
        /// <param name="evalMaterial"></param>
        public void SetSelfShadowParameters(Material evalMaterial)
        {
            if (this.shadowMappingCamera == null)
            {
                Debug.LogError("SetSelfShadowParameters was called on a light which does not cast self shadows!");
                return;
            }

            // Far-/near clipping plane
            evalMaterial.SetVector("_SelfShadowFarNear", new Vector2(this.shadowMappingCamera.farClipPlane, this.shadowMappingCamera.nearClipPlane));

            // Calculate shadow matrix

            // Set shadow matrix
            evalMaterial.SetMatrix("_SelfShadowMatrix", GetShadowMatrix());

            // Set other parameters
            evalMaterial.SetTexture("_SelfShadowMap", this.selfShadowMap);
            evalMaterial.SetFloat("_SelfShadowFiberSpacing", fiberSpacing); // TODO
        }
        
        /// <summary>
        /// Initializes the oit light.
        /// </summary>
        public void Awake()
        {
            this.light = this.GetComponent<Light>();
            if (this.light.type == LightType.Directional)
            {
                _lights.Add(this);
                lightListDirty = true;
            }
            else
            {
                this.enabled = false;
                Debug.LogError("Currently only directional lights are supported :'(");
            }

            // Shadow mapping camera
            if (this.selfShadowing || this.shadows)
            {
                // Create shadow camera
                GameObject cameraGO = new GameObject("TressFX ShadowMappingCam " + this.light);
                this.shadowMappingCamera = cameraGO.AddComponent<Camera>();
                cameraGO.hideFlags = HideFlags.DontSave;// | HideFlags.NotEditable | HideFlags.HideInHierarchy;
                this.shadowMappingCamera.renderingPath = RenderingPath.Forward;
                this.shadowMappingCamera.clearFlags = CameraClearFlags.Nothing;
                this.shadowMappingCamera.depthTextureMode = DepthTextureMode.None;
                this.shadowMappingCamera.useOcclusionCulling = false;
                this.shadowMappingCamera.orthographic = true;
                this.shadowMappingCamera.depth = -100;
                this.shadowMappingCamera.aspect = 1f;
                this.shadowMappingCamera.enabled = false;

                // Shadow textures?
                if (this.selfShadowing)
                {
                    // Self shadow texture
                    this.selfShadowMap = new RenderTexture(this.selfShadowMapSize, this.selfShadowMapSize, 16, RenderTextureFormat.Shadowmap, RenderTextureReadWrite.Linear);
                    this.selfShadowMap.filterMode = FilterMode.Bilinear;
                    this.selfShadowMap.useMipMap = false;
                    this.selfShadowMap.generateMips = false;
                    this.selfShadowMap.Create();
                }
                if (this.shadows)
                {
                    // Shadow texture
                    this.shadowMap = new RenderTexture(this.shadowMapSize, this.shadowMapSize, 16, RenderTextureFormat.Shadowmap, RenderTextureReadWrite.Linear);
                    this.shadowMap.filterMode = FilterMode.Bilinear;
                    this.shadowMap.useMipMap = false;
                    this.shadowMap.generateMips = false;
                    this.shadowMap.Create();
                }
            }
        }

        public bool IsVisible(TressFXOITRenderer renderer)
        {
            if (this.light.type == LightType.Directional)
                return true; // Directional lights are global
            return false;
        }

        public void Update()
        {
            if (this.shadowMappingCamera != null)
            {
                switch (this.light.type)
                {
                    case LightType.Directional:
                        {
                            this.shadowMappingCamera.transform.rotation = this.transform.rotation;
                            this.shadowMappingCamera.transform.position = this.shadowFocusPosition + (-this.transform.forward * focusDistance); // TODO: Correct focus distance!
                        }
                        break;
                    default:
                        {
                            Debug.LogError("Shadow mapping camera on a light which does not support shadowmapping detected!");
                        }
                        break;
                }

                // Prepare cam
                this.shadowMappingCamera.nearClipPlane = -this.sceneCaptureDistance;
                this.shadowMappingCamera.farClipPlane = focusDistance * 2f;
                this.shadowMappingCamera.orthographicSize = focusDistance;
                this.shadowMappingCamera.projectionMatrix = GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(-focusDistance, focusDistance, -focusDistance, focusDistance, 0f, focusDistance * 2f), false);
            }
        }

        /// <summary>
        /// Clears all shadowmaps this light has.
        /// </summary>
        public void ClearShadowMaps()
        {
            if (this.shadowMap != null)
            {
                Graphics.SetRenderTarget(this.shadowMap);
                GL.Clear(true, true, Color.black);
            }
            if (this.selfShadowMap != null)
            {
                Graphics.SetRenderTarget(this.selfShadowMap);
                GL.Clear(true, true, Color.black);
            }
        }

        /// <summary>
        /// Renders the scene shadow map for this light.
        /// </summary>
        public void RenderShadows()
        {
            this.shadowMappingCamera.targetTexture = this.shadowMap;
            this.shadowMappingCamera.SetReplacementShader(sceneShadowReplacementShader, null);
            this.shadowMappingCamera.cullingMask = int.MaxValue;
            this.shadowMappingCamera.Render();
        }

        /// <summary>
        /// Renders the selfshadow map for this light and the specified renderer.
        /// </summary>
        public void RenderSelfShadows(TressFXOITRenderer renderer)
        {
            if (this.shadowMappingCamera == null)
            {
                Debug.LogError("Shadow mapping camera is null but shadow map rendering was requested!");
                return;
            }

            // Prepare material
            Material mat = depthPassMaterial;
            renderer.SetShaderParams(mat);

            // Create command buffer for self shadows
            CommandBuffer selfShadowCommandBuffer = new CommandBuffer();
            selfShadowCommandBuffer.name = "TressFX SelfShadows";
            selfShadowCommandBuffer.SetRenderTarget(new RenderTargetIdentifier(this.selfShadowMap));

            if (this.selfShadowMode == SelfShadowMode.Lines)
                selfShadowCommandBuffer.DrawProcedural(Matrix4x4.identity, depthPassMaterial, 0, MeshTopology.Lines, renderer.g_LineIndicesBuffer.count);
            else if (this.selfShadowMode == SelfShadowMode.Triangles)
                selfShadowCommandBuffer.DrawProcedural(Matrix4x4.identity, depthPassMaterial, 1, MeshTopology.Lines, renderer.master.hairData.m_TriangleIndices.Length);

            // Prepare cam & render
            var shadowDistance = QualitySettings.shadowDistance;
            QualitySettings.shadowDistance = 0f;

            this.shadowMappingCamera.AddCommandBuffer(CameraEvent.AfterEverything, selfShadowCommandBuffer);
            this.shadowMappingCamera.targetTexture = this.selfShadowMap;
            this.shadowMappingCamera.cullingMask = 0;

            // Render shadows
            this.shadowMappingCamera.Render();
            this.shadowMappingCamera.RemoveAllCommandBuffers();

            QualitySettings.shadowDistance = shadowDistance;
        }

        /// <summary>
        /// Returns the light info data for the shader (for this light).
        /// </summary>
        /// <param name="position">The position passed in the shader, for format specification look into TressFX_Lighting.cginc</param>
        /// <param name="data">The data passed in the shader, for format specification look into TressFX_Lighting.cginc</param>
        public void GetLightInfo(out Vector4 position, out Vector4 data, out Vector4 lightColor)
        {
            // Error case:
            position = Vector4.zero;
            data = new Vector4(-1, 0, 0, 0);

            if (this.light.type == LightType.Directional)
            {
                // Directional light:
                // (direction, 0). 0 identifies it as directional light.
                position = new Vector4(this.transform.forward.x, this.transform.forward.y, this.transform.forward.z, 0);
            }

            lightColor = new Vector4(this.light.color.r, this.light.color.g, this.light.color.b, this.light.color.a);
        }

        /// <summary>
        /// Gets all shadow mapping information the shader requires for a light.
        /// </summary>
        /// <param name="data">The data matrix which should get passed into the evaluation shader.</param>
        /// <param name="shadowMatrix">The shadow matrix which should get passed into the evaluation shader.</param>
        public void GetShadowInfo(out Matrix4x4 data, out Matrix4x4 shadowMatrix)
        {
            // Error case:
            shadowMatrix = Matrix4x4.identity;
            data = Matrix4x4.identity;

            // Valid?
            if (!this.shadows)
            {
                Debug.LogError("Shadow mapping info for non-shadowing light requested!");
                return;
            }

            if (this.light.type == LightType.Directional)
            {
                // Directional light
                shadowMatrix = GetShadowMatrix();
                data = new Matrix4x4();

                // We want the same 'softness' regardless of texture resolution.
                var texelsInMap = (float)(int)shadowMapSize;
                var relativeTexelSize = texelsInMap / 2048f;
                
                // Row #0: ("u_UniqueShadowFilterWidth", 0, 0)
                data.SetRow(0, new Vector4(1f / (float)(int)shadowMapSize, 1f / (float)(int)shadowMapSize, 0, 0) * fallbackFilterWidth * relativeTexelSize);

                // Row #1: "u_UniqueShadowBlockerWidth"
                var uniqueShadowBlockerWidth = relativeTexelSize * blockerSearchDistance / texelsInMap;
                data.SetRow(1, Vector4.one * uniqueShadowBlockerWidth);

                // Row #2: ("u_UniqueShadowLightWidth", 0, 0)
                var uniqueShadowLightWidth = new Vector2(lightNearSize, lightFarSize) * relativeTexelSize / texelsInMap;
                data.SetRow(2, uniqueShadowLightWidth);

                // Row #3: ("u_UniqueShadowBlockerDistanceScale", shadowStrength, 0, 0)
                var uniqueShadowBlockerDistanceScale = blockerDistanceScale * focusDistance * 0.5f / 10f; // 10 samples in shader
                data.SetRow(3, new Vector4(uniqueShadowBlockerDistanceScale, this.shadowStrength, 0, 0));
            }
        }

        public void OnDestroy()
        {
            if (this.shadowMappingCamera != null)
                DestroyImmediate(this.shadowMappingCamera);
        }
    }
}