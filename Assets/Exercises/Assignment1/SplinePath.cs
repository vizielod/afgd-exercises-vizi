using System;
using System.Collections.Generic;
using System.Numerics;
using AfGD.Assignment1;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace AfGD
{
    public class SplinePath : MonoBehaviour
    {
        private List<Vector3> path = new List<Vector3>();
        
        CurveSegment curve;
        
        // we get the control points directly from the pathfinding object
        [Tooltip("Path holder")]
        [SerializeField] public PathFinding pathfinder;
        
        [Tooltip("Set the curve type")]
        [SerializeField] private CurveType curveType = CurveType.BEZIER;

        // we will need an array of curves for the path (instead of a single curve)
        public CurveSegment[] curves;

        // exercise 2.5
        // this array is used to store the normalized, cumulative arclength of the path
        // it relates the array index to the arclength of the path, so we can think of it
        // as a table
        private float[] normArclength;
        private int arclengthEntries = 200; // the length of our table/array

        [Range(0.0f, 1.0f)]
        [SerializeField] private float tightness;

        // these variables are only used for visualization
        [Header("Debug variables")]
        [Range(2, 100)]
        public int debugSegments = 20;
        public bool drawPath = true;
        public Color pathColor = Color.magenta;
        public bool drawTangents = true;
        public Color tangentColor = Color.green;


        bool Init()
        {
            if (!pathfinder)
            {
                Debug.LogError("No pathfinder assigned to the curve!");
                return false;
            }
            
            path = pathfinder.m_Path;
            int points = path.Count;
            if (curves == null || curves.Length != points-1)
                curves = (points == 0) ? new CurveSegment[0] : new CurveSegment[points-1];

            // instantiate a curve segment for each valid sequence of points
            for (int i = 0; i < points - 1; i++)
            {
                Vector3 cp1 = Vector3.zero, cp2 = Vector3.zero , cp3 = Vector3.zero, cp4 = Vector3.zero;
                cp1 = path[i];
                cp4 = path[i + 1];
                cp2 = CardinalSplineTangent(cp1);
                cp3 = CardinalSplineTangent(cp4);
                curves[i] = new CurveSegment(cp1, cp2, cp3, cp4, (CurveType)curveType);
            }

            // compute the cumulative arclength, which we use to sample the curve
            // with a parameter that is linear with relation to the arclength of the curve
            UpdateArclength();

            return true;
        }

        public int GetCurveCount()
        {
            return curves.Length;
        }

        public List<Vector3> GetPointsOnCurveSegment(CurveSegment curve, int segments = 50)
        {
            List<Vector3> pointsOnCurveSegment = new List<Vector3>();
            float interval = 1.0f / segments;
            for (int i = 0; i <= segments; i++)
            {
                float start_u = i * interval;

                Vector3 startPoint = curve.Evaluate(start_u);
                pointsOnCurveSegment.Add(startPoint);
            }
            return pointsOnCurveSegment;
        }


        public Vector3 GetSinglePointOnCurveSegment(CurveSegment curve, float t)
        {
            t = Mathf.Clamp01(t);
            return curve.Evaluate(t);
        }

        public Vector3 GetDirection(/*List<CurveSegment> curves*/CurveSegment[] curves, float progress)
        {
            Vector3 direction = Vector3.zero;
            int curveidx = 0;
            float t = 0;
            if (curves.Length > 0)
            {
                curveidx = (int)progress % curves.Length;
                t = progress - curveidx;
            }
            if (curveidx < path.Count - 1)
                direction = curves[curveidx].EvaluateDv(t);
            return direction;
        }

        public Vector3 GetPoint(CurveSegment[] curves, float progress)
        {
            Vector3 point = Vector3.zero;
            int curveidx = 0;
            float t = 0;
            if (/*curves.Count > 0*/curves.Length > 0)
            {
                curveidx = (int)progress % curves.Length;
                t = progress - curveidx;
                Debug.Log(curveidx);
            }
            if (curveidx < path.Count - 1)
            {
                point = GetSinglePointOnCurveSegment(curves[curveidx], t);
            }
            return point;
        }

        public static void DrawCurveSegments(CurveSegment curve,
            Color color, int segments = 50)
        {
            float interval = 1.0f / segments;
            Vector3 lastPos = curve.Evaluate(0);
            for (int i = 1; i <= segments; i++)
            {
                float u = interval * (float)i;
                Vector3 pos = curve.Evaluate(u);

                UnityEngine.Debug.DrawLine(lastPos, pos, color);
                lastPos = pos;
            }
        }

        public static void DrawTangents(CurveSegment curve,
            Color color, int segments = 50, float scale = 0.1f)
        {
            float interval = 1.0f / segments;

            for (int i = 0; i <= segments; i++)
            {
                float u = interval * (float)i;
                Vector3 pos = curve.Evaluate(u);
                Vector3 tangent = curve.EvaluateDv(u);

                UnityEngine.Debug.DrawLine(pos, pos + tangent * scale, color);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            Init();

        }

        // Update is called once per frame
        void Update()
        {
            if (Application.isEditor)
            {
                // reinitialize if we change something while not playing
                if (!Init())
                    return;
            }

            for (int i = 0; i < curves.Length; i++)
            {
                if (drawPath)
                    DrawCurveSegments(curves[i], pathColor, debugSegments);
                if (drawTangents)
                    DrawTangents(curves[i], tangentColor, debugSegments);
            }
        }
        
        private Vector3 FiniteDifferenceTangent(Vector3 cp)
        {
            Vector3 tangent = new Vector3();
            if (path.Contains(cp))
            {
                int index = path.IndexOf(cp);
                if (index == 0 && path.Count > 1)
                {
                    tangent = 0.5f * (path[1] - path[index]) / (path[1].x - path[index].x);
                }  
                else if (index == path.Count - 1) 
                {
                    tangent = 0.5f * (path[index] - path[index - 1]) / (path[index].x - path[index - 1].x);
                }
                else
                {
                    tangent = 0.5f * ((path[1] - path[index]) / (path[1].x - path[index].x) + (path[index] - path[index - 1]) / (path[index].x - path[index - 1].x));
                }
            }
            return tangent;
        }

        private Vector3 CardinalSplineTangent(Vector3 cp)
        {
            Vector3 tangent = new Vector3();
            if (path.Contains(cp))
            {
                Matrix3x2 counterClockwise2D = new Matrix3x2(1, 0, 0, 0, -1, 0);
                int index = path.IndexOf(cp);
                if (index == 0 && path.Count > 1)
                {
                    tangent = path[1] - path[index];
                }  
                else if (index == path.Count - 1) 
                {
                    tangent = path[index] - path[index - 1];
                }
                else
                {
                    tangent = tightness *
                              (path[index + 1] - path[index - 1]);
                }
            }
            //return tangent.normalized;
            return tangent;
        }

        /// <summary>
        /// Evaluate the point in the path at parameter 's', 
        /// 's' is transformed to 'u' if arclengthParameterization is set
        /// Evaluate the tangent if derivative==1, and the curvature if derivative==2
        /// </summary>
        /// <param name="s"></param>
        /// <param name="arclengthParameterization"></param>
        /// <param name="derivative"></param>
        /// <returns></returns>
        public Vector3 Evaluate(float s, bool arclengthParameterization = false, int derivative = 0)
        {
            /*if (curves == null)
                if (!Init()) return Vector3.zero;*/

            if((curves == null && !Init()) || curves.Length == 0)
                return Vector3.zero;

            // exercise 2.5
            // should we use 's' as is or parameterize by the arclength of the path?
            float u = arclengthParameterization ? ParameterizeByArclength(s) : s;

            // scale 'u' from [0,1] to [0, curves.Length]
            float pathU = u * curves.Length;
            // round down pathU to retrieve the curve segment ID
            int curveID = (int)pathU;
            // ensure that the curveID is in a valid range
            curveID = Mathf.Clamp(curveID, 0, curves.Length - 1);
            // 'u' in the selected curve segment
            float curveU = pathU - (float)curveID;


            if (derivative == 1)
                return curves[curveID].EvaluateDv(curveU);
            if (derivative == 2)
                return curves[curveID].EvaluateDv2(curveU);

            return curves[curveID].Evaluate(curveU);
        }

        // exercise 2.5
        /// <summary>
        /// construct a table with the normalized, cumulative arclength of the curve
        /// </summary>
        void UpdateArclength()
        {
            // compute entry intervals so that interval * arclengthEntries == 1
            float interval = 1.0f / (float)arclengthEntries; // the interval is basically the u paramter

            // table initialization
            normArclength = new float[arclengthEntries];
            normArclength[0] = 0.0f;

            // start position in the path
            Vector3 lastPos = Evaluate(0, false);

            for (int i = 1; i < arclengthEntries; i++)
            {
                // sample path at fixed intervals
                float u = interval * (float)i;
                Vector3 pos = Evaluate(u, false);

                // compute the distance between two consecutive samples and 
                // accumulate in arclength
                float s = normArclength[i - 1] + Vector3.Distance(lastPos, pos);
                normArclength[i] = s;

                lastPos = pos;
            }
            // now the entries in normArclength are in the range [0, pathArclenght]

            // the last element in normArclength is the total arclength of the path
            float pathArclenght = normArclength[normArclength.Length - 1];

            // ensure there is no division by 0, 
            // could happen if all control points are in the same place
            if (pathArclenght <= float.Epsilon)
                throw new System.Exception("totalArclength must be positive and bigger than 0");

            // normalise the arclength values by dividing by the last element
            for (int i = 1; i < arclengthEntries; i++)
            {
                normArclength[i] = normArclength[i] / pathArclenght;
            }
            // now the entries in normArclength are in the range [0, 1]
        }

        // exercise 2.5
        /// <summary>
        /// Parameterize by arclength. Given the table of (normalized) cumulative 
        /// arclengths of the path and a parameter 's', returns a parameter 'u' 
        /// that evaluates the point at s*arclength of the path
        /// </summary>
        /// <param name="s"></param>
        /// <returns>returns the 'u' parameter</returns>
        float ParameterizeByArclength(float s)
        {
            // ensure tha s is in a valid range
            s = Mathf.Clamp(s, 0.0f, 1.0f);

            // we perform a binary search to find the entries in the 
            // table that are closer to 's'
            int totalSegments = normArclength.Length - 1;
            int min = 0;
            int max = totalSegments;
            int current = max / 2;

            while (true)
            {
                // if min and max are neighbours, we found the best approximation
                // of 's' in the table
                if (min == max - 1)
                {
                    // table the two best approximations of 's' in the table
                    // and compute the closest 'u' with a linear interpolation
                    // this means that 'u' is just an approximation of the correct parameter
                    float s1 = normArclength[min];
                    float s2 = normArclength[max];
                    float u1 = (float)min / (float)totalSegments;
                    float u2 = (float)max / (float)totalSegments;
                    float delta_s = (s - s1) / (s2 - s1);

                    return Mathf.Lerp(u1, u2, delta_s); ;
                }

                // adjust the bounds of our search range
                if (s > normArclength[current])
                    min = current;
                else
                    max = current;

                // index in the middle of our current search range
                current = min + (max - min) / 2;
            }
        }

    }
}