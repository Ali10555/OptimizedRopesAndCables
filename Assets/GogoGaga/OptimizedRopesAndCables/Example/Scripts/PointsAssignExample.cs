using UnityEngine;

namespace GogoGaga.OptimizedRopesAndCables
{
    public class PointsAssignExample : MonoBehaviour
    {
        [SerializeField] private Transform point1;
        [SerializeField] private Transform point2;
        [SerializeField] private Rope rope;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                //Assigning new end point to the rope with animation
                rope.SetEndPoint(point1);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                //Assigning new end point to the rope with animation
                rope.SetEndPoint(point2);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                //Assigning new end point to the rope without animation
                //The rope will be recalculated immediately
                rope.SetEndPoint(point1, true);
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                //Assigning new end point to the rope without animation
                //The rope will be recalculated immediately
                rope.SetEndPoint(point2, true);
            }

            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                //Removing end point from the rope, the rope will be
                //recalculated, LineRenderer will be cleared
                rope.SetEndPoint(null);
            }
        }
    }
}
