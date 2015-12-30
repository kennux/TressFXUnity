using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TressFXLib.Numerics;

namespace TressFXLib
{
    /// <summary>
    /// This class represents a single hair strand inside of a hair mesh.
    /// </summary>
    public class HairStrand
    {
        /// <summary>
        /// The follow hair root offset.
        /// </summary>
        public Vector4 followRootOffset;

        /// <summary>
        /// Reference to the guidance strand.
        /// This is null on a guidance hair!
        /// </summary>
        public HairStrand guidanceStrand;

        /// <summary>
        /// Gets the list of hair strand vertices.
        /// </summary>
        public List<HairStrandVertex> vertices
        {
            get { return this._vertices; }
        }
        private List<HairStrandVertex> _vertices = new List<HairStrandVertex>();

        /// <summary>
        /// Gets the strand length.
        /// This getter calculates the strand length everytime its called.
        /// </summary>
        public float length
        {
            get
            {
                float _length = 0;

                // compute strand tangent
                for (int i = 1; i < this.vertices.Count; i++)
                {
                    _length += (this.vertices[i].position - this.vertices[i - 1].position).Length;
                }

                return _length;
            }
        }

        /// <summary>
        /// Indicates whether this strand is a guidance strand or not.
        /// Standard is true.
        /// 
        /// The simulation parameter preparation function will automatically set this flag!
        /// </summary>
        public bool isGuidanceStrand
        {
            get { return this._isGuidanceStrand; }
            set { this._isGuidanceStrand = value; }
        }
        private bool _isGuidanceStrand = true;
    }
}
