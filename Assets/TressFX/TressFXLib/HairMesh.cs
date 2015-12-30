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
            "maxRadiusAroundGuideHair " + (maxRadiusAroundGuideHair.ToString().Replace(",",".")) + "\r\n" +
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

                // The current strand length.
                float strandLength = strand.length;

                // The current distance to root
                float distanceToRoot = 0;

                for (int j = 0; j < strand.vertices.Count; j++)
                {
                    // Get current vertex
                    HairStrandVertex vertex = strand.vertices[j];
                    HairStrandVertex lastVertex = strand.vertices[strand.vertices.Count - 1];
                    HairStrandVertex firstVertex = this.strands[j].vertices[0];
                    HairStrandVertex secondVertex = this.strands[j].vertices[1];
                    HairStrandVertex preLastVertex = strand.vertices[strand.vertices.Count - 2];

                    // Get next vertex
                    HairStrandVertex nextVertex = null;
                    if (j < strand.vertices.Count - 1)
                        nextVertex = strand.vertices[j + 1];
                    else
                    {
                        if (i < this.strandCount - 1)
                            nextVertex = this.strands[i + 1].vertices[0];
                        else
                            nextVertex = new HairStrandVertex(Vector3.Zero, new Vector3(0, 0, 1), Vector4.Zero);
                    }

                    // Get previous vertex
                    HairStrandVertex previousVertex = null;
                    if (j > 0)
                        previousVertex = strand.vertices[j - 1];
                    else
                    {
                        if (i > 0)
                        {
                            HairStrand prevStrand = this.strands[i - 1];
                            previousVertex = prevStrand.vertices[prevStrand.vertices.Count - 1];
                        }
                        else
                            previousVertex = new HairStrandVertex(Vector3.Zero, new Vector3(0, 0, 1), Vector4.Zero);
                    }

                    // Get current and next position vectors
                    Vector3 currentPosition = vertex.position;
                    Vector3 nextPosition = nextVertex.position;
                    Vector3 previousPosition = previousVertex.position;

                    // Calculate vertex level information

                    // Initialize first vertex 
                    // First vertex global and local frame initialization
                    // Gets the direction from the current position to the next position
                    if (j == 0)
                    {
                        if (Mathf.Abs(currentPosition.x - (-20.3614f)) <= 0.0001f && Mathf.Abs(currentPosition.y - (81.1211f)) <= 0.0001f && Mathf.Abs(currentPosition.z - (-18.1023f)) <= 0.0001f)
                        {
                            int jk = 0;
                        }

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
                    else
                    {
                        // -6.54806995f 71.0205994f 44.4449997f

                        // if (vertex.position.x == -4.19069004f && vertex.position.y == 73.1125031f && vertex.position.z == 42.9104004f)
                        if (i == 10 && j == 9) // vertex.position.x == -33.5211029f && vertex.position.y == -21.6556110f && vertex.position.z == -11.5229073f)
                        {
                            int elite = 123;
                        }

                        Vector3 v = Vector3.Cross(new Vector3(1, 0, 0), new Vector3(1, 0, 0.010100123f));
                        float d = Vector3.Dot(new Vector3(1, 0, 0), new Vector3(1, 0, 0.010100123f));
                        float angleTest = Mathf.Acos(d);

                        Vector3 vec = Vector3.TressFXTransform((currentPosition - previousPosition), Quaternion.Invert(previousVertex.globalRotation));

                        Vector3 vecX = Vector3.Normalize(vec);

                        Vector3 X = new Vector3(1.0f, 0, 0);
                        Vector3 rotAxis = Vector3.Cross(X, vecX);
                        float angle = Mathf.Acos(Vector3.Dot(X, vecX));


                        if (Mathf.Abs(angle) < 0.0011f || rotAxis.LengthSquared < 0.0011f)
                        {
                            vertex.localRotation = Quaternion.Identity;
                        }
                        else
                        {
                            rotAxis.Normalize();
                            Quaternion rot = Quaternion.FromAxisAngle(rotAxis, angle);
                            vertex.localRotation = rot;
                        }

                        vertex.localTransform.translation = vec;
                        vertex.globalTransform = previousVertex.globalTransform.Multiply(vertex.localTransform);
                        // Set the reference vector
                        vertex.referenceVector = new Vector4(vertex.localTransform.translation, 0);
                    }

                    // Rest length
                    if (j < strand.vertices.Count - 1)
                        vertex.restLength = (strand.vertices[j].position - strand.vertices[j + 1].position).Length;
                    else
                        vertex.restLength = 0;

                    // Compute strand tangent

                    // Calculate tangents for every vertex except the first and the last
                    if (j == 0)
                    {
                        strand.vertices[0].tangent = Vector3.Normalize((strand.vertices[1].position - strand.vertices[0].position));

                    }
                    else if (j == strand.vertices.Count - 1)
                    {
                        // Calculate the last vertex tangent
                        lastVertex.tangent = Vector3.Normalize((lastVertex.position - preLastVertex.position));
                    }
                    else
                    {
                        Vector3 tangent_pre = Vector3.Normalize(vertex.position - previousVertex.position);
                        Vector3 tangent_next = Vector3.Normalize(nextVertex.position - vertex.position);
                        vertex.tangent = Vector3.Normalize(tangent_pre + tangent_next);
                    }

                    // Calculate texcoord
                    vertex.texcoord = new Vector4(i / (float)this.strandCount, j / (float)strand.vertices.Count, 0, 0);

                    // Calculate thickness coefficient
                    if (j > 0)
                    {
                        float segmentLength = (vertex.position - previousVertex.position).Length;
                        distanceToRoot += segmentLength;
                    }

                    float tVal = distanceToRoot / strandLength;
                    vertex.thicknessCoefficient = Mathf.Sqrt(1.0f - tVal * tVal);
                }

                hairIndexCounter++;
            }
        }
    }
}
