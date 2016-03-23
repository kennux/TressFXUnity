using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TressFXLib.Numerics;
using System.Globalization;

namespace TressFXLib.Formats
{
    /// <summary>
    /// Wavefront OBJ importer
    /// 
    /// Important Note:
    /// The Wavefront obj File Format does not specify hairsimulation parameters,
    /// so after importing the ase data the simulation parameters must get prepared manually in order to do a correct import!
    /// 
    /// TODO: Re-implement! (ATM just Quick'n'Dirty code)
    /// </summary>
    public class WavefrontObjFormat : IHairFormat
    {
        private struct Line
        {
            public int index1;
            public int index2;

            public Line(int index1, int index2)
            {
                this.index1 = index1;
                this.index2 = index2;
            }
        }

        public void Export(BinaryWriter writer, string path, Hair hair)
        {
            throw new NotImplementedException("Hair cannot get exported as Wavefront obj file");
        }

        public HairMesh[] Import(BinaryReader reader, string path, Hair hair, HairImportSettings importSettings)
        {
            reader.Close();
            // Initialize the importer
            string[] objContent = File.ReadAllLines(path);
            List<Vector3> vertices = new List<Vector3>();
            List<Line> indices = new List<Line>();
            List<HairStrand> currentStrands = new List<HairStrand>();
            
            // Parse all vertices and indices
            for (int i = 0; i < objContent.Length; i++)
            {
                // Get all parts
                List<string> partsList = new List<string>(objContent[i].Split(' '));
                partsList.RemoveAll(x => string.IsNullOrEmpty(x.Replace(" ", "")));
                string[] parts = partsList.ToArray();

                switch (parts[0])
                {
                    case "v":
                        {
                            vertices.Add(Vector3.Multiply(new Vector3(SafeParse(parts[1]), SafeParse(parts[2]), SafeParse(parts[3])), importSettings.scale));
                        }
                        break;
                    case "l":
                        {
                            indices.Add(new Line(int.Parse(parts[1])-1, int.Parse(parts[2])-1));
                        }
                        break;
                }
            }

            // Parse all strands
            List<int> alreadyLoadedIndices = new List<int>(640000);
            List<HairStrandVertex> currentStrandVertices = new List<HairStrandVertex>(64);
            for (int i = 0; i < indices.Count; i++)
            {
                // In order to detect when a new hair starts, we detect if the first index in a line was not added to the mesh already.
                // If it was NOT, we are in a new strand
                if (i != 0 && !alreadyLoadedIndices.Contains(indices[i].index1))
                {
                    // We are in a new hair strand
                    HairStrand hs = new HairStrand();
                    currentStrandVertices[0].isMovable = false;
                    currentStrandVertices.ForEach((sv) =>
                    {
                        hs.vertices.Add(sv);
                    });

                    hs.isGuidanceStrand = true;
                    currentStrands.Add(hs);

                    // Cleanup
                    currentStrandVertices.Clear();
                }

                // Add
                if (!alreadyLoadedIndices.Contains(indices[i].index1))
                {
                    currentStrandVertices.Add(new HairStrandVertex(vertices[indices[i].index1], Vector3.Zero, Vector4.Zero));
                    alreadyLoadedIndices.Add(indices[i].index1);
                }
                if (!alreadyLoadedIndices.Contains(indices[i].index2))
                {
                    currentStrandVertices.Add(new HairStrandVertex(vertices[indices[i].index2], Vector3.Zero, Vector4.Zero));
                    alreadyLoadedIndices.Add(indices[i].index2);
                }
            }

            // Shuffle strands
            currentStrands = FormatHelper.Shuffle(currentStrands);

            HairMesh hm = new HairMesh();
            currentStrands.ForEach((item) =>
            {
                hm.strands.Add(item);
            });

            return new HairMesh[] { hm };
        }

        #region Helpers

        /// <summary>
        /// Safe float parsing that accepts . and ,.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static float SafeParse(string input)
        {
            if (String.IsNullOrEmpty(input)) { throw new ArgumentNullException("input"); }

            float res;
            if (Single.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out res))
            {
                return res;
            }

            return 0.0f; // Or perhaps throw your own exception type
        }

        #endregion
    }
}