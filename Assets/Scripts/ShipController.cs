using System;
using UnityEngine;

public sealed class ShipController : MonoBehaviour
{
    private const float MinimumSailOpening = 0f;
    private const float MaximumSailOpening = 100f;
    private const float MinimumSailRotation = -45f;
    private const float MaximumSailRotation = 45f;

    [SerializeField]
    private Transform sailAnchor;

    [SerializeField, Range(0.01f, 0.3f)]
    private float closedSailFraction = 0.08f;

    [SerializeField, Range(0f, 100f)]
    private float sailOpening;

    [SerializeField, Range(-45f, 45f)]
    private float sailRotation;

    public float SailOpening => sailOpening;
    public float SailRotation => sailRotation;

    public event Action StateChanged;

    private void Awake()
    {
        sailOpening = Mathf.Clamp(sailOpening, MinimumSailOpening, MaximumSailOpening);
        sailRotation = Mathf.Clamp(sailRotation, MinimumSailRotation, MaximumSailRotation);

        ApplySailRotation();
        ApplySailOpening();
    }

    public void ChangeSailOpening(float amount)
    {
        float newOpening = Mathf.Clamp(
            sailOpening + amount,
            MinimumSailOpening,
            MaximumSailOpening
        );

        if (Mathf.Approximately(newOpening, sailOpening))
        {
            return;
        }

        sailOpening = newOpening;
        ApplySailOpening();
        StateChanged?.Invoke();
    }

    public void ChangeSailRotation(float amount)
    {
        float newRotation = Mathf.Clamp(
            sailRotation + amount,
            MinimumSailRotation,
            MaximumSailRotation
        );

        if (Mathf.Approximately(newRotation, sailRotation))
        {
            return;
        }

        sailRotation = newRotation;
        ApplySailRotation();
        StateChanged?.Invoke();
    }

    private void ApplySailRotation()
    {
        if (sailAnchor == null)
        {
            return;
        }

        sailAnchor.localRotation = Quaternion.Euler(0f, sailRotation, 0f);
    }

    private void ApplySailOpening()
    {
        if (sailAnchor == null)
        {
            return;
        }

        float normalizedOpening = sailOpening / MaximumSailOpening;
        Vector3 scale = sailAnchor.localScale;
        scale.y = Mathf.Max(closedSailFraction, normalizedOpening);
        sailAnchor.localScale = scale;
    }
}
