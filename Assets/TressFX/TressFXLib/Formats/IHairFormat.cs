using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TressFXLib.Formats
{
    /// <summary>
    /// This interface is used in order to implement hair formats.
    /// Hair formats can get imported and/or exported.
    /// 
    /// For example, if there is a format which should be import only, a not implemented exception will get thrown in the export function.
    /// </summary>
    public interface IHairFormat
    {
        /// <summary>
        /// Exports the given hair to the given path.
        /// </summary>
        /// <param name="hair">The hair the user wants to export</param>
        /// <param name="writer">A binary writer instance where the exported hair should be saved to.</param>
        void Export(BinaryWriter writer, string path, Hair hair);

        /// <summary>
        /// Imports the file at the given path
        /// </summary>
        /// <param name="reader">A binary reader instance of the file the user wants to import.</param>
        /// <param name="hair">The hair instance in which the hair meshes will get loaded into.</param>
        /// <returns>The imported hair data</returns>
        HairMesh[] Import(BinaryReader reader, string path, Hair hair, HairImportSettings importSettings);
    }
}
