using Reflex.Attributes;
using Services.Gameplay.GameProcessManagement;
using UnityEngine;

namespace Services.Gameplay.Controls
{
    public class OrbitCamera : MonoBehaviour
    {
        [SerializeField] private Transform horizontal;
        [SerializeField] private Transform vertical;

        [SerializeField] private Transform gunContainer;
        [SerializeField] private Transform target;
        [SerializeField] private float horizontalRotationSpeed = 1f;
        [SerializeField] private float distance = 10f;
        [SerializeField] private float mult = 50f;

        private float _currentAngle = 0f;
        private float _gunRotationX = 0f;

        [Header("UP/DOWN rotation")] [SerializeField]
        private float MAX_GUN_ANGLE = 10f;

        [SerializeField] private float MIN_GUN_ANGLE = -40f;
        [SerializeField] private float verticalRotationSpeed = 0.2f;

        private GameProcessManager _gameProcessManager;

        [Inject]
        public void Inject(GameProcessManager gameProcessManager)
        {
            _gameProcessManager = gameProcessManager;
        }

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

            if (_gameProcessManager.currentState.Value == GameState.Calm)
                return;
            
            // Horizontal
            if (Input.GetKey(KeyCode.A)) // По часовой стрелке
            {
                Debug.LogWarning("pressed");
                _currentAngle -= horizontalRotationSpeed * Time.deltaTime * mult;
                horizontal.Rotate(0f, -horizontalRotationSpeed * Time.deltaTime * mult * 10, 0f, Space.Self);
            }

            if (Input.GetKey(KeyCode.D)) // Против часовой стрелки
            {
                _currentAngle += horizontalRotationSpeed * Time.deltaTime * mult;
                horizontal.Rotate(0f, horizontalRotationSpeed * Time.deltaTime * mult * 10, 0f, Space.Self);
            }

            // Vertical
            if (Input.GetKey(KeyCode.W) && _gunRotationX > MIN_GUN_ANGLE) // Вверх
            {
                _gunRotationX -= verticalRotationSpeed * Time.deltaTime * mult;
                vertical.Rotate(0f, -verticalRotationSpeed * Time.deltaTime * mult * 10, 0f, Space.Self);
            }

            if (Input.GetKey(KeyCode.S) && _gunRotationX < MAX_GUN_ANGLE) // Вниз
            {
                _gunRotationX += verticalRotationSpeed * Time.deltaTime * mult;
                vertical.Rotate(0f, verticalRotationSpeed * Time.deltaTime * mult * 10, 0f, Space.Self);
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