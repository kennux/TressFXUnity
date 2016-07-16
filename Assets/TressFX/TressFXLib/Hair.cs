using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TressFXLib.Formats;
using TressFXLib.Numerics;
using System.IO;

namespace TressFXLib
{
    /// <summary>
    /// This is the main class for handling tressfx hair info.
    /// </summary>
    public class Hair
    {
        /// <summary>
        /// Loads / imports hair from the given file with given format.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="path"></param>
        public static Hair Import(HairFormat format, string path, HairImportSettings importSettings = null)
        {
            if (importSettings == null)
                importSettings = HairImportSettings.standard;

            // Get hair format impl
            IHairFormat formatImpl = format.GetFormatImplementation();

            // Create new hair object
            Hair hair = new Hair();

            // Open the binary reader
            BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open));

            // Import the hair data
            HairMesh[] hairMeshes = null;
            try
            {
                hairMeshes = formatImpl.Import(reader, path, hair, importSettings);
            }
            finally
            {
                reader.Close();
            }
            reader.Close();

            // Validity check
            if (hairMeshes.Length > 4)
                throw new IndexOutOfRangeException("TressFX only supports up to 4 hair meshes, the file you tried to import had " + hairMeshes.Length);

            // Set all meshes
            for (int i = 0; i < hairMeshes.Length; i++)
                hair.SetHairMesh(i, hairMeshes[i]);

            hair.CreateBoundingSphere();

            // We're done :>
            return hair;
        }

        /// <summary>
        /// The hair bounding sphere.
        /// Can get calculated automatically
        /// </summary>
        public HairBoundingSphere boundingSphere
        {
            get
            {
                if (this._boundingSphere == null)
                    this.CreateBoundingSphere();
                return this._boundingSphere;
            }
            private set
            {
                this._boundingSphere = value;
            }
        }
        private HairBoundingSphere _boundingSphere;

        /// <summary>
        /// Contains all hair meshes for this hair.
        /// Hair will consist of 1-4 hair meshes.
        /// If an index in this array is null it means there is no hair mesh in this index.
        /// </summary>
        public HairMesh[] meshes
        {
            get
            {
                return this._meshes;
            }
        }
        private HairMesh[] _meshes = new HairMesh[4];

        /// <summary>
        /// Returns the vertex count for this mesh.
        /// This is a very expensive call, as it iterates through all meshes and the meshes are iterating thorugh all strands!
        /// </summary>
        public int vertexCount
        {
            get
            {
                int count = 0;

                foreach (HairMesh mesh in this.meshes)
                    if (mesh != null)
                        count += mesh.vertexCount;

                return count;
            }
        }

        /// <summary>
        /// Gets the count of strands in this hair (so in all it's meshes).
        /// </summary>
        public int strandCount
        {
            get
            {
                int count = 0;

                foreach (HairMesh mesh in this.meshes)
                    if (mesh != null)
                        count += mesh.strandCount;

                return count;
            }
        }

        /// <summary>
        /// Gets the highest vertex count of all strands.
        /// This is a very expensive call, as it iterates through all meshes and the meshes are iterating thorugh all strands!
        /// </summary>
        public int maxNumVerticesPerStrand
        {
            get
            {
                int highestCount = 0;

                foreach (HairMesh mesh in this.meshes)
                {
                    if (mesh == null)
                        continue;

                    int count = mesh.maxNumVerticesPerStrand;

                    if (highestCount < count)
                        highestCount = count;
                }

                return highestCount;
            }
        }

        /// <summary>
        /// The hair simulation data.
        /// If this is null it will get created (which is very expensive!)
        /// </summary>
        public HairSimulationData hairSimulationData
        {
            get
            {
                if (this._hairSimulationData == null)
                    this._hairSimulationData = this.CreateHairSimulationData();

                return this._hairSimulationData;
            }
        }
        private HairSimulationData _hairSimulationData;

        /// <summary>
        /// The line indices.
        /// If they arent set, they will get calculated.
        /// </summary>
        public int[] lineIndices
        {
            get
            {
                if (this._lineIndices == null || this._lineIndices.Length == 0)
                {
                    this.CreateIndices();
                }
                return this._lineIndices;
            }
            private set { this._lineIndices = value; }
        }
        private int[] _lineIndices;

        /// <summary>
        /// Gets the texcoords for this hair with a global index.
        /// Used to export texturecoordinates for a gpu buffer.
        /// Very expensive call, texcoords will get collected every call!
        /// </summary>
        public Vector4[] texcoords
        {
            get
            {
                Vector4[] texcoordArray = new Vector4[this.vertexCount];
                int i = 0;

                foreach (HairMesh mesh in this.meshes)
                    if (mesh != null)
                        foreach (HairStrand strand in mesh.strands)
                            foreach (HairStrandVertex vertex in strand.vertices)
                            {
                                texcoordArray[i] = vertex.texcoord;
                                i++;
                            }

                return texcoordArray;
            }
        }

        /// <summary>
        /// The triangle indices.
        /// If they arent set, they will get calculated.
        /// </summary>
        public int[] triangleIndices
        {
            get
            {
                if (this._triangleIndices == null || this._triangleIndices.Length == 0)
                {
                    this.CreateIndices();
                }
                return this._triangleIndices;
            }
            private set { this._triangleIndices = value; }
        }
        private int[] _triangleIndices;

        /// <summary>
        /// This function is used to make sure that every hair strand has the same vertex position.
        /// It uses linear interpolation in order to do so.
        /// After calling this, the simulation data must get reprepared, because the hair vertices will get reset.
        /// </summary>
        /// <param name="normalizedVertexCount"></param>
        public void NormalizeStrands(int normalizedVertexCount)
        {
            Utilities.StrandLevelIteration(this, (strand) =>
                {
                    List<HairStrandVertex> newVertices = new List<HairStrandVertex>();

                    // Get the strand and wanted segment length
                    float strandLength = strand.length;
                    float segmentLength = strandLength / (normalizedVertexCount-1);
                    float distCounter = 0;

                    for (int i = 0; i < normalizedVertexCount; i++)
                    {
                        HairStrandVertex v = new HairStrandVertex(Utilities.GetPositionOnStrand(strand, distCounter), Vector3.Zero, Vector4.Zero);
                        newVertices.Add(v);
                        distCounter += segmentLength;
                    }

                    strand.vertices.Clear();
                    strand.vertices.AddRange(newVertices);
                });
        }

        #region Explicit initializations

        /// <summary>
        /// Explicitly initialized the indices of this hair.
        /// This function is only used if the indices are imported with the hair data, for example in tfxb.
        /// Only initialize them manually if you know what you are doing.
        /// They will get generated automatically in the getter functions.
        /// </summary>
        /// <param name="lineIndices"></param>
        /// <param name="triangleIndices"></param>
        public void InitializeIndices(int[] lineIndices, int[] triangleIndices)
        {
            this._lineIndices = lineIndices;
            this._triangleIndices = triangleIndices;
        }

        /// <summary>
        /// Explicitly initializes the bounding sphere.
        /// Only used if the bounding sphere is loaded on import, for example in tfxb.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        public void InitializeBoundingSphere(Vector3 center, float radius)
        {
            this.boundingSphere = new HairBoundingSphere(center, radius);
        }

        /// <summary>
        /// Explicitly initializes the simulation data.
        /// This is used in for example the tressfx binary importer.
        /// </summary>
        /// <param name="simulationData"></param>
        public void InitializeSimulationData(HairSimulationData hairSimulationData)
        {
            this._hairSimulationData = hairSimulationData;
        }

        #endregion

        /// <summary>
        /// Exports this hair to the given file format output.
        /// </summary>
        public void Export(HairFormat format, string path)
        {
            IHairFormat formatImpl = format.GetFormatImplementation();
            this.Export(formatImpl, path);
        }

        /// <summary>
        /// Exports this hair to the given file format class instance.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="path"></param>
        public void Export(IHairFormat format, string path)
        {
            if (File.Exists(path))
                File.Delete(path);

            BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.CreateNew));

            try
            {
                format.Export(writer, path, this);
            }
            finally
            {
                writer.Close();
            }
        }

        /// <summary>
        /// Sets the given hairmesh into the given index.
        /// NOTE: TressFX only supports indices 0-3 (so max. 4 meshes).
        /// </summary>
        /// <param name="index"></param>
        /// <param name="mesh"></param>
        public void SetHairMesh(int index, HairMesh mesh)
        {
            if (index >= 0 && index <= 3)
                this.meshes[index] = mesh;
            else
                throw new IndexOutOfRangeException("TressFX only supports mesh indices between including 0 and 3");
        }

        /// <summary>
        /// Returns true if the hair strands are all having a vertex count of 16.
        /// 
        /// Developer Note:
        /// This will may get extended later on in order to do more advanced checks if simulation will run correctly.
        /// </summary>
        /// <returns></returns>
        public bool SimulationPrecheck()
        {
            // TODO: Actual check
            return true;
        }

        /// <summary>
        /// Performs follow hair generation.
        /// If a hair file gets imported from a "not-tressfx" format like ase every hair will be set to be a guidance hair.
        /// With this functions its possible to generate hairs which will not get simulated, but moved along their guidance hair.
        /// This can get used to make hair more dense without simulation overhead.
        /// </summary>
        /// <param name="followHairsPerGuidanceHair"></param>
        /// <param name="maxRadiusAroundGuideHair"></param>
        public void GenerateFollowHairs(int followHairsPerGuidanceHair, float maxRadiusAroundGuideHair)
        {
            // Generate follow hairs
            if (followHairsPerGuidanceHair <= 0)
                return;

            foreach (HairMesh mesh in this.meshes)
            {
                if (mesh != null)                                                   // Check if mesh is not null to prevent nullpointer exceptions
                {
                    List<HairStrand> resultStrandList = new List<HairStrand>();
                    foreach (HairStrand guideStrand in mesh.strands)                     // Get every strand in every mesh
                    {
                        // Add the current strand
                        resultStrandList.Add(guideStrand);

                        if (guideStrand.isGuidanceStrand)                                // Check if the current strand is a guidance strand and needs follow hairs
                            for (int i = 0; i < followHairsPerGuidanceHair; i++)    // Generate the strands.
                            {
                                // Init the new strand
                                HairStrand newStrand = new HairStrand();
                                newStrand.isGuidanceStrand = false;
                                newStrand.guidanceStrand = guideStrand;

                                Vector3 v01 = (guideStrand.vertices[1].position - guideStrand.vertices[0].position);
                                v01.Normalize();

                                // Find two orthogonal unit tangent vectors to v01
                                Vector3 t0, t1;
                                Utilities.GetTangentVectors(v01, out t0, out t1);
                                Vector3 offset = Utilities.GetRandom(-maxRadiusAroundGuideHair, maxRadiusAroundGuideHair) * t0 + Utilities.GetRandom(-maxRadiusAroundGuideHair, maxRadiusAroundGuideHair) * t1;

                                // Generate the vertices
                                for (int k = 0; k < guideStrand.vertices.Count; k++)
                                {
                                    float factor = 5.0f * (float)k / (float)guideStrand.vertices.Count + 1.0f;
                                    Vector3 position = guideStrand.vertices[k].position + (offset * factor);
                                    HairStrandVertex vertex = new HairStrandVertex(position, Vector3.Zero, guideStrand.vertices[k].texcoord);
                                    vertex.isMovable = guideStrand.vertices[k].isMovable;
                                    newStrand.vertices.Add(vertex);
                                }

                                resultStrandList.Add(newStrand);
                            }
                    }

                    mesh.strands.Clear();
                    mesh.strands.AddRange(resultStrandList);
                }
            }
        }

        /// <summary>
        /// TEMPORARY FUNCTION!
        /// THIS WILL GET REMOVED IN THE NEAR FUTURE!
        /// 
        /// Prepares the simulation parameters by using a binary executable of the assetconverter from amd.
        /// Will return a new hair instance!
        /// </summary>
        /// <param name="followHairs"></param>
        /// <param name="maxRadiusAround"></param>
        /// <param name="assetConverterExePath"></param>
        /// <returns></returns>
        public Hair PrepareSimulationParamatersAssetConverter(int followHairs, float maxRadiusAround, string assetConverterExePath)
        {
            // Get temporary folder
            string tempPath = Path.GetTempPath() + "/";
            List<string> meshPaths = new List<string>();
            List<string> meshNames = new List<string>();

            // Export all meshes
            int meshIndex = 0;
            foreach (HairMesh m in this.meshes)
            {
                if (m == null)
                    continue;

                // Create path
                String path = tempPath + "mesh_" + meshIndex;

                if (File.Exists(path))
                    File.Delete(path);
                meshPaths.Add(path);
                meshNames.Add("mesh_" + meshIndex);

                // Export
                m.ExportAsTFX(path, followHairs, maxRadiusAround);

                meshIndex++;
            }

            String assetConverterTempPath = tempPath + "assetconverter.exe";
            String outputPath = tempPath + "output.tfxb";

            // Copy the asset converter
            if (File.Exists(assetConverterTempPath))
                File.Delete(assetConverterTempPath);
            if (File.Exists(outputPath))
                File.Delete(outputPath);

            File.Copy(assetConverterExePath, assetConverterTempPath);

            // Build commandline
            String arguments = string.Join(" ", meshNames.ToArray());
            arguments += " output.tfxb";

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                WorkingDirectory = Path.GetTempPath(),
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal,
                FileName = assetConverterTempPath,
                RedirectStandardInput = true,
                UseShellExecute = false,
                Arguments = arguments
            };
            var p = new System.Diagnostics.Process();
            p.StartInfo = startInfo;
            p.Start();

            p.WaitForExit();

            // Read back the tfxb file
            return Hair.Import(HairFormat.TFXB, outputPath);
        }

        /// <summary>
        /// This function will get used by importers for example if there were only strand vertex positions available.
        /// It will calculate everything inside of the HairStrand and HairStrandVertex based off the vertex positions.
        /// It also calculates the bounding sphere parameters.
        /// </summary>
        public void PrepareSimulationParameters()
        {
            this.BranchingAvoidance();
            int hairIndexCounter = 0;

            // Forward this call to all meshes
            foreach (HairMesh m in this.meshes)
            {
                if (m != null)
                    m.PrepareSimulationParameters(ref hairIndexCounter);
            }

            this._hairSimulationData = null;
        }

        /// <summary>
        /// This is called in PrepareSimulationParameters().
        /// It may deletes some hairs in order to avoid branching in the compute shader.
        /// </summary>
        private void BranchingAvoidance(uint threadGroupSize = 64)
        {
            // Prepare
            uint loadedNumStrands = (uint)this.strandCount;
            uint deleteStrands = loadedNumStrands % threadGroupSize;

            // Delete last hairs
            // Get last mesh
            HairMesh lastMesh = null;
            foreach (HairMesh m in this.meshes)
                if (m != null)
                    lastMesh = m;

            for (uint i = deleteStrands; i > 0; i--)
            {
                // Remove last hair
                lastMesh.strands.RemoveAt(lastMesh.strands.Count - 1);
            }
        }

        /// <summary>
        /// Creates a new HairSimulationData object and fills it with data from this hair's hair meshes.
        /// It can get used to "easy-export" tressfx-ready data-arrays from hair objects.
        /// </summary>
        /// <returns></returns>
        private HairSimulationData CreateHairSimulationData()
        {
            // Create new hair simulation data object
            HairSimulationData data = new HairSimulationData();

            // Counter variables
            int strandTypeCounter = 0; // Maps to data.strandType. Will get incremented after every mesh iteration.
            int guideHairStrandCount = 0;
            int guideHairVertexCount = 0;
            int maxNumVerticesPerStrand = 0;

            // Start iterating through all meshes.
            foreach (HairMesh mesh in this.meshes)
            {
                if (mesh == null)
                    continue;

                // Move through every strand
                foreach (HairStrand strand in mesh.strands)
                {
                    // Set strand-level information
                    data.strandTypes.Add(strandTypeCounter);
                    data.followRootOffsets.Add(strand.followRootOffset);

                    // Get guidance hair flag
                    bool isGuide = strand.isGuidanceStrand;
                    if (isGuide)
                        guideHairStrandCount++;

                    // Vertex counter
                    int vertexCount = 0;

                    // Iterate through every vertex
                    foreach (HairStrandVertex vertex in strand.vertices)
                    {
                        // Set vertex-level information
                        data.vertices.Add(vertex.GetTressFXPosition());
                        data.tangents.Add(new Vector4(vertex.tangent, 0));
                        data.referenceVectors.Add(vertex.referenceVector);
                        data.globalRotations.Add(vertex.globalRotation);
                        data.localRotations.Add(vertex.localRotation);
                        data.thicknessCoefficients.Add(vertex.thicknessCoefficient);
                        data.restLength.Add(vertex.restLength);

                        if (isGuide)
                            guideHairVertexCount++;
                        vertexCount++;
                    }

                    // Check if we got a new highscore!
                    if (maxNumVerticesPerStrand < vertexCount)
                        maxNumVerticesPerStrand = vertexCount;
                }

                // Counter handling
                strandTypeCounter++;
            }

            // Set the counter variables
            data.guideHairStrandCount = guideHairStrandCount;
            data.guideHairVertexCount = guideHairVertexCount;
            data.maxNumVerticesPerStrand = maxNumVerticesPerStrand;

            return data;
        }

        /// <summary>
        /// Implicitly initializes the indices of this hair.
        /// This calculates the indices automatically
        /// </summary>
        /// <param name="lineIndices"></param>
        /// <param name="triangleIndices"></param>
        public void CreateIndices()
        {
            List<int> lineIndices = new List<int>();
            List<int> triangleIndices = new List<int>();

            int id = 0;

            // Iterate through every mesh and every strand and every vertex and generate the indices for them.
            for (int i = 0; i < this.meshes.Length; i++)
            {
                HairMesh currentMesh = this.meshes[i];

                if (currentMesh == null)
                    continue;

                for (int j = 0; j < currentMesh.strandCount; j++)
                {
                    HairStrand currentStrand = currentMesh.strands[j];

                    for (int k = 0; k < currentStrand.vertices.Count - 1; k++)
                    {
                        // Append line indices
                        lineIndices.Add(id);
                        lineIndices.Add(id+1);

                        // Append triangle indices
                        triangleIndices.Add(2 * id);
                        triangleIndices.Add(2 * id + 1);
                        triangleIndices.Add(2 * id + 2);
                        triangleIndices.Add(2 * id + 2);
                        triangleIndices.Add(2 * id + 1);
                        triangleIndices.Add(2 * id + 3);

                        id++;
                    }

                    id++;
                }
            }

            // Set indices
            this.lineIndices = lineIndices.ToArray();
            this.triangleIndices = triangleIndices.ToArray();
        }

        /// <summary>
        /// Implicitly initializes the bounding sphere.
        /// Calculates the bounding sphere automatically
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        public void CreateBoundingSphere()
        {
            this.boundingSphere = new HairBoundingSphere(new Vector3(0, 0, 0), 0);

            // Get lowest xyz and highest xyz coordinates
            Vector3 lowest = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 highest = Vector3.Zero;

            // Iterate through every vertex
            foreach (HairMesh m in this.meshes)
            {
                if (m != null)
                    foreach (HairStrand s in m.strands)
                        foreach (HairStrandVertex v in s.vertices)
                        {
                            // Lowest
                            lowest.x = (lowest.x > v.position.x) ? v.position.x : lowest.x;
                            lowest.y = (lowest.y > v.position.y) ? v.position.y : lowest.y;
                            lowest.z = (lowest.z > v.position.z) ? v.position.z : lowest.z;

                            // Highest
                            highest.x = (highest.x < v.position.x) ? v.position.x : highest.x;
                            highest.y = (highest.y < v.position.y) ? v.position.y : highest.y;
                            highest.z = (highest.z < v.position.z) ? v.position.z : highest.z;
                        }
            }

            BoundingBox boundingBox = new BoundingBox(lowest, highest);
            this.boundingSphere.center = boundingBox.Center;
            this.boundingSphere.radius = boundingBox.IsInside(boundingBox.Center) ? (boundingBox.Center - highest).Length : 0f;
        }

        /// <summary>
        /// UV-Maps the hair.
        /// The uv-coordinates will look like this: { STRAND_INDEX / STRAND_COUNT, VERTEX_INDEX / VERTEX_COUNT_IN_STRAND, 0, 0 }
        /// </summary>
        public void CreateUVs()
        {
            int hairIndex = 0;
            float strandCount = this.strandCount;

            foreach (HairMesh mesh in this.meshes)
                if (mesh != null)
                    foreach (HairStrand strand in mesh.strands)
                    {
                        int vertexIndex = 0;
                        foreach (HairStrandVertex vertex in strand.vertices)
                        {
                            // { STRAND_INDEX / STRAND_COUNT, VERTEX_INDEX / VERTEX_COUNT_IN_STRAND, 0, 0 }
                            vertex.texcoord = new Vector4(hairIndex / strandCount, vertexIndex / (float)strand.vertices.Count, 0, 0);
                            vertexIndex++;
                        }

                        hairIndex++;
                    }
        }
    }
}
