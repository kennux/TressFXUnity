using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using TressFXUtils.Util;
using System.Numerics;

namespace TressFXUtils.TressFX
{
    public class AseConverter
    {
        /// <summary>
        /// Converts a given ase file to a dictionary of tressfx hairs.
        /// Key of the dictionary contains the mesh name, value is the tressfxhair data.
        /// </summary>
        /// <param name="asefile"></param>
        /// <param name="hairFilePrefix"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static Dictionary<string, TressFXHair> ConvertAse(string asefile)
        {
            // Start parsing the file
            ConsoleUtil.LogToConsole("Loading ASE File...", ConsoleColor.Blue);
            string[] aseContent = File.ReadAllLines(asefile);
            Dictionary<string, TressFXHair> hairMeshes = new Dictionary<string, TressFXHair>();
            List<TressFXStrand> currentStrands = new List<TressFXStrand>();
            string aseFilenameWithoutExt = Path.GetFileNameWithoutExtension(asefile);

            int currentStrand = 0;
            int currentHairId = -1;
            float texcoordMultiplier = 0;

            ConsoleUtil.LogToConsole("Starting ASE parsing... This may take a LONG while..", ConsoleColor.Blue);

            // Now the hard part begins...
            for (int i = 0; i < aseContent.Length; i++)
            {
                string[] tokens = aseContent[i].Split('\t');

                if (aseContent[i].Contains("*SHAPE_LINECOUNT"))
                {
                    tokens = tokens[1].Split(' ');
                }
                else if (aseContent[i].Contains("SHAPE_LINE"))
                {
                    tokens = tokens[1].Split(' ');
                }

                if (tokens.Length >= 2)
                {
                    if (tokens[0] == "*SHAPE_LINECOUNT")
                    {
                        if (currentStrand > 0)
                        {
                            currentHairId++;
                            currentStrand = 0;

                            // Add to mesh list
                            TressFXHair hairMesh = new TressFXHair();
                            foreach (TressFXStrand strand in currentStrands)
                            {
                                hairMesh.AddStrand(strand);
                            }

                            hairMeshes.Add(aseFilenameWithoutExt + "_" + currentHairId, hairMesh);

                            // Clear current strands
                            currentStrands.Clear();

                            texcoordMultiplier = 1.0f / (float)int.Parse(tokens[1]);
                            ConsoleUtil.LogToConsole("Starting parse hair: " + currentHairId + ", lines count: " + int.Parse(tokens[1]), ConsoleColor.Yellow);
                        }
                    }
                    else if (tokens[0] == "*SHAPE_LINE")
                    {
                        // Parse the current line
                        Vector3[] positions = null;

                        string[] vertexCountTokens = aseContent[i + 1].Split(' ');
                        positions = new Vector3[int.Parse(vertexCountTokens[1])];

                        // Parse vertices
                        for (int j = 0; j < positions.Length; j++)
                        {
                            string[] vertexTokens = aseContent[i + 2 + j].Replace('.', ',').Split('\t');

                            positions[j] = new Vector3(float.Parse(vertexTokens[4]), float.Parse(vertexTokens[5]), float.Parse(vertexTokens[6]));
                        }

                        TressFXStrand strand = new TressFXStrand();
                        strand.vertices = positions;
                        strand.texcoordX = texcoordMultiplier * currentStrand;
                        currentStrands.Add(strand);

                        i = i + 1 + positions.Length;

                        currentStrand++;
                    }
                }
            }

            ConsoleUtil.LogToConsole("Asefile Parsed! Hairs parsed: " + currentHairId + "!", ConsoleColor.Green);

            return hairMeshes;
        }
    }
}
