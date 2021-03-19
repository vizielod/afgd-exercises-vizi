using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AfGD
{
    // exercise 2.4
    // attach to an object so that it moves along a path
    public class MoveAlongPath : MonoBehaviour
    {
        [Tooltip("Path to move along")]
        public SplinePath splinePath;

        [Tooltip("How long should a full lap take?")]
        [Range(2, 60)]
        public float lapTime = 5.0f;

        [Tooltip("Should the object move?")]
        public bool stop = false;
        [Tooltip("Should the object look into the moving direction?")]
        public bool lookForward = false;

        public bool parameterizeByArclength = true;
        public bool useEasingCurve = true;

        // we keep an internal clock for this object for higher flexibility
        private float localTime = 0;
        private float progress = 0;

        // Update is called once per frame
        void Update()
        {
            if (splinePath == null || stop)
                return;

            localTime += Time.deltaTime;
            // one cycle every lapTime (modulo)
            localTime = localTime % lapTime;

            // time is normalimalized so [0, lapTime] -> [0,1]
            float t = localTime / lapTime;

            float s = t;
            // part of exercise 2.5, easing curve to control normalized time
            if (useEasingCurve)
                s = EasingFunctions.Crossfade(EasingFunctions.SmoothStart2, EasingFunctions.SmoothStop2, 0.5f, t);

            progress += Time.deltaTime / lapTime;
            if (progress > splinePath.GetCurveCount())
            {
                progress = 0f;
            }
            // exercise 2.4, move this object along the line            
            Vector3 pos = splinePath.Evaluate(s, parameterizeByArclength);
            this.transform.position = pos;

            if (lookForward)
            {
                Vector3 direction = splinePath.Evaluate(s, parameterizeByArclength, 1);
                transform.LookAt(this.transform.position + direction);
            }

        }

        //public SplinePath splinePath;

        /*public float duration;
        public bool lookForward;

        private float progress = 0;

        List<Vector3> pointsOnCurveSegment;*/

        // Start is called before the first frame update
        void Start()
        {
            /*pointsOnCurveSegment = debugCurve.GetPointsOnCurveSegment(debugCurve.curves[0], debugCurve.debugSegments);
            transform.localPosition = pointsOnCurveSegment[0];*/
        }

        // Update is called once per frame
        /*void Update()
        {
            if (splinePath.pathfinder.isPathConstructed)
            {
                progress += Time.deltaTime / duration;
                if (progress > splinePath.GetCurveCount())
                {
                    progress = 0f;
                }
                Vector3 position = splinePath.GetPoint(splinePath.curves, progress);
                transform.localPosition = position;
                if (lookForward)
                {
                    Vector3 direction = splinePath.GetDirection(splinePath.curves, progress);
                    transform.LookAt(position + direction);
                }
            }
        }*/
    }
}