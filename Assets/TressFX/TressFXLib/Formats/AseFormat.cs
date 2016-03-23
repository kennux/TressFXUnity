using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TressFXLib.Numerics;

namespace TressFXLib.Formats
{
    /// <summary>
    /// Ascii scene exporter file format implementation.
    /// This file format can only get imported, not exported.
    /// 
    /// Important Note:
    /// The ASE File Format does not specify hairsimulation parameters,
    /// so after importing the ase data the simulation parameters must get prepared manually in order to do a correct import!
    /// 
    /// TODO: Re-implement! (ATM just Quick'n'Dirty code)
    /// </summary>
    public class AseFormat : IHairFormat
    {
        public void Export(BinaryWriter writer, string path, Hair hair)
        {
            throw new NotImplementedException("Hair cannot get exported as ASE file");
        }

        public HairMesh[] Import(BinaryReader reader, string path, Hair hair, HairImportSettings importSettings)
        {
            reader.Close();
            // Initialize the import
            // Load the ase file content
            // Create a List for all meshes
            // Create a list for the current-mesh strands
            string[] aseContent = File.ReadAllLines(path);
            List<HairMesh> hairMeshes = new List<HairMesh>();
            List<HairStrand> currentStrands = new List<HairStrand>();

            // Init "state"-variables
            int currentStrand = 0;
            int currentHairId = -1;

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
                            // Start parsing next mesh after flushing the current strands buffer
                            currentHairId++;
                            currentStrand = 0;

                            // Add to mesh list / flush current strands buffer
                            HairMesh hairMesh = new HairMesh();
                            foreach (HairStrand strand in currentStrands)
                            {
                                hairMesh.strands.Add(strand);
                            }
                            hairMeshes.Add(hairMesh);

                            // Clear current strands
                            currentStrands.Clear();
                        }
                    }
                    else if (tokens[0] == "*SHAPE_LINE")
                    {
                        HairStrand strand = new HairStrand();
                        strand.isGuidanceStrand = true;

                        string[] vertexCountTokens = aseContent[i + 1].Split(' ');

                        // Parse the current line
                        int vertexCount = int.Parse(vertexCountTokens[1]);

                        // Parse vertices
                        for (int j = 0; j < vertexCount; j++)
                        {
                            string[] vertexTokens = aseContent[i + 2 + j].Replace('.', ',').Split('\t');

                            if (vertexTokens[2] == "*SHAPE_VERTEX_INTERP")
                                continue;

                            System.Globalization.NumberFormatInfo nf
                            = new System.Globalization.NumberFormatInfo()
                            {
                                NumberGroupSeparator = "."
                            };

                            vertexTokens[4] = vertexTokens[4].Replace(',', '.');
                            vertexTokens[5] = vertexTokens[5].Replace(',', '.');
                            vertexTokens[6] = vertexTokens[6].Replace(',', '.');
                            Vector3 position = new Vector3(float.Parse(vertexTokens[4], nf), float.Parse(vertexTokens[6], nf), float.Parse(vertexTokens[5], nf));

                            position = Vector3.Multiply(position, importSettings.scale);
                            HairStrandVertex v = new HairStrandVertex(position, Vector3.Zero, Vector4.Zero);

                            if (strand.vertices.Count == 0)
                                v.isMovable = false;

                            strand.vertices.Add(v);
                        }
                        currentStrands.Add(strand);

                        // Increment file-line-pointer
                        i = i + 1 + vertexCount;

                        currentStrand++;
                    }
                }
            }

            // Shuffle strands
            currentStrands = FormatHelper.Shuffle(currentStrands);

            // Get last mesh
            HairMesh lastMesh = new HairMesh();
            lastMesh.strands.AddRange(currentStrands);
            hairMeshes.Add(lastMesh);

            return hairMeshes.ToArray();
        }
    }
}
