using UnityEngine;

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

    private Vector3 velocity;

    private void Update()
    {
        if (shipController == null)
        {
            return;
        }

        Vector3 windDirection = Quaternion.Euler(0f, windDirectionDegrees, 0f) * Vector3.forward;

        float sailOpening = shipController.SailOpening / 100f;
        float sailAngle = shipController.SailRotation;

        Vector3 sailDirection = Quaternion.Euler(0f, sailAngle, 0f) * transform.forward;

        float windAlignment = Vector3.Dot(sailDirection, windDirection);

        float windEffectiveness = Mathf.Max(0f, windAlignment);

        // Total aerodynamic force magnitude caught by the sail:
        float totalSailForce =
            windStrength * sailOpening * windEffectiveness * acceleration;

        // The force acts normal to the sail surface:
        Vector3 sailForceVector = sailDirection * totalSailForce;

        // Decompose force: only the forward component pushes the hull forward.
        // The lateral component (Dot with transform.right) is absorbed/wasted by keel drag.
        float forwardThrust = Mathf.Max(0f, Vector3.Dot(sailForceVector, transform.forward));

        Vector3 thrust = transform.forward * forwardThrust;

        velocity += thrust * Time.deltaTime;
        velocity *= Mathf.Exp(-drag * Time.deltaTime);
        velocity = Vector3.ClampMagnitude(velocity, maximumSpeed);

        transform.position += velocity * Time.deltaTime;
    }
}
