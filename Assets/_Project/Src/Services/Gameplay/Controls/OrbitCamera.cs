using UnityEngine;

namespace Services.Gameplay.Controls
{
    public class OrbitCamera : MonoBehaviour
    {
        [SerializeField] private Transform gunContainer;
        [SerializeField] private Transform target;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float distance = 10f;
        [SerializeField] private float mult = 50f;

        private float _currentAngle = 0f;
        private float _gunRotationX = 0f;

        [Header("UP/DOWN rotation")] [SerializeField]
        private float MAX_GUN_ANGLE = 10f;

        [SerializeField] private float MIN_GUN_ANGLE = -40f;

        private void Awake()
        {
            distance = Vector3.Distance(target.transform.position, transform.position);
        }

        private void Start()
        {
            if (!target)
            {
                Debug.LogError("No Target Selected");
                return;
            }

            if (!gunContainer)
            {
                Debug.LogError("No Gun Container Selected");
                return;
            }

            UpdateCameraPosition();
        }

        private void Update()
        {
            if (!target || !gunContainer) return;

            if (Input.GetKey(KeyCode.D))
            {
                _currentAngle += rotationSpeed * Time.deltaTime * mult;
            }

            if (Input.GetKey(KeyCode.A))
            {
                _currentAngle -= rotationSpeed * Time.deltaTime * mult;
            }

            if (Input.GetKey(KeyCode.W))
            {
                _gunRotationX -= rotationSpeed * Time.deltaTime * mult;
            }

            if (Input.GetKey(KeyCode.S))
            {
                _gunRotationX += rotationSpeed * Time.deltaTime * mult;
            }


            _gunRotationX = Mathf.Clamp(_gunRotationX, MIN_GUN_ANGLE, MAX_GUN_ANGLE);


            gunContainer.localRotation = Quaternion.Euler(_gunRotationX, 0f, 0f);

            UpdateCameraPosition();
        }

        private void UpdateCameraPosition()
        {
            var radianAngle = _currentAngle * Mathf.Deg2Rad;

            var x = Mathf.Sin(radianAngle) * distance;
            var z = Mathf.Cos(radianAngle) * distance;

            transform.position = new Vector3(
                target.position.x + x,
                target.position.y,
                target.position.z + z
            );

            transform.LookAt(target);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}