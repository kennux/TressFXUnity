using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TressFXLib.Formats
{
    /// <summary>
    /// The unity tressfx format was implemented in order to create an own format for the unity tressfx implementation and add some additional parameters to the rendering and simulation.
    /// </summary>
    public class UTFXFormat : TFXBFormat
    {
        
        public override void Export(BinaryWriter writer, string path, Hair hair)
        {
            base.Export(writer, path, hair);

            foreach (HairMesh mesh in hair.meshes)
                if (mesh != null)
                    foreach (HairStrand strand in mesh.strands)
                        foreach (HairStrandVertex vertex in strand.vertices)
                            WriteVector4(writer, vertex.texcoord);
        }

        public override HairMesh[] Import(BinaryReader reader, string path, Hair hair, HairImportSettings importSettings)
        {
            HairMesh[] meshes = base.Import(reader, path, hair, importSettings);
            int id = 0;

            foreach (HairMesh mesh in meshes)
                if (mesh != null)
                    foreach (HairStrand strand in mesh.strands)
                        foreach (HairStrandVertex vertex in strand.vertices)
                        {
                            vertex.texcoord = ReadVector4(reader);
                            id++;
                        }

            return meshes;
        }
    }
}
