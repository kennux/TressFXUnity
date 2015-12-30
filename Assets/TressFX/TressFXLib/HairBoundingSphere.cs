using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TressFXLib.Numerics;

namespace TressFXLib
{
    /// <summary>
    /// Represents the tressfx hair bounding sphere structure.
    /// It is used in tressfx to perform frustum culling on the hair.
    /// </summary>
    public class HairBoundingSphere
    {
	    /// <summary>
	    /// The center position of the sphere.
	    /// </summary>
	    public Vector3 center;

	    /// <summary>
	    /// The sphere radius.
	    /// </summary>
	    public float radius;

	    /// <summary>
        /// Initializes a new instance of the <see cref="HairBoundingSphere"/> struct.
	    /// </summary>
	    /// <param name="center">Center.</param>
	    /// <param name="radius">Radius.</param>
        public HairBoundingSphere(Vector3 center, float radius)
	    {
		    this.center = center;
		    this.radius = radius;
	    }
    }
}
