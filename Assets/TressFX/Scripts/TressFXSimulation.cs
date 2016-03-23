using UnityEngine;
using System.Collections;

namespace TressFX
{
	/// <summary>
	/// Tress FX simulation implementation.
	/// Uses the AMD Simulation shader to simulate the hair strands.
	/// </summary>
	[RequireComponent(typeof(TressFX))]
	public class TressFXSimulation : MonoBehaviour
    {
        /// <summary>
        /// The size of the thread groups for compute shader dispatching.
        /// </summary
        const int THREAD_GROUP_SIZE = 64;

        /// <summary>
        /// The simulation shader.
        /// </summary>
        public ComputeShader simulationShader;

		/// <summary>
		/// The local shape constraint iterations.
		/// </summary>
		public int localShapeConstraintIterations = 1;

		/// <summary>
		/// The length constraint iterations.
		/// </summary>
		public int lengthConstraintIterations = 1;

		/// <summary>
		/// If this is set to true simulation is skipped and instead of this the hairs are just moved staticly.
		/// </summary>
		public bool isWarping;

		/// <summary>
		/// The gravity magnitude.
		/// </summary>
		public float gravityMagnitude = 9.81f;

        /// <summary>
        /// The tip seperation factor used to update follow hair vertices.
        /// </summary>
        public float tipSeperationFactor = 5.0f;

		/// <summary>
		/// If this is set to true head collision is performed.
		/// </summary>
		public bool collision = true;

		/// <summary>
		/// The integration and global shape constraints kernel identifier.
		/// </summary>
		private int IntegrationAndGlobalShapeConstraintsKernelId;

		/// <summary>
		/// The local shape constraints kernel identifier.
		/// </summary>
		private int LocalShapeConstraintsKernelId;

		/// <summary>
		/// The local shape constraints with iteration kernel identifier.
		/// </summary>
		private int LocalShapeConstraintsWithIterationKernelId;

		/// <summary>
		/// The length constriants wind and collision kernel identifier.
		/// </summary>
		private int LengthConstraintsWindAndCollisionKernelId;

		/// <summary>
		/// The update follow hair vertices kernel identifier.
		/// </summary>
		private int UpdateFollowHairVerticesKernelId;

		/// <summary>
		/// The prepare follow hair before turning into guide kernel identifier.
		/// </summary>
		private int PrepareFollowHairBeforeTurningIntoGuideKernelId;

		/// <summary>
		/// The tressfx master class.
		/// </summary>
		private TressFX master;

		/// <summary>
		/// The number of strands per thread group.
		/// </summary>
		private int numOfStrandsPerThreadGroup;

		/// <summary>
		/// The wind direction.
		/// </summary>
		public Vector3 windDirection;

		/// <summary>
		/// The wind magnitude.
		/// </summary>
		public float windMagnitude;

		/// <summary>
		/// The follow hairs flag.
		/// If this flag is checked, follow hairs will get used for the simulation.
		/// </summary>
		public bool followHairs = true;
		private bool followHairsLastFrame = false;

		[Header("Colliders")]
		public SphereCollider collider1;
		public SphereCollider collider2;
		public SphereCollider collider3;
		
		public float frameLimit = 999;

        [Header("DEBUG")]
        public bool doIntegrationAndGlobalShapeConstraints = true;
        public bool doLocalShapeConstraints = true;
        public bool doLengthConstraintsWindAndCollision = true;
        public HairPartConfig[] partConfigs;

        private Vector4 windForce1;
		private Vector4 windForce2;
		private Vector4 windForce3;
		private Vector4 windForce4;

		// Hair sim configuration
		private const int maxHairSections = 4;

		private float lastTimeSimulated = 0;

		private bool firstUpdate = true;


		public void Awake()
		{
			// No shader? :-(
			if (this.simulationShader == null)
			{
				Debug.LogError ("No simulation shader attached to TressFXSimulation :-'(");
				this.enabled = false;
				return;
			}

			// Get master
			this.master = this.GetComponent<TressFX> ();

			if (this.master == null)
			{
				Debug.LogError ("No TressFX intance attached to TressFXSimulation's gameobject :'-(");
				this.enabled = false;
				return;
			}

			if (this.master.hairData.hairPartConfig.Length > maxHairSections)
			{
				Debug.LogError ("TressFX mesh has more hair sections than allowed :'-(");
				this.enabled = false;
				return;
			}

			// Get Kernel Ids
			this.IntegrationAndGlobalShapeConstraintsKernelId = this.simulationShader.FindKernel ("IntegrationAndGlobalShapeConstraints");
			this.LocalShapeConstraintsKernelId = this.simulationShader.FindKernel ("LocalShapeConstraints");
			this.LocalShapeConstraintsWithIterationKernelId = this.simulationShader.FindKernel ("LocalShapeConstraintsWithIteration");
			this.LengthConstraintsWindAndCollisionKernelId = this.simulationShader.FindKernel ("LengthConstraintsWindAndCollision");
			this.UpdateFollowHairVerticesKernelId = this.simulationShader.FindKernel ("UpdateFollowHairVertices");
			this.PrepareFollowHairBeforeTurningIntoGuideKernelId = this.simulationShader.FindKernel ("PrepareFollowHairBeforeTurningIntoGuide");

			// Calculate num of strand per tg
			this.numOfStrandsPerThreadGroup = THREAD_GROUP_SIZE/this.master.hairData.m_NumOfVerticesPerStrand;

			this.followHairsLastFrame = this.followHairs;

            // Init part config instances
            this.partConfigs = new HairPartConfig[this.master.hairData.hairPartConfig.Length];
            System.Array.Copy(this.master.hairData.hairPartConfig, this.partConfigs, this.master.hairData.hairPartConfig.Length);
		}

		public void LateUpdate()
		{
			// Warp into origin first
			if (this.firstUpdate)
			{
				this.firstUpdate = false;

				bool warp = this.isWarping;
				this.isWarping = true;
				this.LateUpdate();
				this.isWarping = warp;
			}

			// Frame skipping if rendering too fast
			if (Time.time - this.lastTimeSimulated < 1.0f / this.frameLimit && !this.isWarping)
				return;
            
			this.lastTimeSimulated = Time.time;
			
			this.SimulateWind ();
			
			// Set constant data
			this.SetConstants ();
			
			// Set buffers to all kernels
			this.SetBuffers (this.IntegrationAndGlobalShapeConstraintsKernelId);
			this.SetBuffers (this.LocalShapeConstraintsKernelId);
			this.SetBuffers (this.LocalShapeConstraintsWithIterationKernelId);
			this.SetBuffers (this.LengthConstraintsWindAndCollisionKernelId);
			this.SetBuffers (this.UpdateFollowHairVerticesKernelId);
			this.SetBuffers (this.PrepareFollowHairBeforeTurningIntoGuideKernelId);

			// Calculate dispatch counts
			int vertexCount = (this.followHairsLastFrame ? this.master.hairData.m_NumGuideHairVertices : this.master.hairData.m_NumTotalHairVertices);
			int strandCount = (this.followHairsLastFrame ? this.master.hairData.m_NumGuideHairStrands : this.master.hairData.m_NumTotalHairStrands);
			
			int numOfGroupsForCS_VertexLevel = (int)(((float)(vertexCount) / (float)THREAD_GROUP_SIZE)*1); // * 1 = * density
			int numOfGroupsForCS_StrandLevel = (int)(((float)(strandCount) / (float)THREAD_GROUP_SIZE) *1);
			
			if (this.followHairsLastFrame && !this.followHairs)
			{
				// Prepare guide vertices
				this.simulationShader.Dispatch (this.PrepareFollowHairBeforeTurningIntoGuideKernelId, numOfGroupsForCS_VertexLevel, 1, 1);
			}
			
			// Dispatch shaders
            if (this.doIntegrationAndGlobalShapeConstraints)
			    this.simulationShader.Dispatch (this.IntegrationAndGlobalShapeConstraintsKernelId, numOfGroupsForCS_VertexLevel, 1, 1);
            if (this.doLocalShapeConstraints)
                this.simulationShader.Dispatch (this.LocalShapeConstraintsWithIterationKernelId, numOfGroupsForCS_StrandLevel, 1, 1);
            if (this.doLengthConstraintsWindAndCollision)
                this.simulationShader.Dispatch (this.LengthConstraintsWindAndCollisionKernelId, numOfGroupsForCS_VertexLevel, 1, 1);

			if (this.followHairs)
				this.simulationShader.Dispatch (this.UpdateFollowHairVerticesKernelId, numOfGroupsForCS_VertexLevel, 1, 1);

			this.followHairsLastFrame = this.followHairs;
		}

		private void SimulateWind()
		{
			// Simulate wind
			float wM = windMagnitude * (Mathf.Pow( Mathf.Sin(Time.frameCount*0.05f), 2.0f ) + 0.5f);
			
			Vector3 windDirN = this.windDirection.normalized;
			
			Vector3 XAxis = new Vector3(1,0,0);
			Vector3 xCrossW = Vector3.Cross (XAxis, windDirN);
			
			Quaternion rotFromXAxisToWindDir = Quaternion.identity;
			
			float angle = Mathf.Asin(xCrossW.magnitude);
			
			if ( angle > 0.001f )
			{
				rotFromXAxisToWindDir = Quaternion.AngleAxis(angle, xCrossW.normalized);
			}
			
			float angleToWideWindCone = 40.0f;
			
			{
				Vector3 rotAxis = new Vector3(0, 1.0f, 0);
				
				Quaternion rot = Quaternion.AngleAxis(angleToWideWindCone, rotAxis);
				Vector3 newWindDir = rotFromXAxisToWindDir * rot * XAxis; 
				this.windForce1 = new Vector4(newWindDir.x * wM, newWindDir.y * wM, newWindDir.z * wM, Time.frameCount);
			}
			
			{
				Vector3 rotAxis = new Vector3(0, -1.0f, 0);
				Quaternion rot = Quaternion.AngleAxis(angleToWideWindCone, rotAxis);
				Vector3 newWindDir = rotFromXAxisToWindDir * rot * XAxis;
				this.windForce2 = new Vector4(newWindDir.x * wM, newWindDir.y * wM, newWindDir.z * wM, Time.frameCount);
			}
			
			{
				Vector3 rotAxis = new Vector3(0, 0, 1.0f);
				Quaternion rot = Quaternion.AngleAxis(angleToWideWindCone, rotAxis);
				Vector3 newWindDir = rotFromXAxisToWindDir * rot * XAxis;
				this.windForce3 = new Vector4(newWindDir.x * wM, newWindDir.y * wM, newWindDir.z * wM, Time.frameCount);
			}
			
			{
				Vector3 rotAxis = new Vector3(0, 0, -1.0f);
				Quaternion rot = Quaternion.AngleAxis(angleToWideWindCone, rotAxis);
				Vector3 newWindDir = rotFromXAxisToWindDir * rot * XAxis;
				this.windForce4 = new Vector4(newWindDir.x * wM, newWindDir.y * wM, newWindDir.z * wM, Time.frameCount);
			}
		}

		/// <summary>
		/// Sets the buffers to the given kernel id. Sets all simulation related buffers:
		/// 
		/// g_HairVertexPositions
		/// g_HairVertexPositionsPrev
		/// g_HairVertexTangents
		/// g_InitialHairPositions
		/// g_GlobalRotations
		/// g_LocalRotations
		/// g_HairRestLengthSRV
		/// g_HairStrandType
		/// g_HairRefVecsInLocalFrame
		/// g_FollowHairRootOffset 
		/// </summary>
		/// <param name="kernelId">Kernel identifier.</param>
		protected virtual void SetBuffers(int kernelId)
		{
			this.simulationShader.SetBuffer (kernelId, "g_HairVertexPositions", this.master.g_HairVertexPositions);
			this.simulationShader.SetBuffer (kernelId, "g_HairVertexPositionsPrev", this.master.g_HairVertexPositionsPrev);
			this.simulationShader.SetBuffer (kernelId, "g_InitialHairPositions", this.master.g_InitialHairPositions);
			this.simulationShader.SetBuffer (kernelId, "g_GlobalRotations", this.master.g_GlobalRotations);
			this.simulationShader.SetBuffer (kernelId, "g_LocalRotations", this.master.g_LocalRotations);
			this.simulationShader.SetBuffer (kernelId, "g_HairRestLength", this.master.g_HairRestLengthSRV);
			this.simulationShader.SetBuffer (kernelId, "g_HairStrandType", this.master.g_HairStrandType);
			this.simulationShader.SetBuffer (kernelId, "g_HairVertexTangents", this.master.g_HairVertexTangents);
			this.simulationShader.SetBuffer (kernelId, "g_HairRefVecsInLocalFrame", this.master.g_HairRefVecsInLocalFrame);
			this.simulationShader.SetBuffer (kernelId, "g_FollowHairRootOffset", this.master.g_FollowHairRootOffset);
		}

		/// <summary>
		/// Sets the constants for the compute shader simulation.
		/// </summary>
		protected virtual void SetConstants()
		{
            // Set transform values
            Vector3 scale = new Vector3
            (
                1 / this.transform.lossyScale.x,
                1 / this.transform.lossyScale.y,
                1 / this.transform.lossyScale.z
            );

            Matrix4x4 m = Matrix4x4.TRS(Vector3.Scale(this.transform.position, scale), this.transform.rotation, Vector3.one);

			this.simulationShader.SetFloats ("g_ModelTransformForHead", this.MatrixToFloatArray (m));
			this.simulationShader.SetFloats ("g_ModelRotateForHead", this.QuaternionToFloatArray (this.transform.rotation));

			// Set wind
			this.simulationShader.SetVector ("g_Wind", this.windForce1);
			this.simulationShader.SetVector ("g_Wind1", this.windForce2);
			this.simulationShader.SetVector ("g_Wind2", this.windForce3);
			this.simulationShader.SetVector ("g_Wind3", this.windForce4);

			// Simulation values
			this.simulationShader.SetInt ("g_NumLengthConstraintIterations", this.lengthConstraintIterations);
			this.simulationShader.SetInt ("g_bCollision", this.collision ? 1 : 0);
			this.simulationShader.SetFloat ("g_GravityMagnitude", this.gravityMagnitude);
			this.simulationShader.SetFloat ("g_TimeStep", Time.deltaTime);
			this.simulationShader.SetInt ("g_NumOfStrandsPerThreadGroup", this.numOfStrandsPerThreadGroup);
			this.simulationShader.SetInt ("g_NumFollowHairsPerGuideHair", this.master.hairData.m_NumFollowHairsPerOneGuideHair);
			this.simulationShader.SetInt ("g_bWarp", this.isWarping ? 1 : 0);
            this.simulationShader.SetInt("g_NumLocalShapeMatchingIterations", this.localShapeConstraintIterations);
            this.simulationShader.SetFloat("g_TipSeparationFactor", this.tipSeperationFactor);
            this.simulationShader.SetInt("g_bSingleHeadTransform", 1);
            this.simulationShader.SetInt("g_NumVerticesPerStrand", this.master.hairData.m_NumOfVerticesPerStrand);

            // Colliders
            if (this.collider1 != null && this.collider2 != null && this.collider3 != null)
            {
                // Since the simulation always happens in uniform 1 scaling space, the colliders need to be inverse scaled in order to match the hair coordinate system
                // This is done to provide visual downscaling by like 100 times and running simulation with the higher scaled version in order to prevent precision loss
                // and simulation system instability when running with low scale hair.
                float lowestScale = Mathf.Min(this.transform.localScale.x, this.transform.localScale.y, this.transform.localScale.z);
                Vector3 scaleBk = this.transform.localScale;
                this.transform.localScale = Vector3.one;

                Vector3 c1Center = this.transform.InverseTransformPoint(collider1.transform.TransformPoint(collider1.center));
                Vector3 c2Center = this.transform.InverseTransformPoint(collider2.transform.TransformPoint(collider2.center));
                Vector3 c3Center = this.transform.InverseTransformPoint(collider3.transform.TransformPoint(collider3.center));

                this.transform.localScale = scaleBk;

                c1Center /= lowestScale;
                c2Center /= lowestScale;
                c3Center /= lowestScale;

                float r1 = this.collider1.radius / lowestScale;
                float r2 = this.collider2.radius / lowestScale;
                float r3 = this.collider3.radius / lowestScale;

                this.simulationShader.SetVector("g_cc0_center1AndRadius", new Vector4(c1Center.x, c1Center.y, c1Center.z, r1));
                this.simulationShader.SetVector("g_cc0_center2AndRadiusSquared", new Vector4(c2Center.x, c2Center.y, c2Center.z, r1*r1));
                this.simulationShader.SetVector("g_cc1_center1AndRadius", new Vector4(c2Center.x, c2Center.y, c2Center.z, r2));
                this.simulationShader.SetVector("g_cc1_center2AndRadiusSquared", new Vector4(c3Center.x, c3Center.y, c3Center.z, r2*r2));
                this.simulationShader.SetVector("g_cc2_center1AndRadius", new Vector4(c3Center.x, c3Center.y, c3Center.z, r3));
                this.simulationShader.SetVector("g_cc2_center2AndRadiusSquared", new Vector4(c1Center.x, c1Center.y, c1Center.z, r3*r3));
            }

            // Set config constants
            for (int i = 0; i < this.partConfigs.Length; i++)
			{
				this.simulationShader.SetFloat("g_Damping"+i, this.partConfigs[i].Damping);
				this.simulationShader.SetFloat("g_StiffnessForLocalShapeMatching"+i, this.partConfigs[i].StiffnessForLocalShapeMatching);
				this.simulationShader.SetFloat("g_StiffnessForGlobalShapeMatching"+i, this.partConfigs[i].StiffnessForGlobalShapeMatching);
				this.simulationShader.SetFloat("g_GlobalShapeMatchingEffectiveRange"+i, this.partConfigs[i].GlobalShapeMatchingEffectiveRange);
			}
		}

		/// <summary>
		/// Convertes a Matrix4x4 to a float array.
		/// </summary>
		/// <returns>The to float array.</returns>
		/// <param name="matrix">Matrix.</param>
		private float[] MatrixToFloatArray(Matrix4x4 matrix)
		{
			return new float[] 
			{
				matrix.m00, matrix.m01, matrix.m02, matrix.m03,
				matrix.m10, matrix.m11, matrix.m12, matrix.m13,
				matrix.m20, matrix.m21, matrix.m22, matrix.m23,
				matrix.m30, matrix.m31, matrix.m32, matrix.m33
			};
		}
		
		/// <summary>
		/// Quaternion to float array for passing to compute shader
		/// </summary>
		/// <returns>The to float array.</returns>
		/// <param name="quaternion">Quaternion.</param>
		private float[] QuaternionToFloatArray(Quaternion quaternion)
		{
			return new float[]
			{
				quaternion.x,
				quaternion.y,
				quaternion.z,
				quaternion.w
			};
		}
	}
}
