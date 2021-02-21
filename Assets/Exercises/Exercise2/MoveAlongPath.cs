using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AfGD
{
    [ExecuteInEditMode]
    public class MoveAlongPath : MonoBehaviour
    {
        public DebugCurve debugCurve;

        public float duration;
        public bool lookForward;

        private float progress = 0;

        List<Vector3> pointsOnCurveSegment;
        // Start is called before the first frame update
        void Start()
        {
            /*pointsOnCurveSegment = debugCurve.GetPointsOnCurveSegment(debugCurve.curves[0], debugCurve.debugSegments);
            transform.localPosition = pointsOnCurveSegment[0];*/
        }

        // Update is called once per frame
        void Update()
        {
            progress += Time.deltaTime / duration;
            if (progress > debugCurve.GetCurveCount())
            {
                progress = 0f;
            }
            //transform.localPosition = debugCurve.GetSinglePointOnCurveSegment(debugCurve.curves[0], progress);
            Vector3 position = debugCurve.GetPoint(debugCurve.curves, progress);
            transform.localPosition = position;
            if (lookForward)
            {
                Vector3 direction = debugCurve.GetDirection(debugCurve.curves, progress);
                transform.LookAt(position + direction);
            }
        }
    }
}
