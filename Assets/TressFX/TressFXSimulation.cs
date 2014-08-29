using UnityEngine;
using System.Collections;

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
	/// The last model matrix (from last frame).
	/// </summary>
	private Matrix4x4 lastModelMatrix;

	/// <summary>
	/// The number of strands per thread group.
	/// </summary>
	private int numOfStrandsPerThreadGroup;

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
			Debug.LogError ("No TressFX intance attached to TressFXSimulation's gameobject :-'(");
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

		// Init
		this.lastModelMatrix = this.transform.localToWorldMatrix;
	}

	public void Update()
	{
		// Set constant data
		this.SetConstants ();

		// Set buffers to all kernels
		this.SetBuffers (this.IntegrationAndGlobalShapeConstraintsKernelId);
		this.SetBuffers (this.LocalShapeConstraintsKernelId);
		this.SetBuffers (this.LocalShapeConstraintsWithIterationKernelId);
		this.SetBuffers (this.LengthConstriantsWindAndCollisionKernelId);
		this.SetBuffers (this.UpdateFollowHairVerticesKernelId);
		this.SetBuffers (this.PrepareFollowHairBeforeTurningIntoGuideKernelId);

		// Dispatch shaders
		int numOfGroupsForCS_VertexLevel = (int)(((float)(this.master.hairData.m_NumGuideHairVertices) / (float)64)*1);
		int numOfGroupsForCS_StrandLevel = (int)(((float)(this.master.hairData.m_NumGuideHairStrands)/(float)64)*1);

		// this.simulationShader.Dispatch (this.PrepareFollowHairBeforeTurningIntoGuideKernelId, numOfGroupsForCS_VertexLevel, 1, 1);
		
		this.simulationShader.Dispatch (this.IntegrationAndGlobalShapeConstraintsKernelId, numOfGroupsForCS_VertexLevel, 1, 1);
		this.simulationShader.Dispatch (this.LocalShapeConstraintsKernelId, numOfGroupsForCS_StrandLevel, 1, 1);
		this.simulationShader.Dispatch (this.LengthConstriantsWindAndCollisionKernelId, numOfGroupsForCS_VertexLevel, 1, 1);
		this.simulationShader.Dispatch (this.UpdateFollowHairVerticesKernelId, numOfGroupsForCS_VertexLevel, 1, 1);
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
		this.simulationShader.SetBuffer (kernelId, "g_HairRefVecsInLocalFrame", this.master.g_HairRefVecsInLocalFrame);
		this.simulationShader.SetBuffer (kernelId, "g_FollowHairRootOffset", this.master.g_FollowHairRootOffset);
	}

	/// <summary>
	/// Sets the constants for the compute shader simulation.
	/// </summary>
	private void SetConstants()
	{
		// Set transform values
		this.simulationShader.SetFloats ("g_ModelTransformForHead", this.MatrixToFloatArray (this.transform.localToWorldMatrix));
		this.simulationShader.SetFloats ("g_ModelRotateForHead", this.QuaternionToFloatArray (this.transform.rotation));

		// Set wind (TODO)
		this.simulationShader.SetVector ("g_Wind", Vector4.zero);
		this.simulationShader.SetVector ("g_Wind1", Vector4.zero);
		this.simulationShader.SetVector ("g_Wind2", Vector4.zero);
		this.simulationShader.SetVector ("g_Wind3", Vector4.zero);

		// Simulation values
		this.simulationShader.SetInt ("g_NumLengthConstraintIterations", 4);
		this.simulationShader.SetInt ("g_bCollision", 1);
		this.simulationShader.SetFloat ("g_GravityMagnitude", 9.81f);
		this.simulationShader.SetFloat ("g_TimeStep", Time.deltaTime);
		this.numOfStrandsPerThreadGroup = /*THREAD_GROUP_SIZE*/64/this.master.hairData.m_MaxNumOfVerticesInStrand;
		this.simulationShader.SetInt ("g_NumOfStrandsPerThreadGroup", this.numOfStrandsPerThreadGroup);
		this.simulationShader.SetInt ("g_NumFollowHairsPerOneGuideHair", this.master.hairData.m_NumFollowHairsPerOneGuideHair);
		this.simulationShader.SetInt ("g_bWarp", 0);
		this.simulationShader.SetInt ("g_NumLocalShapeMatchingIterations", 4);

		// Hair values
		this.simulationShader.SetFloat ("g_Damping0", 0.25f);
		this.simulationShader.SetFloat ("g_StiffnessForLocalShapeMatching0", 1f);
		this.simulationShader.SetFloat ("g_StiffnessForGlobalShapeMatching0", 0.2f);
		this.simulationShader.SetFloat ("g_GlobalShapeMatchingEffectiveRange0", 0.3f);
		
		this.simulationShader.SetFloat ("g_Damping1", 0.25f);
		this.simulationShader.SetFloat ("g_StiffnessForLocalShapeMatching1", 1f);
		this.simulationShader.SetFloat ("g_StiffnessForGlobalShapeMatching1", 0.2f);
		this.simulationShader.SetFloat ("g_GlobalShapeMatchingEffectiveRange1", 0.3f);
		
		this.simulationShader.SetFloat ("g_Damping2", 0.02f);
		this.simulationShader.SetFloat ("g_StiffnessForLocalShapeMatching2", 0.7f);
		this.simulationShader.SetFloat ("g_StiffnessForGlobalShapeMatching2", 0f);
		this.simulationShader.SetFloat ("g_GlobalShapeMatchingEffectiveRange2", 0.0f);
		
		this.simulationShader.SetFloat ("g_Damping3", 0.1f);
		this.simulationShader.SetFloat ("g_StiffnessForLocalShapeMatching3", 1f);
		this.simulationShader.SetFloat ("g_StiffnessForGlobalShapeMatching3", 0.2f);
		this.simulationShader.SetFloat ("g_GlobalShapeMatchingEffectiveRange3", 0.3f);

		// Colliders
		this.simulationShader.SetFloat ("g_cc0_radius", 0.1f);
		this.simulationShader.SetFloat ("g_cc1_radius", 0.1f);
		this.simulationShader.SetFloat ("g_cc2_radius", 0.1f);
		this.simulationShader.SetFloat ("g_cc0_radius2", 0.1f);
		this.simulationShader.SetFloat ("g_cc1_radius2", 0.1f);
		this.simulationShader.SetFloat ("g_cc2_radius2", 0.1f);
		this.simulationShader.SetFloat ("pad", 0.1f);
		
		this.simulationShader.SetVector ("g_cc0_center", Vector3.zero);
		this.simulationShader.SetVector ("g_cc1_center", Vector3.zero);
		this.simulationShader.SetVector ("g_cc2_center", Vector3.zero);
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
