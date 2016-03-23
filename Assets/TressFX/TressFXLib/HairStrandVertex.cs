using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TressFXLib.Numerics;

namespace TressFXLib
{
    /// <summary>
    /// This class represents a single vertex inside a hair strand.
    /// </summary>
    public class HairStrandVertex
    {
        /// <summary>
        /// The position of this vertex.
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// The tangent of this vertex.
        /// </summary>
        public Vector3 tangent;

        /// <summary>
        /// The texcoord of this strand.
        /// xy = 1. texcoord,
        /// zw = 2. texcoord (normally unused)
        /// </summary>
        public Vector4 texcoord;

        /// <summary>
        /// The reference vector.
        /// </summary>
        public Vector4 referenceVector;

        /// <summary>
        /// The global rotation of this strand vertex.
        /// </summary>
        public Quaternion globalRotation
        {
            get
            {
                return this.globalTransform.rotation;
            }
            set
            {
                this.globalTransform.rotation = value;
            }
        }

        /// <summary>
        /// The global transformation's translation.
        /// </summary>
        public Vector3 globalTranslation
        {
            get
            {
                return this.globalTransform.translation;
            }
            set
            {
                this.globalTransform.translation = value;
            }
        }

        /// <summary>
        /// The local rotation of this strand vertex.
        /// </summary>
        public Quaternion localRotation
        {
            get
            {
                return this.localTransform.rotation;
            }
            set
            {
                this.localTransform.rotation = value;
            }
        }

        /// <summary>
        /// The local transformation's translation.
        /// </summary>
        public Vector3 localTranslation
        {
            get
            {
                return this.localTransform.translation;
            }
            set
            {
                this.localTransform.translation = value;
            }
        }

        /// <summary>
        /// The thickness coefficient of this vertex.
        /// </summary>
        public float thicknessCoefficient;

        /// <summary>
        /// The rest length of this vertex.
        /// </summary>
        public float restLength;

        /// <summary>
        /// If this is set to false, the hair strand vertex will become immovable.
        /// </summary>
        public bool isMovable = true;

        /// <summary>
        /// The distance from this vertex to the root of the hair.
        /// This is generated during simulation data preparation.
        /// This value is normalized between 0 and 1
        /// </summary>
        public float distanceToRoot;

        /// <summary>
        /// The global transformation of the current vertex.
        /// </summary>
        public HairStrandVertexTransform globalTransform = new HairStrandVertexTransform();

        /// <summary>
        /// The local transformation of the current vertex.
        /// </summary>
        public HairStrandVertexTransform localTransform = new HairStrandVertexTransform();

        /// <summary>
        /// Creates a 4-component tressfx position vector.
        /// Tressfx uses the first 3 components for the actual world position (XYZ) and the fourth component as "movable flag".
        /// If the movable flag is 0, the hair is immovable. If it is > 0 it is movable.
        /// </summary>
        /// <returns></returns>
        public Vector4 GetTressFXPosition()
        {
            return new Vector4(this.position, (this.isMovable ? 1 : 0));
        }

        /// <summary>
        /// "Easy-init" constructor.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="tangent"></param>
        /// <param name="texcoord"></param>
        public HairStrandVertex(Vector3 position, Vector3 tangent, Vector4 texcoord)
	    {
		    this.position = position;
		    this.tangent = tangent;
		    this.texcoord = texcoord;
	    }
    }
}
