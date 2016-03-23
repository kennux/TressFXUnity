using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TressFXLib.Numerics;

namespace TressFXLib
{
    /// <summary>
    /// This is the hair mesh class.
    /// It contains hair information for a single "hair submesh".
    /// </summary>
    public class HairMesh
    {
        /// <summary>
        /// Gets the list of hair strands.
        /// </summary>
        public List<HairStrand> strands
        {
            get { return this._strands; }
        }
        private List<HairStrand> _strands = new List<HairStrand>();

        /// <summary>
        /// If this is set to true both ends of all strands inside of this hair mesh are not movable by physics.
        /// </summary>
        public bool bothEndsImmovable;

        /// <summary>
        /// Returns the vertex count for this mesh.
        /// This is a expensive costly call, as it iterates through all strands!
        /// </summary>
        public int vertexCount
        {
            get
            {
                int count = 0;

                foreach (HairStrand strand in this.strands)
                    count += strand.vertices.Count;

                return count;
            }
        }

        /// <summary>
        /// Gets the count of strands in this hair mesh.
        /// </summary>
        public int strandCount
        {
            get
            {
                return this.strands.Count;
            }
        }

        /// <summary>
        /// Gets the highest vertex count of all strands.
        /// This is a very expensive call, as it iterates thorugh all strands!
        /// </summary>
        public int maxNumVerticesPerStrand
        {
            get
            {
                int highestCount = 0;

                foreach (HairStrand strand in this.strands)
                    if (highestCount < strand.vertices.Count)
                        highestCount = strand.vertices.Count;

                return highestCount;
            }
        }

        /// <summary>
        /// Exports this hair mesh as tressfx plaintext file.
        /// </summary>
        /// <param name="path"></param>
        public void ExportAsTFX(String path, int followHairPerGuideHair, float maxRadiusAroundGuideHair)
        {
            // Export as TFX
            System.Text.StringBuilder tfxStringBuilder = new StringBuilder("version 2.0" + "\r\n" +
            "scale 1.0" + "\r\n" +
            "rotation 0 0 0" + "\r\n" +
            "translation 0 0 0" + "\r\n" +
            "bothEndsImmovable 0" + "\r\n" +
            "maxNumVerticesInStrand " + this.maxNumVerticesPerStrand + "\r\n" +
            "numFollowHairsPerGuideHair " + followHairPerGuideHair + "\r\n" +
            "maxRadiusAroundGuideHair " + (maxRadiusAroundGuideHair.ToString().Replace(",", ".")) + "\r\n" +
            "numStrands " + this.strandCount + "\r\n" +
            "is sorted 1" + "\r\n");

            int strandIndex = 0;
            foreach (HairStrand s in this.strands)
            {
                if (!s.isGuidanceStrand)
                    continue;

                tfxStringBuilder.Append("strand " + strandIndex + " numVerts 16 texcoord 0.000000 000000" + "\r\n");
                foreach (HairStrandVertex v in s.vertices)
                {
                    tfxStringBuilder.Append(v.position.x + " " + v.position.y + " " + v.position.z + "\r\n");
                }

                strandIndex++;
            }

            System.IO.File.WriteAllText(path, tfxStringBuilder.ToString());
        }

        /// <summary>
        /// This function will get used by importers for example if there were only strand vertex positions available.
        /// It will calculate everything inside of the HairStrand and HairStrandVertex based off the vertex positions.
        /// 
        /// This function assumes that hair strands always have atleast 2 vertices!
        /// and atleast 2 strands.
        /// 
        /// NOTE: This is currently non-functional, it seems like somewhere some rounding issues happen that will make the output of this unusable.
        /// The logic was taken from AMD's AssetConverter. I already spent some days of debugging on this but there is something really weird going on here.
        /// My best guess is that C++ floats are more precise than C# floats for this calculations.
        /// </summary>
        public void PrepareSimulationParameters(ref int hairIndexCounter)
        {
            // TODO: Validity checks
            int counterOffset = hairIndexCounter;

            for (int i = 0; i < this.strandCount; i++)
            {
                // Get current strand
                HairStrand strand = this.strands[i];

                // Set the root follow hair offset
                if (strand.isGuidanceStrand)
                    strand.followRootOffset = new Vector4(0, 0, 0, hairIndexCounter);
                else
                {
                    if (strand.guidanceStrand == null)
                        throw new FormatException("Tried to process a follow strand without guidance strand reference!");

                    Vector3 offset = strand.vertices[0].position - strand.guidanceStrand.vertices[0].position;
                    strand.followRootOffset = new Vector4(offset, counterOffset + this.strands.FindIndex(0, this.strands.Count, s => s == strand.guidanceStrand));
                }
                
                // First vertex
                HairStrandVertex firstVertex = this.strands[i].vertices[0];
                HairStrandVertex secondVertex = this.strands[i].vertices[1];
                {
                    Vector3 vec = (secondVertex.position - firstVertex.position);
                    Vector3 vecX = Vector3.Normalize(vec);

                    Vector3 vecZ = Vector3.Cross(vecX, new Vector3(1, 0, 0));

                    if (vecZ.LengthSquared < 0.0001f)
                    {
                        vecZ = Vector3.Cross(vecX, new Vector3(0, 1, 0));
                    }

                    vecZ.Normalize();

                    Vector3 vecY = Vector3.Normalize(Vector3.Cross(vecZ, vecX));

                    // Construct rotation matrix
                    Matrix3 rotL2W = new Matrix3();
                    rotL2W.R0C0 = vecX.x;
                    rotL2W.R1C0 = vecX.y;
                    rotL2W.R2C0 = vecX.z;

                    rotL2W.R0C1 = vecY.x;
                    rotL2W.R1C1 = vecY.y;
                    rotL2W.R2C1 = vecY.z;

                    rotL2W.R0C2 = vecZ.x;
                    rotL2W.R1C2 = vecZ.y;
                    rotL2W.R2C2 = vecZ.z;

                    firstVertex.localTranslation = firstVertex.position;
                    firstVertex.localRotation = rotL2W.ToQuaternion();
                    firstVertex.globalTransform = firstVertex.localTransform;
                }

                // The rest n-1 vertices
                for (int j = 1; j < strand.vertices.Count; j++)
                {
                    Vector3 currentPosition = strand.vertices[j].position;
                    Vector3 previousPosition = strand.vertices[j-1].position;
                    HairStrandVertex previousVertex = strand.vertices[j - 1];
                    HairStrandVertex currentVertex = strand.vertices[j];
                    
                    Vector3 vec = Vector3.TressFXTransform((currentPosition - previousPosition), Quaternion.Invert(previousVertex.globalRotation));

                    Vector3 vecX = Vector3.Normalize(vec);

                    Vector3 X = new Vector3(1.0f, 0, 0);
                    Vector3 rotAxis = Vector3.Cross(X, vecX);
                    float angle = Mathf.Acos(Vector3.Dot(X, vecX));


                    if (Mathf.Abs(angle) < 0.001f || rotAxis.LengthSquared < 0.001f)
                    {
                        currentVertex.localRotation = Quaternion.Identity;
                    }
                    else
                    {
                        rotAxis.Normalize();
                        Quaternion rot = Quaternion.FromAxisAngle(rotAxis, angle);
                        currentVertex.localRotation = rot;
                    }

                    currentVertex.localTransform.translation = vec;
                    currentVertex.globalTransform = previousVertex.globalTransform.Multiply(currentVertex.localTransform);
                    // Set the reference vector
                    currentVertex.referenceVector = new Vector4(currentVertex.localTransform.translation, 0);
                }

                hairIndexCounter++;
            }
            
            for (int i = 0; i < this.strandCount; i++)
            {
                // Get current strand
                HairStrand strand = this.strands[i];
                int vertexCount = strand.vertices.Count;

                // Compute strand tangent
                {
                    strand.vertices[0].tangent = Vector3.Normalize((strand.vertices[1].position - strand.vertices[0].position));
                    strand.vertices[0].tangent.Normalize();

                    // 1 - (n-1)
                    for (int j = 1; j < vertexCount - 1; j++)
                    {
                        Vector3 tangentPre = strand.vertices[j].position - strand.vertices[j - 1].position;
                        Vector3 tangentNext = strand.vertices[j + 1].position - strand.vertices[j].position;
                        strand.vertices[j].tangent = (tangentPre + tangentNext);
                        strand.vertices[j].tangent.Normalize();
                    }

                    // Last vertex
                    strand.vertices[vertexCount - 1].tangent = Vector3.Normalize((strand.vertices[vertexCount - 1].position - strand.vertices[vertexCount - 2].position));
                    strand.vertices[vertexCount - 1].tangent.Normalize();
                }

                // The current distance to root
                float distanceToRoot = 0;

                // Compute distances to root
                for (int j = 1; j < vertexCount; j++)
                {
                    Vector3 vec = strand.vertices[j].position - strand.vertices[j-1].position;
                    float disSeg = vec.Length;
                    distanceToRoot += disSeg;
                    strand.vertices[j].distanceToRoot = distanceToRoot;
                }

                // Distance to root normalization
                for (int j = 0; j < vertexCount; j++)
                {
                    strand.vertices[j].distanceToRoot /= distanceToRoot;
                }

                // Rest length
                for (int j = 0; j < vertexCount - 1; j++)
                {
                    strand.vertices[j].restLength = (strand.vertices[j].position - strand.vertices[j + 1].position).Length;
                }

                // Calculate texcoord
                for (int j = 0; j < vertexCount - 1; j++)
                    strand.vertices[j].texcoord = new Vector4(i / (float)this.strandCount, j / (float)strand.vertices.Count, 0, 0);

                // Calculate thickness coefficient
                for (int j = 0; j < vertexCount; j++)
                {
                    float tVal = strand.vertices[j].distanceToRoot;
                    strand.vertices[j].thicknessCoefficient = Mathf.Sqrt(1.0f - tVal * tVal);
                }
            }
        }
    }
}
