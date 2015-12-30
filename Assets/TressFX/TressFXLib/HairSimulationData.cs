using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TressFXLib.Numerics;

namespace TressFXLib
{
    /// <summary>
    /// This class holds all data needed by tressfx in order to simulate the hair.
    /// It does not contain logic and only fulfills the purpose of a "data-holder".
    /// </summary>
    public class HairSimulationData
    {
        /// <summary>
        /// The vertex count of this simulation data object.
        /// Equal to calling data.vertices.Count.
        /// </summary>
        public int vertexCount
        {
            get { return this.vertices.Count; }
        }

        /// <summary>
        /// The vertex count of this simulation data object.
        /// Equal to calling data.strandTypes.Count.
        /// </summary>
        public int strandCount
        {
            get { return this.strandTypes.Count; }
        }

        /// <summary>
        /// The maximum number of vertices per strand.
        /// </summary>
        public int maxNumVerticesPerStrand;

        /// <summary>
        /// The number of guidance hair vertices (Of all guidance hairs).
        /// </summary>
        public int guideHairVertexCount;

        /// <summary>
        /// The number of guidance hair strands.
        /// </summary>
        public int guideHairStrandCount;

        /// <summary>
        /// The number of follow hairs per one guidance hair.
        /// 
        /// "this.strandCount / this.guideHairStrands".
        /// </summary>
        public int followHairsPerOneGuideHair
        {
            get
            {
                int followHairPerStrandCount = (this.strandCount / this.guideHairStrandCount) - 1;
                return followHairPerStrandCount < 0 ? 0 : followHairPerStrandCount;
            }
        }

        /// <summary>
        /// The tressfx hair vertex position vectors (Referred to as vertices in TressFX).
        /// 
        /// The length of this array is equal to the vertex count.
        /// </summary>
        public List<Vector4> vertices = new List<Vector4>();

        /// <summary>
        /// The strand types array.
        /// This is used by tressfx in order to recognize on which hair mesh a strand is located.
        /// Based on the "mesh-index-location" of the hair the simulation and rendering shader is able to apply different material / simulation parameters.
        /// 
        /// The length of this array is equal to the strand count.
        /// </summary>
        public List<int> strandTypes = new List<int>();

        /// <summary>
        /// The reference Vectors.
        /// 
        /// The length of this array is equal to the vertex count.
        /// </summary>
        public List<Vector4> referenceVectors = new List<Vector4>();

        /// <summary>
        /// The global rotations vector.
        /// 
        /// The length of this array is equal to the vertex count.
        /// </summary>
        public List<Quaternion> globalRotations = new List<Quaternion>();

        /// <summary>
        /// The local rotations vector.
        /// 
        /// The length of this array is equal to the vertex count.
        /// </summary>
        public List<Quaternion> localRotations = new List<Quaternion>();

        /// <summary>
        /// The tangent vectors.
        /// 
        /// The length of this array is equal to the vertex count.
        /// </summary>
        public List<Vector4> tangents = new List<Vector4>();

        /// <summary>
        /// The thickness coefficients used to make the hair smaller from root to tip.
        /// 
        /// The length of this array is equal to the vertex count.
        /// </summary>
        public List<float> thicknessCoefficients = new List<float>();

        /// <summary>
        /// The rest lengths for the vertices
        /// 
        /// The length of this array is equal to the vertex count.
        /// </summary>
        public List<float> restLength = new List<float>();

        /// <summary>
        /// The follow root offset vectors.
        /// The layout of them is:
        /// (0,0,0,globalHairIndex) for a guidance hair and
        /// (guide.xyz-hair.xyz, globalGuidanceHairIndex) for a follow hair
        /// 
        /// The length of this array is equal to the strand count.
        /// </summary>
        public List<Vector4> followRootOffsets = new List<Vector4>();
    }
}
