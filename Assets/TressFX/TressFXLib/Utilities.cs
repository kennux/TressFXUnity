using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TressFXLib.Numerics;

namespace TressFXLib
{
    class Utilities
    {
        private static Random rand = new Random();

        //--------------------------------------------------------------------------------------
        //
        // GetTangentVectors
        //
        // Create two arbitrary tangent vectors (t0 and t1) perpendicular to the input normal vector (n).
        //
        //--------------------------------------------------------------------------------------
        public static void GetTangentVectors(Vector3 n, out Vector3 t0, out Vector3 t1)
        {
            t0 = new Vector3();
            t1 = new Vector3();

            if (Mathf.Abs(n.z) > 0.707f)
            {
                float a = n.y * n.y + n.z * n.z;
                float k = 1.0f / Mathf.Sqrt(a);
                t0.x = 0;
                t0.y = -n.z * k;
                t0.z = n.y * k;

                t1.x = a * k;
                t1.y = -n.x * t0.z;
                t1.z = n.x * t0.y;
            }
            else
            {
                float a = n.x * n.x + n.y * n.y;
                float k = 1.0f / Mathf.Sqrt(a);
                t0.x = -n.y * k;
                t0.y = n.x * k;
                t0.z = 0;

                t1.x = -n.z * t0.y;
                t1.y = n.z * t0.x;
                t1.z = a * k;
            }
        }

        public static float GetRandom(float Min, float Max)
        {
            return ((float)rand.NextDouble())* (Max - Min) + Min;
        }

        public static string[] ReadAllLines(BinaryReader reader)
        {
            List<string> strings = new List<string>();
            StringBuilder temp = new StringBuilder();

            char lastChar = reader.ReadChar();
            // an EndOfStreamException here would propogate to the caller

            try
            {
                while (true)
                {
                    char newChar = reader.ReadChar();
                    if (lastChar == '\r' && newChar == '\n')
                    {
                        strings.Add(temp.ToString());
                    }

                    temp.Append(lastChar);
                    lastChar = newChar;
                }
            }
            catch (EndOfStreamException)
            {
                temp.Append(lastChar);
                strings.Add(temp.ToString());
                return strings.ToArray();
            }
        }

        /// <summary>
        /// Executes the given action on every strand on the hair.
        /// </summary>
        /// <param name="hair"></param>
        /// <param name="action"></param>
        public static void StrandLevelIteration(Hair hair, Action<HairStrand> action)
        {
            foreach (HairMesh m in hair.meshes)
                if (m != null)
                    foreach (HairStrand s in m.strands)
                        action(s);
        }

        /// <summary>
        /// Executes the given action on every vertex on the hair.
        /// </summary>
        /// <param name="hair"></param>
        /// <param name="action"></param>
        public static void StrandLevelIteration(Hair hair, Action<HairStrand, HairStrandVertex> action)
        {
            foreach (HairMesh m in hair.meshes)
                if (m != null)
                    foreach (HairStrand s in m.strands)
                        foreach (HairStrandVertex v in s.vertices)
                            action(s, v);
        }

        /// <summary>
        /// Gets the vector3 for the given distance to root for the given strand.
        /// 
        /// If the dist to root is > than the hair length, the last vertex is returned, if its shorter then the first hair vertex is returned.
        /// </summary>
        /// <param name="strand"></param>
        /// <param name="distToRoot"></param>
        /// <returns></returns>
        public static Vector3 GetPositionOnStrand(HairStrand strand, float distToRoot)
        {
            // Get strand root
            Vector3 strandRoot = strand.vertices[0].position;
            float strandLength = strand.length;

            if (distToRoot <= 0)
                return strandRoot;
            else if (distToRoot >= strandLength)
                return strand.vertices[strand.vertices.Count-1].position;

            // Loop variables
            float rootDist = 0;
            Vector3 lastPos = strandRoot;
            int lineSegment = -1;
            float[] rootDists = new float[strand.vertices.Count];

            // Find the line segment
            for (int i = 1; i < strand.vertices.Count; i++)
            {
                Vector3 curPos = strand.vertices[i].position;

                // Calculate root dists
                rootDist += (lastPos - curPos).Length;
                rootDists[i] = rootDist;

                if (rootDist > distToRoot)
                {
                    // We found the segment
                    lineSegment = i;
                    break;
                }

                lastPos = curPos;
            }

            if (lineSegment == -1 || lineSegment == 0)
                throw new KeyNotFoundException("Could not find the line segment for dist to root = " + distToRoot + "!");

            // Sample position
            Vector3 segmentStart = strand.vertices[lineSegment-1].position;
            Vector3 segmentEnd = strand.vertices[lineSegment].position;
            
            // Build lerp t factor
            float segmentStartDist = rootDists[lineSegment-1];
            float segmentEndDist = rootDists[lineSegment];
            float segmentLength = segmentEndDist - segmentStartDist;

            float lerpT = (distToRoot - segmentStartDist) / segmentLength;

            return Vector3.Lerp(segmentStart, segmentEnd, lerpT);
        }

    }
}
