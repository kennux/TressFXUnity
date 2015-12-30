using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TressFXLib.Numerics;

namespace TressFXLib
{
    /// <summary>
    /// Represents a hair strand transformation.
    /// Every vertex has 2 transforms (local and global).
    /// </summary>
    public struct HairStrandVertexTransform
    {
        /// <summary>
        /// The transformation's translation
        /// </summary>
        public Vector3 translation;

        /// <summary>
        /// The transformation's rotation
        /// </summary>
        public Quaternion rotation;

        public HairStrandVertexTransform(Vector3 translation, Quaternion rotation)
        {
            this.translation = translation;
            this.rotation = rotation;
        }

        /// <summary>
        /// Multiplies this transform with the given other transform.
        /// It will then return a new transform containing the result of the operation.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public HairStrandVertexTransform Multiply(HairStrandVertexTransform other)
        {
            return new HairStrandVertexTransform(Vector3.MultiplyQuaternion(other.translation, this.rotation) + this.translation, Quaternion.TressFXMultiply(this.rotation, other.rotation));
        }
    }
}
