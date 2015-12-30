using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TressFXLib.Numerics;

namespace TressFXLib
{
    /// <summary>
    /// Hair import settings structure.
    /// Used to define import setting for the import pipeline.
    /// </summary>
    public class HairImportSettings
    {
        public static HairImportSettings standard = new HairImportSettings();

        public Vector3 scale;

        public HairImportSettings()
        {
            this.scale = Vector3.One;
        }
    }
}
