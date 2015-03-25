using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AseToTFX.Util
{
    /// <summary>
    /// Very simple vector3 implementation
    /// </summary>
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public static Vector3 zero
        {
            get { return new Vector3(0, 0, 0); }
        }

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}
