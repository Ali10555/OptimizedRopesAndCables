using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GogoGaga.OptimizedRopesAndCables
{
    public class CameraMove : MonoBehaviour
    {
        public float speed = 15;
        public Transform[] cameraPoses;
        int current = 0;
        void Start()
        {

        }



        void Update()
        {
            transform.position = Vector3.Lerp(transform.position, cameraPoses[current].position, Time.deltaTime * speed);
            transform.rotation = Quaternion.Lerp(transform.rotation, cameraPoses[current].rotation, Time.deltaTime * speed);

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Q))
            {
                current--;
                if (current == -1)
                    current = cameraPoses.Length -1;
            }
            else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.E))
            {
                current++;
                if (current == cameraPoses.Length)
                    current = 0;
            }

            current = Mathf.Clamp(current, 0, cameraPoses.Length - 1);
        }
    }
}