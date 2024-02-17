using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class Rope : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;

    [Tooltip("This will move at the center, if you want to attach stuff")]
    public Transform midPoint;

    [Range(2,100)]public int linePoints = 10;
    public float stiffness = 1f; // value highly dependent on use case
    public float damping = 0.1f; // 0 is no damping, 1 is a lot, I think
    public float ropeLenght = 15;


    float currentValue;
    float currentVelocity;
    float targetValue;
    float valueThreshold = 0.01f;
    float velocityThreshold = 0.01f;


    LineRenderer lineRenderer;
    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        currentValue = getMidPoint().y;
    }
    private void Update()
    {
        SetSplinePoint();
    }

    void SetSplinePoint()
    {
        if (lineRenderer.positionCount != linePoints+1)
            lineRenderer.positionCount = linePoints+1;

        Vector3 mid = getMidPoint();
        targetValue = mid.y;
        mid.y = currentValue;

        if (midPoint != null)
            midPoint.position = getBezierPoint(startPoint.position, mid, endPoint.position, 0.5f);

        for (int i = 0; i < linePoints; i++)
        {
            Vector3 p = getBezierPoint(startPoint.position, mid, endPoint.position, i / (float)linePoints);
            lineRenderer.SetPosition(i, p);
        }

        lineRenderer.SetPosition(linePoints, endPoint.position);
    }
    
    Vector3 getMidPoint()
    {
        Vector3 midpos = Vector3.Lerp(startPoint.position, endPoint.position, .5f);
        float yFactor = ropeLenght - Mathf.Min(Vector3.Distance(startPoint.position, endPoint.position),ropeLenght);
        midpos.y -= yFactor;
        return midpos;
    }

    Vector3 getBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        Vector3 a = Vector3.Lerp(p0, p1, t);

        Vector3 b = Vector3.Lerp(p1, p2, t);

        Vector3 point = Vector3.Lerp(a, b, t);

        return point;
    }
    

    void FixedUpdate()
    {
        SimulatePhysics();
    }
    

    void SimulatePhysics()
    {
        float dampingFactor = Mathf.Max(0, 1 - damping * Time.fixedDeltaTime);
        float acceleration = (targetValue - currentValue) * stiffness * Time.fixedDeltaTime;
        currentVelocity = currentVelocity * dampingFactor + acceleration;
        currentValue += currentVelocity * Time.fixedDeltaTime;
        
        if (Mathf.Abs(currentValue - targetValue) < valueThreshold && Mathf.Abs(currentVelocity) < velocityThreshold)
        {
            currentValue = targetValue;
            currentVelocity = 0f;
        }
    }

    private void OnDrawGizmos()
    {
        if (endPoint == null || startPoint == null)
            return;
        Vector3 midPos = getMidPoint();

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(midPos, 0.2f);
    }
}