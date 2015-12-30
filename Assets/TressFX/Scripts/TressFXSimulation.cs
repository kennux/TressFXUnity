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
		/// The size of the thread group.
		/// </summary>
		public int threadGroupSize = 64;

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
		private int LengthConstriantsWindAndCollisionKernelId;

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
		public Vector4 windDirection;

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
		
		private Vector4 windForce1;
		private Vector4 windForce2;
		private Vector4 windForce3;
		private Vector4 windForce4;

		// Hair sim configuration
		private const int maxHairSections = 4;

		private float lastTimeSimulated = 0;

		private bool firstUpdate = true;

		public void Start()
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
			this.LengthConstriantsWindAndCollisionKernelId = this.simulationShader.FindKernel ("LengthConstriantsWindAndCollision");
			this.UpdateFollowHairVerticesKernelId = this.simulationShader.FindKernel ("UpdateFollowHairVertices");
			this.PrepareFollowHairBeforeTurningIntoGuideKernelId = this.simulationShader.FindKernel ("PrepareFollowHairBeforeTurningIntoGuide");

			// Calculate num of strand per tg
			this.numOfStrandsPerThreadGroup = this.threadGroupSize/this.master.hairData.m_NumOfVerticesPerStrand;

			this.followHairsLastFrame = this.followHairs;
		}

		public void Update()
		{
			// Warp into origin first
			if (this.firstUpdate)
			{
				this.firstUpdate = false;

				bool warp = this.isWarping;
				this.isWarping = true;
				this.Update ();
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
			this.SetBuffers (this.LengthConstriantsWindAndCollisionKernelId);
			this.SetBuffers (this.UpdateFollowHairVerticesKernelId);
			this.SetBuffers (this.PrepareFollowHairBeforeTurningIntoGuideKernelId);

			// Calculate dispatch counts
			int vertexCount = (this.followHairsLastFrame ? this.master.hairData.m_NumGuideHairVertices : this.master.hairData.m_NumTotalHairVertices);
			int strandCount = (this.followHairsLastFrame ? this.master.hairData.m_NumGuideHairStrands : this.master.hairData.m_NumTotalHairStrands);
			
			int numOfGroupsForCS_VertexLevel = (int)(((float)(vertexCount) / (float)this.threadGroupSize)*1); // * 1 = * density
			int numOfGroupsForCS_StrandLevel = (int)(((float)(strandCount) / (float)this.threadGroupSize)*1);
			
			if (this.followHairsLastFrame && !this.followHairs)
			{
				// Prepare guide vertices
				this.simulationShader.Dispatch (this.PrepareFollowHairBeforeTurningIntoGuideKernelId, numOfGroupsForCS_VertexLevel, 1, 1);
			}
			
			// Dispatch shaders
			this.simulationShader.Dispatch (this.IntegrationAndGlobalShapeConstraintsKernelId, numOfGroupsForCS_VertexLevel, 1, 1);
			this.simulationShader.Dispatch (this.LocalShapeConstraintsWithIterationKernelId, numOfGroupsForCS_StrandLevel, 1, 1);
			this.simulationShader.Dispatch (this.LengthConstriantsWindAndCollisionKernelId, numOfGroupsForCS_VertexLevel, 1, 1);

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
			
			if ( angle > 0.001 )
			{
				rotFromXAxisToWindDir = Quaternion.AngleAxis(angle, xCrossW.normalized);
			}
			
			float angleToWideWindCone = 40.0f;
			
			{
				Vector3 rotAxis = new Vector3(0, 1.0f, 0);
				
				// Radians?
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
		private void SetBuffers(int kernelId)
		{
			this.simulationShader.SetBuffer (kernelId, "g_HairVertexPositions", this.master.g_HairVertexPositions);
			this.simulationShader.SetBuffer (kernelId, "g_HairVertexPositionsPrev", this.master.g_HairVertexPositionsPrev);
			this.simulationShader.SetBuffer (kernelId, "g_InitialHairPositions", this.master.g_InitialHairPositions);
			this.simulationShader.SetBuffer (kernelId, "g_GlobalRotations", this.master.g_GlobalRotations);
			this.simulationShader.SetBuffer (kernelId, "g_LocalRotations", this.master.g_LocalRotations);
			this.simulationShader.SetBuffer (kernelId, "g_HairRestLengthSRV", this.master.g_HairRestLengthSRV);
			this.simulationShader.SetBuffer (kernelId, "g_HairStrandType", this.master.g_HairStrandType);
			this.simulationShader.SetBuffer (kernelId, "g_HairVertexTangents", this.master.g_HairVertexTangents);
			this.simulationShader.SetBuffer (kernelId, "g_HairRefVecsInLocalFrame", this.master.g_HairRefVecsInLocalFrame);
			this.simulationShader.SetBuffer (kernelId, "g_FollowHairRootOffset", this.master.g_FollowHairRootOffset);
		}

		/// <summary>
		/// Sets the constants for the compute shader simulation.
		/// </summary>
		private void SetConstants()
		{
			// Set transform values
            Matrix4x4 m = Matrix4x4.TRS(this.transform.position, this.transform.rotation, Vector3.one);

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
			this.simulationShader.SetInt ("g_NumFollowHairsPerOneGuideHair", this.master.hairData.m_NumFollowHairsPerOneGuideHair);
			this.simulationShader.SetInt ("g_bWarp", this.isWarping ? 1 : 0);
			this.simulationShader.SetInt ("g_NumLocalShapeMatchingIterations", this.localShapeConstraintIterations);

			// Colliders
			if (this.collider1 != null)
			{
				this.simulationShader.SetVector ("g_cc0_center", this.collider1.center);// new Vector3(-0.095f, 92.000f, -9.899f));
				this.simulationShader.SetFloat ("g_cc0_radius", this.collider1.radius);// 26.5f);
				this.simulationShader.SetFloat ("g_cc0_radius2", this.collider1.radius*this.collider1.radius);//  26.5f*26.5f);
				
			}
			if (this.collider2 != null)
			{
				this.simulationShader.SetVector ("g_cc1_center", this.collider2.center);// new Vector3(-0.405f, 93.707f, 5.111f));
				this.simulationShader.SetFloat ("g_cc1_radius", this.collider2.radius);//  24.113f);
				this.simulationShader.SetFloat ("g_cc1_radius2", this.collider2.radius*this.collider2.radius);//  24.113f*24.113f);
			}
			if (this.collider3 != null)
			{
				this.simulationShader.SetVector ("g_cc2_center", this.collider3.center);// new Vector3(-0.072f, 68.548f, 10.561f));
				this.simulationShader.SetFloat ("g_cc2_radius", this.collider3.radius);//  30.0f);
				this.simulationShader.SetFloat ("g_cc2_radius2", this.collider3.radius*this.collider3.radius);//  25.500f*25.500f);
			}

			// Set config constants
			for (int i = 0; i < this.master.hairData.hairPartConfig.Length; i++)
			{
				this.simulationShader.SetFloat("g_Damping"+i, this.master.hairData.hairPartConfig[i].Damping);
				this.simulationShader.SetFloat("g_StiffnessForLocalShapeMatching"+i, this.master.hairData.hairPartConfig[i].StiffnessForLocalShapeMatching);
				this.simulationShader.SetFloat("g_StiffnessForGlobalShapeMatching"+i, this.master.hairData.hairPartConfig[i].StiffnessForGlobalShapeMatching);
				this.simulationShader.SetFloat("g_GlobalShapeMatchingEffectiveRange"+i, this.master.hairData.hairPartConfig[i].GlobalShapeMatchingEffectiveRange);
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
