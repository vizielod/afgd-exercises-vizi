using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AfGD.Assignment1;

namespace AfGD
{
    [ExecuteInEditMode]
    public class SplinePath_Vizi : MonoBehaviour
    {
        CurveSegment curve;
        [Tooltip("curve control points/vectors")]
        public Transform cp1, cp2, cp3, cp4;
        // we use an array of transforms as control points
        [Tooltip("Path control points")]
        public Transform[] controlPoints;
        [Tooltip("Set the curve type")]
        public CurveType curveType = CurveType.BEZIER;

        // we get the control points directly from the pathfinding object
        [Tooltip("Path holder")]
        [SerializeField] private PathFinding pathfinder;

        // we will need an array of curves for the path (instead of a single curve)
        public CurveSegment[] curves;

        //public List<CurveSegment> curves = new List<CurveSegment>();

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
            /*if (cp1 == null || cp2 == null || cp3 == null || cp4 == null)
                return false;
            curve = new CurveSegment(cp1.position, cp2.position, cp3.position, cp4.position, curveType);*/

            if (controlPoints == null)
                return false; // can't initialize

            int points = controlPoints.Length;
            if (curves == null || curves.Length != points)
                curves = new CurveSegment[points];

            // insteantiate a curve segment for each valid sequence of points
            // since we only support curves that connect the second and third points 
            // (b-spline and Catmull-Rom) we can advance our index by only one
            for (int i = 0; i < points; i++)
            {

                Vector3 cp1 = Vector3.zero, cp2 = Vector3.zero, cp3 = Vector3.zero, cp4 = Vector3.zero;
                if (i > 0 && i < points - 2)
                {
                    cp1 = controlPoints[i].position;
                    cp2 = (controlPoints[i + 1].position - controlPoints[i - 1].position).normalized;
                    cp3 = (controlPoints[i + 2].position - controlPoints[i].position).normalized;
                    cp4 = controlPoints[(i + 1) % points].position;
                }
                else if (i == 0)
                {
                    cp1 = controlPoints[i].position;
                    cp2 = (controlPoints[i + 1].position - controlPoints[i].position).normalized;
                    cp3 = (controlPoints[i + 2].position - controlPoints[i].position).normalized;
                    cp4 = controlPoints[(i + 1) % points].position;
                }
                else
                {
                    cp1 = controlPoints[points - 2].position;
                    cp2 = (controlPoints[points - 1].position - controlPoints[points - 2].position).normalized;
                    cp3 = (controlPoints[points - 1].position - controlPoints[points - 2].position).normalized;
                    cp4 = controlPoints[points - 1].position;
                }

                curves[i] = new CurveSegment(cp1, cp2, cp3, cp4, (CurveType)curveType);
            }
            return true;
        }

        public int GetCurveCount()
        {
            //return curves.Count;
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
            if (/*curves.Count > 0*/curves.Length > 0)
            {
                curveidx = (int)progress % curves.Length;
                t = progress - curveidx;
            }
            direction = curves[curveidx].EvaluateDv(t);
            return direction;
        }

        public Vector3 GetPoint(/*List<CurveSegment> curves*/CurveSegment[] curves, float progress)
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
            point = GetSinglePointOnCurveSegment(curves[curveidx], t);
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
                // reinitialize if we change somethign while not playing
                if (!Init())
                    return;
            }

            /*if(curveType == CurveType.HERMITE)
            {
                // control vectors
                Debug.DrawLine(cp1.position, cp2.position);
                Debug.DrawLine(cp4.position, cp3.position);
            }
            else
            {
                // line connecting control points
                Debug.DrawLine(cp1.position, cp2.position);
                Debug.DrawLine(cp2.position, cp3.position);
                Debug.DrawLine(cp3.position, cp4.position);
            }*/

            for (int i = 0; i < curves.Length; i++)
            {
                if (drawPath)
                    DrawCurveSegments(curves[i], pathColor, debugSegments);
                if (drawTangents)
                    DrawTangents(curves[i], tangentColor, debugSegments);
                /*if (drawCurvature)
                    DrawCurveCurvatures(curves[i], curvatureColor, debugSegments);*/
            }

            /*if (drawPath)
                DrawCurveSegments(curve, pathColor, debugSegments);
            if (drawTangents)
                DrawTangents(curve, tangentColor, debugSegments);*/

        }
    }
}