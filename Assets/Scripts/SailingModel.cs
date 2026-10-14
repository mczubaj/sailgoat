using UnityEngine;

namespace Sailgoat
{
    public sealed class SailingModel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private ShipController shipController;

        [Header("Wind")]
        [SerializeField, Min(0f)]
        private float windStrength = 1f;

        [SerializeField, Range(0f, 360f)]
        private float windDirectionDegrees = 0f;

        public float WindDirectionDegrees => windDirectionDegrees;

        [Header("Movement")]
        [SerializeField, Min(0f)]
        private float acceleration = 4f;

        [SerializeField, Min(0f)]
        private float drag = 0.5f;

        [SerializeField, Min(0f)]
        private float maximumSpeed = 10f;

        [Header("Steering")]
        [SerializeField, Min(0f)]
        private float turnSensitivity = 15f;

        private Vector3 velocity;

        [Header("Telemetry (Debug)")]
        [SerializeField]
        private float currentSpeed;

        [SerializeField]
        private float currentThrust;

        [SerializeField]
        private float currentTurnRate;

        [SerializeField]
        private float apparentWindSpeedDebug;

        private void Update()
        {
            if (shipController == null)
            {
                return;
            }

            // 1. Apparent Wind calculation:
            // True environmental wind:
            Vector3 trueWind =
                Quaternion.Euler(0f, windDirectionDegrees, 0f) * Vector3.forward * windStrength;

            // Apparent wind experienced on the moving boat (True Wind - Boat Velocity):
            Vector3 apparentWind = trueWind - velocity;
            float apparentWindSpeed = apparentWind.magnitude;
            Vector3 apparentWindDir = apparentWind.normalized;

            float sailOpening = shipController.SailOpening / 100f;
            float sailAngle = shipController.SailRotation;

            Vector3 sailDirection = Quaternion.Euler(0f, sailAngle, 0f) * transform.forward;

            // The sail catches wind relative to the apparent wind direction:
            float windAlignment = Vector3.Dot(sailDirection, apparentWindDir);

            float windEffectiveness = Mathf.Max(0f, windAlignment);

            // Total aerodynamic force magnitude caught by the sail scales with apparent wind speed:
            float totalSailForce = apparentWindSpeed * sailOpening * windEffectiveness * acceleration;

            // The force acts normal to the sail surface.
            Vector3 sailForceVector = sailDirection * totalSailForce;

            // 1. Steering & Rotation (Yaw):
            // Lateral component pushes sideways on the mast, creating a turning moment.
            float lateralForce = Vector3.Dot(sailForceVector, transform.right);

            // Steerageway: ships turn better with water flowing past the hull (speed).
            // A small baseline (0.15) allows gentle turning even at low speeds.
            float normalizedSpeed = Mathf.Clamp01(velocity.magnitude / maximumSpeed);
            float steerageFactor = Mathf.Lerp(0.15f, 1f, normalizedSpeed);

            currentTurnRate = lateralForce * turnSensitivity * steerageFactor;
            transform.Rotate(0f, currentTurnRate * Time.deltaTime, 0f);

            // The keel forces existing momentum to follow the new hull heading.
            velocity = transform.forward * velocity.magnitude;

            // 2. Forward Propulsion (Acceleration):
            // Forward component pushes the hull forward along the newly oriented heading.
            float forwardThrust = Mathf.Max(0f, Vector3.Dot(sailForceVector, transform.forward));
            Vector3 thrust = transform.forward * forwardThrust;

            velocity += thrust * Time.deltaTime;
            velocity *= Mathf.Exp(-drag * Time.deltaTime);
            velocity = Vector3.ClampMagnitude(velocity, maximumSpeed);

            currentThrust = forwardThrust;
            currentSpeed = velocity.magnitude;
            apparentWindSpeedDebug = apparentWindSpeed;

            transform.position += velocity * Time.deltaTime;
        }
    }
}
