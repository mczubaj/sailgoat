using UnityEngine;

namespace Sailgoat
{
    public sealed class SailingModel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private ShipController shipController;

        [Header("Wind")]
        [Tooltip("Speed/strength of the environmental wind.")]
        [SerializeField, Min(0f)]
        private float windStrength = 1f;

        [Tooltip(
            "Direction the true environmental wind is blowing towards (0 = North, 90 = East)."
        )]
        [SerializeField, Range(0f, 360f)]
        private float windDirectionDegrees = 0f;

        public float WindDirectionDegrees => windDirectionDegrees;

        [Header("Movement")]
        [Tooltip("Linear acceleration multiplier applied to usable sail thrust.")]
        [SerializeField, Min(0f)]
        private float acceleration = 4f;

        [Tooltip("Hydrodynamic water resistance limiting top speed.")]
        [SerializeField, Min(0f)]
        private float drag = 0.5f;

        [Tooltip("Absolute maximum speed clamp.")]
        [SerializeField, Min(0f)]
        private float maximumSpeed = 10f;

        [Tooltip(
            "How aggressively the keel resists lateral sliding and aligns momentum with the bow."
        )]
        [SerializeField, Min(0f)]
        private float keelGrip = 5f;

        [Header("Steering")]
        [Tooltip("Sensitivity multiplier converting lateral sail force into turning torque.")]
        [SerializeField, Min(0f)]
        private float turnSensitivity = 15f;

        [Tooltip("Rotational water resistance damping angular velocity.")]
        [SerializeField, Min(0f)]
        private float angularDrag = 2f;

        // Physics State
        private Vector3 velocity;
        private float angularVelocity;

        [Header("Telemetry (Live Debug)")]
        [Tooltip("Current speed across the water.")]
        [SerializeField]
        private float currentSpeed;

        [Tooltip("Current forward thrust produced by the sail.")]
        [SerializeField]
        private float currentThrust;

        [Tooltip("Current angular velocity (turning rate in deg/s).")]
        [SerializeField]
        private float currentTurnRate;

        [Tooltip("Speed of the apparent wind currently felt by the sail.")]
        [SerializeField]
        private float currentApparentWind;

        private void Update()
        {
            if (shipController == null)
            {
                return;
            }

            float speed = velocity.magnitude;

            // =========================================================================
            // Phase 1: Apparent Wind Calculation
            // =========================================================================
            Vector3 trueWind =
                Quaternion.Euler(0f, windDirectionDegrees, 0f) * Vector3.forward * windStrength;
            Vector3 apparentWind = trueWind - velocity;
            float apparentWindSpeed = apparentWind.magnitude;
            Vector3 apparentWindDirection = apparentWind.normalized;

            // =========================================================================
            // Phase 2: Sail Aerodynamics (Square Rig Normal Pressure Model)
            // =========================================================================
            float normalizedSailOpening = shipController.SailOpening / 100f;
            float sailAngle = shipController.SailRotation;

            // The normal vector of the square-rigged sail:
            Vector3 sailDirection = Quaternion.Euler(0f, sailAngle, 0f) * transform.forward;

            // Alignment: How directly the apparent wind hits the face of the sail
            float windAlignment = Vector3.Dot(sailDirection, apparentWindDirection);
            float windEffectiveness = Mathf.Max(0f, windAlignment);

            // Aerodynamic force vector acting perpendicular to the canvas:
            float totalSailForce =
                apparentWindSpeed * normalizedSailOpening * windEffectiveness * acceleration;
            Vector3 sailForceVector = sailDirection * totalSailForce;

            // =========================================================================
            // Phase 3: Steering, Angular Dynamics & Keel Alignment
            // =========================================================================
            // Lateral component creates turning torque around the vertical Y-axis:
            float lateralForce = Vector3.Dot(sailForceVector, transform.right);

            // Steerageway: turning authority scales with speed, retaining a small baseline:
            float normalizedSpeed = Mathf.Clamp01(speed / maximumSpeed);
            float steerageFactor = Mathf.Lerp(0.15f, 1f, normalizedSpeed);

            // Torque accelerates angular velocity, damped by angular water drag:
            float turnTorque = lateralForce * turnSensitivity * steerageFactor;
            angularVelocity += turnTorque * Time.deltaTime;
            angularVelocity *= Mathf.Exp(-angularDrag * Time.deltaTime);

            transform.Rotate(0f, angularVelocity * Time.deltaTime, 0f);

            // Anisotropic Keel: Damps lateral slide, smoothly settling momentum along the bow:
            Vector3 targetVelocity = transform.forward * speed;
            velocity = Vector3.Lerp(
                velocity,
                targetVelocity,
                1f - Mathf.Exp(-keelGrip * Time.deltaTime)
            );

            // =========================================================================
            // Phase 4: Forward Propulsion & Movement Integration
            // =========================================================================
            // Keel absorbs lateral force; only the forward component accelerates the boat:
            float forwardThrust = Mathf.Max(0f, Vector3.Dot(sailForceVector, transform.forward));
            Vector3 thrust = transform.forward * forwardThrust;

            velocity += thrust * Time.deltaTime;
            velocity *= Mathf.Exp(-drag * Time.deltaTime);
            velocity = Vector3.ClampMagnitude(velocity, maximumSpeed);

            transform.position += velocity * Time.deltaTime;

            // =========================================================================
            // Phase 5: Telemetry Update
            // =========================================================================
            currentSpeed = velocity.magnitude;
            currentThrust = forwardThrust;
            currentTurnRate = angularVelocity;
            currentApparentWind = apparentWindSpeed;
        }
    }
}

