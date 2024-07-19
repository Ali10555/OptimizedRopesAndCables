using UnityEngine;

namespace GogoGaga.OptimizedRopesAndCables
{

    [RequireComponent(typeof(LineRenderer))]
    public class Rope : MonoBehaviour
    {
        [Header("Rope Transforms")]
        [Tooltip("The rope will start at this point")]
        public Transform startPoint;
        [Tooltip("This will move at the center hanging from the rope, like a necklace, for example")]
        public Transform midPoint;
        [Tooltip("The rope will end at this point")]
        public Transform endPoint;

        [Header("Rope Settings")]
        [Tooltip("How many points should the rope have, 2 would be a triangle with straight lines, 100 would be a very flexible rope with many parts")]
        [Range(2, 100)] public int linePoints = 10;
        [Tooltip("Value highly dependent on use case, a metal cable would have high stiffness, a rubber rope would have a low one")]
        public float stiffness = 350f;
        [Tooltip("0 is no damping, 50 is a lot")]
        public float damping = 15f;
        [Tooltip("How long is the rope, it will hang more or less from starting point to end point depending on this value")]
        public float ropeLength = 15;
        [Tooltip("The Rope width set at start (changing this value during run time will produce no effect)")]
        public float ropeWidth = 0.1f;

        [Header("Rational Bezier Weight Control")]
        [Tooltip("Adjust the middle control point weight for the Rational Bezier curve")]
        [Range(1, 15)] public float midPointWeight = 1f;
        float startPointWeight = 1f; //these need to stay at 1, could be removed but makes calling the rational bezier function easier to read and understand
        float endPointWeight = 1f;

        [Header("Midpoint Position")]
        [Tooltip("Position of the midpoint along the line between start and end points")]
        [Range(0.25f, 0.75f)] public float midPointPosition = 0.5f; //undesired line behaviour and midpoint position outside this safe range, there's probably a better way to do this



        private Vector3 currentValue;
        private Vector3 currentVelocity;
        private Vector3 targetValue;
        public Vector3 otherPhysicsFactors { get; set; }
        float valueThreshold = 0.01f;
        float velocityThreshold = 0.01f;

        LineRenderer lineRenderer;

        private void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.startWidth = ropeWidth;
            lineRenderer.endWidth = ropeWidth;
            currentValue = GetMidPoint();
        }

        private void OnValidate()
        {
            if(!lineRenderer)
                lineRenderer = GetComponent<LineRenderer>();
            
            lineRenderer.startWidth = ropeWidth;
            lineRenderer.endWidth = ropeWidth;
        }


        private void Update()
        {
            if (!startPoint || !endPoint)
                return;

            SetSplinePoint();
        }

        void SetSplinePoint()
        {
            if (lineRenderer.positionCount != linePoints + 1)
                lineRenderer.positionCount = linePoints + 1;

            Vector3 mid = GetMidPoint();
            targetValue = mid;
            mid = currentValue;

            if (midPoint != null)
                midPoint.position = GetRationalBezierPoint(startPoint.position, mid, endPoint.position, midPointPosition, startPointWeight, midPointWeight, endPointWeight);

            for (int i = 0; i < linePoints; i++)
            {
                Vector3 p = GetRationalBezierPoint(startPoint.position, mid, endPoint.position, i / (float)linePoints, startPointWeight, midPointWeight, endPointWeight);
                lineRenderer.SetPosition(i, p);
            }

            lineRenderer.SetPosition(linePoints, endPoint.position);
        }

        float CalculateYFactorAdjustment(float weight)
        {
            //float k = 0.360f; //after testing this seemed to be a good value for most cases, more accurate k is available.
            float k = Mathf.Lerp(0.493f, 0.323f, (Mathf.InverseLerp(1, 15, weight))); //K calculation that is more accurate, interpolates between precalculated values.
            float w = 1f + k * Mathf.Log(weight);

            return w;
        }

        Vector3 GetMidPoint()
        {
            var (startPointPosition, endPointPosition) = (startPoint.position, endPoint.position);
            Vector3 midpos = Vector3.Lerp(startPointPosition, endPointPosition, midPointPosition);
            float yFactor = (ropeLength - Mathf.Min(Vector3.Distance(startPointPosition, endPointPosition), ropeLength)) / CalculateYFactorAdjustment(midPointWeight);
            midpos.y -= yFactor;
            return midpos;
        }

        Vector3 GetRationalBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t, float w0, float w1, float w2)
        {
            //scale each point by it's weight (can probably remove w0 and w2 if the midpoint is the only adjustable weight)
            Vector3 wp0 = w0 * p0;
            Vector3 wp1 = w1 * p1;
            Vector3 wp2 = w2 * p2;

            //calculate the denominator of the rational bezier curve
            float denominator = w0 * Mathf.Pow(1 - t, 2) + 2 * w1 * (1 - t) * t + w2 * Mathf.Pow(t, 2);
            //calculate the numerator and devide by the demoninator to get the point on the curve
            Vector3 point = (wp0 * Mathf.Pow(1 - t, 2) + wp1 * 2 * (1 - t) * t + wp2 * Mathf.Pow(t, 2)) / denominator;

            return point;
        }

        public Vector3 GetPointAt(float t)
        {
            return GetRationalBezierPoint(startPoint.position, currentValue, endPoint.position, t, startPointWeight, midPointWeight, endPointWeight);
        }

        void FixedUpdate()
        {
            if (!startPoint || !endPoint)
                return;

            SimulatePhysics();
        }

        void SimulatePhysics()
        {
            float dampingFactor = Mathf.Max(0, 1 - damping * Time.fixedDeltaTime);
            Vector3 acceleration = (targetValue - currentValue) * stiffness * Time.fixedDeltaTime;
            currentVelocity = currentVelocity * dampingFactor + acceleration + otherPhysicsFactors;
            currentValue += currentVelocity * Time.fixedDeltaTime;

            if (Vector3.Distance(currentValue, targetValue) < valueThreshold && currentVelocity.magnitude < velocityThreshold)
            {
                currentValue = targetValue;
                currentVelocity = Vector3.zero;
            }
        }

        

        private void OnDrawGizmos()
        {
            if (endPoint == null || startPoint == null)
                return;
            Vector3 midPos = GetMidPoint();

           // Gizmos.color = Color.red;
           // Gizmos.DrawSphere(midPos, 0.2f);
        }
    }
}