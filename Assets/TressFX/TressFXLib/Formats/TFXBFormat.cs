using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TressFXLib.Numerics;
using System.IO;

namespace TressFXLib.Formats
{
    /// <summary>
    /// TressFX Binary (TFXB) file format implementation.
    /// This format is available for import and export.
    /// </summary>
    public class TFXBFormat : IHairFormat
    {
        public virtual void Export(BinaryWriter writer, string path, Hair hair)
        {
            // Get simulation data
            HairSimulationData simulationData = hair.hairSimulationData;

            int vertexCount = hair.vertexCount;

            // Write the header
            writer.Write(vertexCount);
            writer.Write(hair.strandCount);
            writer.Write(hair.maxNumVerticesPerStrand);
            writer.Write(simulationData.guideHairVertexCount);
            writer.Write(simulationData.guideHairStrandCount);
            writer.Write(simulationData.followHairsPerOneGuideHair);

            // Write data
            foreach (int strandType in simulationData.strandTypes)
                writer.Write(strandType);

            // Reference vectors
            foreach (Vector4 refVector in simulationData.referenceVectors)
                WriteVector4(writer, refVector);

            // Global rotations
            foreach (Quaternion globalRot in simulationData.globalRotations)
                WriteQuaternion(writer, globalRot);

            // Local rotations
            foreach (Quaternion localRot in simulationData.localRotations)
                WriteQuaternion(writer, localRot);

            // Position vectors
            foreach (Vector4 vertex in simulationData.vertices)
                WriteVector4(writer, vertex);

            // Tangent vectors
            foreach (Vector4 tangent in simulationData.tangents)
                WriteVector4(writer, tangent);

            // Write binary 0 as triangle vertices as they arent used anyway.
            for (int i = 0; i < vertexCount; i++)
            {
                WriteVector3(writer, Vector3.Zero); // Positions
                WriteVector3(writer, Vector3.Zero); // Tangents
                WriteVector4(writer, Vector4.Zero); // Texcoord
            }

            // Thickness coeffs
            foreach (float thicknessCoeff in simulationData.thicknessCoefficients)
                writer.Write(thicknessCoeff);

            // Follow root offsets vectors
            foreach (Vector4 followRootOffset in simulationData.followRootOffsets)
                WriteVector4(writer, followRootOffset);

            // Rest lengths
            foreach (float restLength in simulationData.restLength)
                writer.Write(restLength);

            // Write bounding sphere
            WriteVector3(writer, hair.boundingSphere.center);
            writer.Write(hair.boundingSphere.radius);

            // Triangle indices
            writer.Write(hair.triangleIndices.Length);
            foreach (int triangleIndices in hair.triangleIndices)
                writer.Write(triangleIndices);

            // Line indices
            writer.Write(hair.lineIndices.Length);
            foreach (int lineIndices in hair.lineIndices)
                writer.Write(lineIndices);
        }

        public virtual HairMesh[] Import(BinaryReader reader, string path, Hair hair, HairImportSettings importSettings)
        {
            // TODO: Implement import settings
            HairMesh[] returnData = null;

            // Load header information
            int numVertices = reader.ReadInt32();
            int numStrands = reader.ReadInt32();

            // maxVerticesPerStrand is what amd calls it
            // Actually this should be vertices per strand, as tressfx needs a uniform vertex count on every strand
            // We will assume that the input file is has a uniform vertex count.
            // If it doesnt, the import will fail.
            int maxVerticesPerStrand = reader.ReadInt32();
            int numGuideHairVertices = reader.ReadInt32();
            int numGuideHairStrands = reader.ReadInt32();
            int numFollowHairsPerOneGuideHair = reader.ReadInt32();

            // Load hair data information
            int[] strandTypes = ReadIntegerArray(reader, numStrands);
            Vector4[] referenceVectors = ReadVector4Array(reader, numVertices);
            Quaternion[] globalRotations = ReadQuaternionArray(reader, numVertices);
            Quaternion[] localRotations = ReadQuaternionArray(reader, numVertices);
            Vector4[] vertices = ReadVector4Array(reader, numVertices);
            Vector4[] tangents = ReadVector4Array(reader, numVertices);

            // Read the triangle vertices
            // Actually those are __NEVER__ used anywhere.
            // So, we are going to skip / ignore them
            Vector3[] nonsense = ReadVector3Array(reader, numVertices);
            nonsense = ReadVector3Array(reader, numVertices);
            Vector4[] nonsense2 = ReadVector4Array(reader, numVertices);

            float[] thicknessCoefficients = ReadFloatArray(reader, numVertices);
            Vector4[] followRootOffsets = ReadVector4Array(reader, numStrands);
            float[] restLengths = ReadFloatArray(reader, numVertices);

            // Get bounding sphere
            hair.InitializeBoundingSphere(ReadVector3(reader), reader.ReadSingle());

            // Load indices
            int indexTmp = reader.ReadInt32();
            int[] triangleIndices = ReadIntegerArray(reader, indexTmp);
            indexTmp = reader.ReadInt32();
            int[] lineIndices = ReadIntegerArray(reader, indexTmp);

            // Init indices
            hair.InitializeIndices(lineIndices, triangleIndices);

            // Set simulation data
            HairSimulationData simulationData = new HairSimulationData();
            simulationData.followRootOffsets = followRootOffsets.ToList();
            simulationData.globalRotations = globalRotations.ToList();
            simulationData.guideHairStrandCount = numGuideHairStrands;
            simulationData.guideHairVertexCount = numGuideHairVertices;
            simulationData.maxNumVerticesPerStrand = maxVerticesPerStrand;
            simulationData.localRotations = localRotations.ToList();
            simulationData.referenceVectors = referenceVectors.ToList();
            simulationData.strandTypes = strandTypes.ToList();
            simulationData.tangents = tangents.ToList();
            simulationData.thicknessCoefficients = thicknessCoefficients.ToList();
            simulationData.vertices = vertices.ToList();
            simulationData.restLength = restLengths.ToList();

            hair.InitializeSimulationData(simulationData);

            // So, the data is read now.
            // Next, we are going to construct the hair meshes.
            HairMesh[] hairMeshes = new HairMesh[4];

            // Current list "pointer"
            int vertexPointer = 0;
            int strandPointer = 0;

            List<HairStrand> strandReferences = new List<HairStrand>();

            foreach (int meshIndex in strandTypes)
            {
                // Check if we got a valid strand types array
                if (meshIndex < 0 || meshIndex > 3)
                    throw new FormatException("Mesh ids (strand types) < 0 or > 3 are not supported by TressFX!");

                // Get mesh and create it if it doesnt exist
                if (hairMeshes[meshIndex] == null)
                    hairMeshes[meshIndex] = new HairMesh();
                HairMesh mesh = hairMeshes[meshIndex];

                // Create new strand
                HairStrand strand = new HairStrand();
                strandReferences.Add(strand);
                strand.followRootOffset = followRootOffsets[strandPointer];


                if (strand.followRootOffset.x != 0 || strand.followRootOffset.y != 0 || strand.followRootOffset.z != 0)
                {
                    // This is a follow hair
                    strand.guidanceStrand = strandReferences[(int)strand.followRootOffset.w];
                    strand.isGuidanceStrand = false;
                }
                else
                {
                    // This is a guidance hair
                    strand.isGuidanceStrand = true;
                }


                // Read all vertices
                for (int i = 0; i < maxVerticesPerStrand; i++)
                {
                    // Read vertex data
                    // As texcoord we will create a coordinate which projects the hair along the x-axis based on the strand index
                    // and the y-axis based on the vertex index inside of the current strand
                    HairStrandVertex vertex = new HairStrandVertex(new Vector3(vertices[vertexPointer].X, vertices[vertexPointer].Y, vertices[vertexPointer].Z),
                                                    new Vector3(tangents[vertexPointer].X, tangents[vertexPointer].Y, tangents[vertexPointer].Z),
                                                    new Vector4(strandPointer / (float)numStrands, i / (float)maxVerticesPerStrand, 0, 0));

                    // Set movable flag
                    vertex.isMovable = (vertices[vertexPointer].W > 0);

                    // Set simulation data
                    vertex.globalRotation = globalRotations[vertexPointer];
                    vertex.localRotation = localRotations[vertexPointer];
                    vertex.referenceVector = referenceVectors[vertexPointer];
                    vertex.restLength = restLengths[vertexPointer];
                    vertex.thicknessCoefficient = thicknessCoefficients[vertexPointer];

                    // Add to strand
                    strand.vertices.Add(vertex);

                    vertexPointer++;
                }

                // Add strand to mesh
                mesh.strands.Add(strand);

                strandPointer++;
            }

            // Construct return array
            List<HairMesh> meshes = new List<HairMesh>();

            for (int i = 0; i < hairMeshes.Length; i++)
                if (hairMeshes[i] == null)
                {
                    // We're done reading
                    break;
                }
                else
                {
                    // Read the mesh
                    meshes.Add(hairMeshes[i]);
                }

            returnData = hairMeshes.ToArray();

            return returnData;
        }

        #region Static helper functions

        protected static void WriteVector3(BinaryWriter writer, Vector3 vector)
        {
            writer.Write(vector.x);
            writer.Write(vector.y);
            writer.Write(vector.z);
        }
        protected static void WriteVector4(BinaryWriter writer, Vector4 vector)
        {
            writer.Write(vector.x);
            writer.Write(vector.y);
            writer.Write(vector.z);
            writer.Write(vector.w);
        }
        protected static void WriteQuaternion(BinaryWriter writer, Quaternion quaternion)
        {
            writer.Write(quaternion.x);
            writer.Write(quaternion.y);
            writer.Write(quaternion.z);
            writer.Write(quaternion.W);
        }

        /// <summary>
        /// Reads a 3-component vector (Vector3) from the given BinaryReader reader.
        /// </summary>
        /// <returns>The vector3.</returns>
        /// <param name="reader">Reader.</param>
        protected static Vector3 ReadVector3(BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        /// <summary>
        /// Reads a 4-component vector (Vector4) from the given BinaryReader reader.
        /// </summary>
        /// <returns>The vector4.</returns>
        /// <param name="reader">Reader.</param>
        protected static Vector4 ReadVector4(BinaryReader reader)
        {
            return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        /// <summary>
        /// Reads a 4-component vector (Quaternion) from the given BinaryReader reader.
        /// </summary>
        /// <returns>The vector4.</returns>
        /// <param name="reader">Reader.</param>
        protected static Quaternion ReadQuaternion(BinaryReader reader)
        {
            return new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        /// <summary>
        /// Reads a 3-component vector (Vector3) array from the given BinaryReader reader.
        /// </summary>
        /// <returns>The vector3 array.</returns>
        /// <param name="reader">Reader.</param>
        /// <param name="count">The count of elements to load.</param>
        protected static Vector3[] ReadVector3Array(BinaryReader reader, int count)
        {
            Vector3[] returnArray = new Vector3[count];

            // Load
            for (int i = 0; i < count; i++)
                returnArray[i] = ReadVector3(reader);

            return returnArray;
        }

        /// <summary>
        /// Reads a 4-component vector (Vector4) array from the given BinaryReader reader.
        /// </summary>
        /// <returns>The vector4 array.</returns>
        /// <param name="reader">Reader.</param>
        /// <param name="count">The count of elements to load.</param>
        protected static Vector4[] ReadVector4Array(BinaryReader reader, int count)
        {
            Vector4[] returnArray = new Vector4[count];

            // Load
            for (int i = 0; i < count; i++)
                returnArray[i] = ReadVector4(reader);

            return returnArray;
        }

        /// <summary>
        /// Reads a 4-component vector (Quaternion) array from the given BinaryReader reader.
        /// </summary>
        /// <returns>The vector4 array.</returns>
        /// <param name="reader">Reader.</param>
        /// <param name="count">The count of elements to load.</param>
        protected static Quaternion[] ReadQuaternionArray(BinaryReader reader, int count)
        {
            Quaternion[] returnArray = new Quaternion[count];

            // Load
            for (int i = 0; i < count; i++)
                returnArray[i] = ReadQuaternion(reader);

            return returnArray;
        }

        /// <summary>
        /// Reads an integer array from BinaryReader reader.
        /// </summary>
        /// <returns>The integer array.</returns>
        /// <param name="reader">Reader.</param>
        /// <param name="count">The count of elements to load.</param>
        protected static int[] ReadIntegerArray(BinaryReader reader, int count)
        {
            int[] returnArray = new int[count];

            // Load
            for (int i = 0; i < count; i++)
            {
                int value = reader.ReadInt32();

                returnArray[i] = value;
            }

            return returnArray;
        }

        /// <summary>
        /// Reads an float array from BinaryReader reader.
        /// </summary>
        /// <returns>The integer array.</returns>
        /// <param name="reader">Reader.</param>
        /// <param name="count">The count of elements to load.</param>
        protected static float[] ReadFloatArray(BinaryReader reader, int count)
        {
            float[] returnArray = new float[count];

            // Load
            for (int i = 0; i < count; i++)
                returnArray[i] = reader.ReadSingle();

            return returnArray;
        }

        #endregion
    }
}
