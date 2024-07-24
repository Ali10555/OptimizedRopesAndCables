using UnityEngine;


namespace GogoGaga.OptimizedRopesAndCables
{
    [RequireComponent(typeof(Rope))]
    public class RopeWindEffect : MonoBehaviour
    {

        [Header("Wind Settings")]
        [Tooltip("Set wind direction perpendicular to the rope based on the start and end points")]
        public bool perpendicularWind = false;
        [Tooltip("Flip the direction of the wind")]
        public bool flipWindDirection = false;

        [Tooltip("Direction of the wind force in degrees")]
        [Range(-360f, 360f)]
        public float windDirectionDegrees;
        Vector3 windDirection;

        [Tooltip("Magnitude of the wind force")]
        [Range(0f, 500f)] public float windForce;
        float appliedWindForce;
        float windSeed; //gives a little variety on the movement when there are multiple ropes


        Rope rope;


        private void Awake()
        {
            rope = GetComponent<Rope>();
        }
        void Start()
        {
            windSeed = Random.Range(-0.3f, 0.3f);
        }

        // Update is called once per frame
        void Update()
        {
            GenerateWind();
        }

        void FixedUpdate()
        {
            SimulatePhysics();
        }

        void GenerateWind()
        {

            if (perpendicularWind)
            {
                Vector3 startToEnd = rope.EndPoint.position - rope.StartPoint.position; //calculate the vector from start to end
                windDirection = Vector3.Cross(startToEnd, Vector3.up).normalized; //find the perpendicular direction

                //make some noise and calculate the wind direction in degrees
                float noise = Mathf.PerlinNoise(Time.time + windSeed, 0.0f) * 20f - 10f; //20 degrees of range for the wind direction
                float perpendicularWindDirection = Vector3.SignedAngle(Vector3.forward, windDirection, Vector3.up);
                float noisyWindDirection = perpendicularWindDirection + noise; //add the noise to the wind direction

                //convert the noisy wind direction back to a vector
                float radians = noisyWindDirection * Mathf.Deg2Rad;
                windDirection = new Vector3(Mathf.Sin(radians), 0, Mathf.Cos(radians)).normalized;

                windDirectionDegrees = perpendicularWindDirection; //set the wind direction so the user can see a change has happened and what direction the wind is set to
            }
            else
            {
                //add Perlin noise to the wind direction
                float noise = Mathf.PerlinNoise(Time.time + windSeed, 0.0f) * 20f - 10f; //20 degrees of range for the wind direction
                float noisyWindDirection = windDirectionDegrees + noise; //add the noise to the wind direction

                //convert the noisy wind direction back to a vector
                float radians = noisyWindDirection * Mathf.Deg2Rad;
                windDirection = new Vector3(Mathf.Sin(radians), 0, Mathf.Cos(radians)).normalized;
            }

            //apply perlin noise to the wind force with a check for flipped wind direction
            float windNoise = Mathf.PerlinNoise(Time.time + windSeed, 0.0f) * Mathf.PerlinNoise(0.5f * Time.time, 0.0f);
            if (flipWindDirection) appliedWindForce = ((windForce * -1) * 5f) * windNoise;
            else appliedWindForce = (windForce * 5f) * windNoise;

        }

        void SimulatePhysics()
        {
            Vector3 windEffect = windDirection.normalized * appliedWindForce * Time.fixedDeltaTime;
            rope.otherPhysicsFactors = windEffect;
        }
    }
}