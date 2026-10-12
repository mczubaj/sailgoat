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

        Vector3 thrust =
            windDirection * windStrength * sailOpening * windEffectiveness * acceleration;

        velocity += thrust * Time.deltaTime;
        // reminder to self that this is less accurate/natural and can produce negative speeds:
        // velocity *= 1f - drag * Time.deltaTime;
        velocity *= Mathf.Exp(-drag * Time.deltaTime);
        velocity = Vector3.ClampMagnitude(velocity, maximumSpeed);

        transform.position += velocity * Time.deltaTime;
    }
}
