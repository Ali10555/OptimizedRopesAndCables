using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Rope : MonoBehaviour {
    [Header("Rope Transforms")]
    [Tooltip("The rope will start at this point")]
    public Transform startPoint;
    [Tooltip("The rope will end at this point")]
    public Transform endPoint;
    [Tooltip("This will move at the center hanging from the rope, like a necklace, for example")]
    public Transform midPoint;
    [Header("Rope Settings")]
    [Tooltip("How many points should the rope have, 2 would be a triangle with straight lines, 100 would be a very flexible rope with many parts")]
    [Range(2, 100)] public int linePoints = 10;
    [Tooltip("Value highly dependent on use case, a metal cable would have high stiffness, a rubber rope would have a low one")]
    public float stiffness = 1f;
    [Tooltip("0 is no damping, 50 is a lot")]
    public float damping = 15f;
    [Tooltip("How long is the rope, it will hang more or less from starting point to end point depending on this value")]
    public float ropeLength = 15;
    [Tooltip("The Rope width set at start (changing this value during run time will produce no effect)")]
    public float ropeWidth = 1;

    Vector3 currentValue;
    Vector3 currentVelocity;
    Vector3 targetValue;
    float valueThreshold = 0.01f;
    float velocityThreshold = 0.01f;

    LineRenderer lineRenderer;

    private void Start () {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = ropeWidth;
        lineRenderer.endWidth = ropeWidth;
        currentValue = GetMidPoint();
    }

    private void Update () {
        SetSplinePoint();
    }

    void SetSplinePoint () {
        if (lineRenderer.positionCount != linePoints + 1)
            lineRenderer.positionCount = linePoints + 1;

        Vector3 mid = GetMidPoint();
        targetValue = mid;
        mid = currentValue;

        if (midPoint != null)
            midPoint.position = GetBezierPoint(startPoint.position, mid, endPoint.position, 0.5f);

        for (int i = 0; i < linePoints; i++) {
            Vector3 p = GetBezierPoint(startPoint.position, mid, endPoint.position, i / (float)linePoints);
            lineRenderer.SetPosition(i, p);
        }

        lineRenderer.SetPosition(linePoints, endPoint.position);
    }

    Vector3 GetMidPoint () {
        var (startPointPosition, endPointPosition) = (startPoint.position, endPoint.position);
        Vector3 midpos = Vector3.Lerp(startPointPosition, endPointPosition, .5f);
        float yFactor = ropeLength - Mathf.Min(Vector3.Distance(startPointPosition, endPointPosition), ropeLength);
        midpos.y -= yFactor;
        return midpos;
    }

    Vector3 GetBezierPoint (Vector3 p0, Vector3 p1, Vector3 p2, float t) {
        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        Vector3 point = Vector3.Lerp(a, b, t);
        return point;
    }

    void FixedUpdate () {
        SimulatePhysics();
    }

    void SimulatePhysics () {
        float dampingFactor = Mathf.Max(0, 1 - damping * Time.fixedDeltaTime);
        Vector3 acceleration = (targetValue - currentValue) * stiffness * Time.fixedDeltaTime;
        currentVelocity = currentVelocity * dampingFactor + acceleration;
        currentValue += currentVelocity * Time.fixedDeltaTime;

        if (Vector3.Distance(currentValue, targetValue) < valueThreshold && currentVelocity.magnitude < velocityThreshold) {
            currentValue = targetValue;
            currentVelocity = Vector3.zero;
        }
    }

    private void OnDrawGizmos () {
        if (endPoint == null || startPoint == null)
            return;
        Vector3 midPos = GetMidPoint();

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(midPos, 0.2f);
    }
}
