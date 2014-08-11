using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AseToTFX.Util;

namespace AseToTFX
{
    class Program
    {
        static void Main(string[] args)
        {
            // Check args length
            if (args.Length < 1)
            {
                LogToConsole("ERROR! Wrong syntax! asetotfx.exe [ASE FILE] [TFX PREFIX]", ConsoleColor.Red);
                return;
            }

            // Load args
            string asefile = args[0];
            string hairFilePrefix = args[1];

            // Start parsing the file
            LogToConsole("Loading ASE File...", ConsoleColor.Blue);
            string[] aseContent = File.ReadAllLines(asefile);
            List<TressFXStrand[]> currentStrands = new List<TressFXStrand[]>();
            int currentStrand = 0;
            int currentHairId = -1;
            float texcoordMultiplier = 0;

            LogToConsole("Starting ASE parsing... This may take a LONG while..", ConsoleColor.Blue);

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
                        currentStrands.Add(new TressFXStrand[int.Parse(tokens[1])]);
                        currentStrand = 0;
                        currentHairId++;

                        texcoordMultiplier = 1.0f / (float)int.Parse(tokens[1]);
                        LogToConsole("Starting parse hair: " + currentHairId + ", lines count: " + int.Parse(tokens[1]), ConsoleColor.Yellow);
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

                        currentStrands[currentHairId][currentStrand] = new TressFXStrand();
                        currentStrands[currentHairId][currentStrand].vertices = positions;
                        currentStrands[currentHairId][currentStrand].texcoordX = texcoordMultiplier * currentStrand;

                        i = i + 1 + positions.Length;

                        currentStrand++;
                    }
                }
            }

            LogToConsole("Asefile Parsed! Hairs parsed: " + currentHairId + " starting TressFX Export...", ConsoleColor.Green);

            // Build TFX files
            for (int i = 0; i < currentStrands.Count; i++)
            {
                LogToConsole("Exporting hair " + i + "...", ConsoleColor.Yellow); 
                StringBuilder currentHairBuilder = new StringBuilder();
                currentHairBuilder.Append("numStrands " + currentStrands[i].Length + "\r\nis sorted 1\r\n");

                // Write strands
                for (int j = 0; j < currentStrands[i].Length; j++)
                {
                    currentHairBuilder.Append("strand " + j + " numVerts " + currentStrands[i][j].vertices.Length + " texcoord " + currentStrands[i][j].texcoordX + " 000000\r\n");
                    for (int k = 0; k < currentStrands[i][j].vertices.Length; k++)
                    {
                        currentHairBuilder.Append(currentStrands[i][j].vertices[k].x + " " + currentStrands[i][j].vertices[k].y + " " + currentStrands[i][j].vertices[k].z + "\r\n");
                    }
                }

                File.WriteAllText(hairFilePrefix + "_" + i + ".txt", currentHairBuilder.ToString().Replace(",", "."));
                LogToConsole("Exported hair " + i + "!", ConsoleColor.Green);
                currentStrands[i] = null;
            }


            LogToConsole("Everything done :->", ConsoleColor.Green);
            Console.ReadLine();
        }

        private static void LogToConsole(string message, ConsoleColor color)
        {
            ConsoleColor c = Console.ForegroundColor;

            Console.ForegroundColor = color;
            Console.WriteLine(message);

            Console.ForegroundColor = c;
        }
    }
}
