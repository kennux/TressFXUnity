using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TressFXLib.Formats;

namespace TressFXLib
{
    /// <summary>
    /// Hair format enumeration.
    /// </summary>
    public enum HairFormat
    {
        ASE,
        TFXB,
        OBJ
    }

    public static class HairFormatExt
    {
        /// <summary>
        /// Returns the implementation of the hair format.
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static IHairFormat GetFormatImplementation(this HairFormat format)
        {
            switch (format)
            {
                case HairFormat.ASE:
                    return new AseFormat();
                case HairFormat.TFXB:
                    return new TFXBFormat();
                case HairFormat.OBJ:
                    return new WavefrontObjFormat();
                default:
                    throw new FormatException("Format unknown!"); // This should never happen if the library is unmodified
            }
        }
    }
}
