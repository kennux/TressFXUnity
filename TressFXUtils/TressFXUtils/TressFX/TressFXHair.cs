using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TressFXUtils.Util;
using System.Numerics;

namespace TressFXUtils.TressFX
{
    /// <summary>
    /// TressFX Hair implementation.
    /// It can load and save modified tressfx hair files.
    /// </summary>
    public class TressFXHair
    {
        public List<TressFXStrand> strands;
        public int vertexCount;

        public string filename;

        // Vertex per strand counts
        public int lowestVertexPerStrandCount = 0;
        public int highestVertexPerStrandCount = 0;

        // Tressfx header information
        public float scale;
        public Vector3 rotation;
        public Vector3 translation;
        public bool bothEndsImmovable;
        public int numFollowHairsPerGuideHair;
        public float maxRadiusAroundGuideHair;

        public TressFXHair()
        {
            this.strands = new List<TressFXStrand>();
        }

        public void LoadTressFXFile(string file)
        {
            ConsoleUtil.LogToConsole("Loading tressfx mesh file " + file, ConsoleColor.Yellow);

            this.filename = file;
            string[] lines = File.ReadAllLines(file);
            List<TressFXStrand> strands = new List<TressFXStrand>();

            // Parse file
            for (int i = 0; i < lines.Length; i++)
            {
                string[] tokens = lines[i].Split(' ');

                switch (tokens[0])
                {
                    // Header information
                    case "scale":
                        this.scale = ParseUtils.ParseFloat(tokens[1]);
                        break;
                    case "rotation":
                        this.rotation = new Vector3(ParseUtils.ParseFloat(tokens[1]), ParseUtils.ParseFloat(tokens[2]), ParseUtils.ParseFloat(tokens[3]));
                        break;
                    case "translation":
                        this.translation = new Vector3(ParseUtils.ParseFloat(tokens[1]),ParseUtils.ParseFloat(tokens[2]),ParseUtils.ParseFloat(tokens[3]));
                        break;
                    case "bothEndsImmovable":
                        this.bothEndsImmovable = (int.Parse(tokens[1]) == 1);
                        break;
                    case "numFollowHairsPerGuideHair":
                        this.numFollowHairsPerGuideHair = int.Parse(tokens[1]);
                        break;
                    case "maxRadiusAroundGuideHair":
                        this.maxRadiusAroundGuideHair = ParseUtils.ParseFloat(tokens[1]);
                        break;
                    
                    // Strand information
                    case "strand":
                        int strandIndex = int.Parse(tokens[1]);
                        int vertices = int.Parse(tokens[3]);
                        float texcoordX = float.Parse(tokens[5]);

                        TressFXStrand strand = new TressFXStrand();
                        strand.vertices = new Vector3[vertices];

                        for (int j = 0; j < vertices; j++)
                        {
                            i++;

                            string[] vertexTokens = lines[i].Split(' ');

                            float x = 0;
                            float y = 0;
                            float z = 0;

                            // Replace points with comma's
                            for (int k = 0; k < vertexTokens.Length; k++)
                                vertexTokens[k] = vertexTokens[k].Replace('.', ',');

                            if (float.TryParse(vertexTokens[0], out x) && float.TryParse(vertexTokens[1], out y) && float.TryParse(vertexTokens[2], out z))
                            {
                                this.vertexCount++;
                                strand.vertices[j] = new Vector3(x, y, z);
                            }
                            else
                            {
                                strand = null;
                                break;
                            }
                        }

                        if (strand != null)
                            this.AddStrand(strand);
                        break;
                }
            }

            ConsoleUtil.LogToConsole("Loaded tressfx file! Strands: " + this.strands.Count + ", Vertices: " + this.vertexCount, ConsoleColor.Green);
            ConsoleUtil.LogToConsole("Highest vertex count per strand: " + this.highestVertexPerStrandCount + ", Lowest count: " + this.lowestVertexPerStrandCount, ConsoleColor.Green);
            ConsoleUtil.LogToConsole("Header Information: ", ConsoleColor.Blue);
            ConsoleUtil.LogToConsole("Scale: " + this.scale, ConsoleColor.Blue);
            ConsoleUtil.LogToConsole("Rotation: " + this.rotation, ConsoleColor.Blue);
            ConsoleUtil.LogToConsole("Translation: " + this.translation, ConsoleColor.Blue);
            ConsoleUtil.LogToConsole("Both ends immovable: " + this.bothEndsImmovable, ConsoleColor.Blue);
            ConsoleUtil.LogToConsole("Num follow hairs per guide hair: " + this.numFollowHairsPerGuideHair, ConsoleColor.Blue);
            ConsoleUtil.LogToConsole("Max radius around guide hair: " + this.maxRadiusAroundGuideHair, ConsoleColor.Blue);
        }

        /// <summary>
        /// Adds the given strand to this tressfx hair mesh.
        /// </summary>
        /// <param name="strand"></param>
        public void AddStrand(TressFXStrand strand)
        {
            // Set lowest / highest vertex counts
            if (this.lowestVertexPerStrandCount == 0 || this.lowestVertexPerStrandCount > strand.vertices.Length)
            {
                this.lowestVertexPerStrandCount = strand.vertices.Length;
            }
            else if (this.highestVertexPerStrandCount == 0 || this.highestVertexPerStrandCount < strand.vertices.Length)
            {
                this.highestVertexPerStrandCount = strand.vertices.Length;
            }

            vertexCount += strand.vertices.Length;

            this.strands.Add(strand);
        }

        /// <summary>
        /// Adds the given strand array to this hair mesh.
        /// </summary>
        /// <param name="strands"></param>
        public void AddStrands(TressFXStrand[] strands)
        {
            foreach (TressFXStrand strand in strands)
            {
                this.AddStrand(strand);
            }
        }

        /// <summary>
        /// Saves the hair file.
        /// </summary>
        /// <param name="file"></param>
        public void SaveHair(string file)
        {
            // Write file
            StringBuilder fileContent = new StringBuilder();

            this.filename = file;

            // Write header
            fileContent.Append("version 2.0" + "\r\n");
            fileContent.Append("scale "+ this.scale.ToString().Replace(',', '.') + "\r\n");
            fileContent.Append("rotation " + this.rotation.X + " " + this.rotation.Y + " " + this.rotation.Z + "\r\n");
            fileContent.Append("translation " + this.translation.X + " " + this.translation.Y + " " + this.translation.Z + "\r\n");
            fileContent.Append("bothEndsImmovable " + (this.bothEndsImmovable ? 1 : 0) + "\r\n");
            fileContent.Append("maxNumVerticesInStrand " + this.highestVertexPerStrandCount + "\r\n");
            fileContent.Append("numFollowHairsPerGuideHair " + this.numFollowHairsPerGuideHair + "\r\n");
            fileContent.Append("maxRadiusAroundGuideHair " + this.maxRadiusAroundGuideHair.ToString().Replace(',', '.') + "\r\n");
            fileContent.Append("numStrands " + this.strands.Count + "\r\nis sorted 1\r\n");

            // Write strands
            int i = 0;
            foreach (TressFXStrand strand in this.strands)
            {
                fileContent.Append("strand " + i + " numVerts " + strand.vertices.Length + " texcoord " + strand.texcoordX + " 000000\r\n");
                for (int j = 0; j < this.strands[i].vertices.Length; j++)
                {
                    fileContent.Append(strand.vertices[j].X + " " + strand.vertices[j].Y + " " + strand.vertices[j].Z + "\r\n");
                }

                i++;
            }

            File.WriteAllText(file, fileContent.ToString().Replace(",", "."));
        }

        /// <summary>
        /// Recalculates the UV-Coordinates for the hairs.
        /// </summary>
        public void RecalculateUVs()
        {
            float texcoordMultiplier = 1.0f / (float)this.strands.Count;

            int i = 0;
            foreach (TressFXStrand strand in this.strands)
            {
                this.strands[i].texcoordX = texcoordMultiplier * i;

                i++;
            }
        }

        /// <summary>
        /// Adds tressfx hair to this instance. used for merging!
        /// </summary>
        /// <param name="h"></param>
        public void AddTressFXHair(TressFXHair h)
        {
            List<TressFXStrand> strands = new List<TressFXStrand>(this.strands);
            strands.AddRange(h.strands);

            // Recalculate vertex count and lowestVertexPerStrandCount and highestVertexPerStrandCount
            int vertices = 0;
            int lowestVertexCount = 0;
            int highestVertexCount = 0;
            foreach (TressFXStrand s in strands)
            {

                vertices += s.vertices.Length;

                if (lowestVertexCount == 0 || lowestVertexCount > s.vertices.Length)
                {
                    lowestVertexCount = s.vertices.Length;
                }

                if (highestVertexCount == 0 || highestVertexCount < s.vertices.Length)
                {
                    highestVertexCount = s.vertices.Length;
                }
            }

            // Set values
            this.strands = strands;
            this.vertexCount = vertices;
            this.lowestVertexPerStrandCount = lowestVertexCount;
            this.highestVertexPerStrandCount = highestVertexCount;
        }

        /// <summary>
        /// This function converts the current tressfx hair object to a binary tressfx asset for use in unity.
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateBinaryTressFXAsset()
        {
            // Generate follow hairs
            List<TressFXStrand> followHairs = new List<TressFXStrand>();


            return null;
        }

        /// <summary>
        /// Uniforms the hair vertices so every strand has the same vertex count.
        /// 
        /// IMPORTANT: This function does not care about any hair shapes and interpolates positions between the vertices!
        /// TODO
        /// </summary>
        public void UniformHair()
        {

        }
    }
}
