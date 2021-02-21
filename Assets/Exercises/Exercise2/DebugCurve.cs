using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AfGD
{
    [ExecuteInEditMode]
    public class DebugCurve : MonoBehaviour
    {

        // TODO exercise 2.3
        // you will want to have more than one CurveSegment when creating a cyclic path
        // you can consider a List<CurveSegment>. 
        // You may also want to add more control points, and "lock" the CurveType, since 
        // different curve types make curves in different ranges 
        // (e.g. Catmull-rom and B-spline make a curve from cp2 to cp3, Hermite and Bezier from cp1 to cp4)
        public List<CurveSegment> curves = new List<CurveSegment>();
        CurveSegment curve, curve2, curve3, curve4;
        // must be assigned in the inspector
        [Tooltip("curve control points/vectors")]
        public Transform cp1, cp2, cp3, cp4, cp5, cp6, cp7, cp8, cp9;
        [Tooltip("Set the curve type")]
        //public CurveType curveType = CurveType.BEZIER;
        public CurveType curveType = CurveType.BEZIER;

        // these variables are only used for visualization
        [Header("Debug varaibles")]
        [Range(2, 100)]
        public int debugSegments = 20;
        public bool drawPath = true;
        public Color pathColor = Color.magenta;
        public bool drawTangents = true;
        public Color tangentColor = Color.green;


        bool Init()
        {
            // initialize curve if all control points are valid
            if ((cp1 == null || cp2 == null || cp3 == null || cp4 == null))
                return false;
            curves.Clear();
            if (curveType == CurveType.BEZIER)
            {
                curves.Add(new CurveSegment(cp1.position, cp2.position, cp3.position, cp4.position, curveType));
                curves.Add(new CurveSegment(cp4.position, cp5.position, cp6.position, cp7.position, curveType));
                curves.Add(new CurveSegment(cp7.position, cp8.position, cp9.position, cp1.position, curveType));
                /*curves.Add(new CurveSegment(cp2.position, cp3.position, cp4.position, cp5.position, curveType));
                curves.Add(new CurveSegment(cp3.position, cp4.position, cp5.position, cp6.position, curveType));
                curves.Add(new CurveSegment(cp4.position, cp5.position, cp6.position, cp7.position, curveType));
                curves.Add(new CurveSegment(cp5.position, cp6.position, cp7.position, cp8.position, curveType));*/
            }
            else
            {
                curves.Add(new CurveSegment(cp1.position, cp2.position, cp3.position, cp4.position, curveType));
                curves.Add(new CurveSegment(cp2.position, cp3.position, cp4.position, cp1.position, curveType));
                curves.Add(new CurveSegment(cp3.position, cp4.position, cp1.position, cp2.position, curveType));
                curves.Add(new CurveSegment(cp4.position, cp1.position, cp2.position, cp3.position, curveType));
            }

            /*if (curveType == CurveType.CATMULLROM)
            {
                curve = new CurveSegment(cp1.position, cp2.position, cp3.position, cp4.position, curveType);
                curve2 = new CurveSegment(cp2.position, cp3.position, cp4.position, cp1.position, curveType);
                curve3 = new CurveSegment(cp3.position, cp4.position, cp1.position, cp2.position, curveType);
                curve4 = new CurveSegment(cp4.position, cp1.position, cp2.position, cp3.position, curveType);
                curves.Clear();
                curves.Add(curve);
                curves.Add(curve2);
                curves.Add(curve3);
                curves.Add(curve4);
            }
            if(curveType == CurveType.HERMITE)
            {
                curve = new CurveSegment(cp1.position, cp2.position, cp3.position, cp4.position, curveType);
                curve2 = new CurveSegment(cp2.position, cp3.position, cp4.position, cp1.position, curveType);
                curve3 = new CurveSegment(cp3.position, cp4.position, cp1.position, cp2.position, curveType);
                curve4 = new CurveSegment(cp4.position, cp1.position, cp2.position, cp3.position, curveType);
            }*/

            /*curve = new CurveSegment(cp1.position, cp2.position, cp3.position, cp4.position, curveType);
            curve2 = new CurveSegment(cp2.position, cp3.position, cp4.position, cp1.position, curveType);
            curve3 = new CurveSegment(cp3.position, cp4.position, cp1.position, cp2.position, curveType);
            curve4 = new CurveSegment(cp4.position, cp1.position, cp2.position, cp3.position, curveType);*/
            return true;
        }

        public int GetCurveCount()
        {
            return curves.Count;
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

        public Vector3 GetDirection(List<CurveSegment> curves, float progress)
        {
            Vector3 direction = Vector3.zero;
            int curveidx = 0;
            float t = 0;
            if (curves.Count > 0)
            {
                curveidx = (int)progress % curves.Count;
                t = progress - curveidx;
            }
            direction = curves[curveidx].EvaluateDv(t);
            return direction;
        }

        public Vector3 GetPoint(List<CurveSegment> curves, float progress)
        {
            Vector3 point = Vector3.zero;
            int curveidx = 0;
            float t = 0;
            if(curves.Count > 0)
            {
                curveidx = (int)progress % curves.Count;
                t = progress - curveidx;
                Debug.Log(curveidx);
            }
            point = GetSinglePointOnCurveSegment(curves[curveidx], t);
            return point;
        }
        /*public Vector3 GetSinglePointOnCurve(float t)
        {

        }*/
        public static void DrawCurveSegments(CurveSegment curve,
            Color color, int segments = 50)
        {
            // TODO exercise 2.2
            // evaluate the curve from start to end (range [0, 1])
            // and you draw a number of line segments between 
            // consecutive points
            float interval = 1.0f / segments;
            for (int i = 0; i < segments; i++)
            {
                float start_u = i * interval;
                float end_u = (i + 1) * interval;

                Vector3 startPoint = curve.Evaluate(start_u);
                Vector3 endPoint = curve.Evaluate(end_u);
                Debug.DrawLine(startPoint, endPoint, color);
            }     

        }

        public static void DrawTangents(CurveSegment curve,
            Color color, int segments = 50, float scale = 0.1f)
        {
            // TODO exercise 2.2
            // evaluate the curve and tangent from start to end (range [0, 1])
            // and draw the tangent as a line from the current curve point
            // to the current point + the tangent vector 
            float interval = 1.0f / segments;
            for (int i = 0; i < segments; i++)
            {
                float start_u = i * interval;

                Vector3 startPoint = curve.Evaluate(start_u);
                Vector3 startpointTanget = curve.EvaluateDv(start_u);
                Debug.DrawLine(startPoint, startPoint + startpointTanget * scale, color);
                //Debug.DrawLine(startpointTanget, endpointTanget, color);
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
                // this is here so we can update the debug draw of the curve
                // while in edit mode
                if (!Init())
                    return;
            }

            if(curveType == CurveType.HERMITE)
            {
                // Hermite spline has control vectors besides start and end points
                Debug.DrawLine(cp1.position, cp2.position);
                Debug.DrawLine(cp4.position, cp3.position);
            }
            else if (curveType == CurveType.BEZIER)
            {
                Debug.DrawLine(cp1.position, cp2.position);
                Debug.DrawLine(cp2.position, cp3.position);
                Debug.DrawLine(cp3.position, cp4.position);
                Debug.DrawLine(cp4.position, cp5.position);
                Debug.DrawLine(cp5.position, cp6.position);
                Debug.DrawLine(cp6.position, cp7.position);
                Debug.DrawLine(cp7.position, cp8.position);
                Debug.DrawLine(cp8.position, cp9.position);
                Debug.DrawLine(cp9.position, cp1.position);
            }
            else
            {
                // line connecting control points
                Debug.DrawLine(cp1.position, cp2.position);
                Debug.DrawLine(cp2.position, cp3.position);
                Debug.DrawLine(cp3.position, cp4.position);
            }

            // draw the debug shapes
            if (drawPath)
            {
                foreach (CurveSegment curve in curves)
                {
                    DrawCurveSegments(curve, pathColor, debugSegments);
                }
            }
                
            if (drawTangents)
            {
                foreach (CurveSegment curve in curves)
                {
                    DrawTangents(curve, tangentColor, debugSegments);
                }
            }
                

        }
    }
}