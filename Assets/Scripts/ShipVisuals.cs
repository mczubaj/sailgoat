using System.Collections.Generic;
using UnityEngine;

namespace Sailgoat
{
    /// <summary>
    /// VIBECODED! Just for more fun and clarity when testing
    /// Handles physical visual feedback for the ship:
    /// - Leeward heeling (rolling away from the wind based on lateral sail force).
    /// - Bow rise and pitch response to forward thrust.
    /// - Idle oceanic swell and bobbing (buoyancy rocking).
    /// - Dual wake trails at the stern quarters showing path, turning arc, and speed.
    /// </summary>
    public sealed class ShipVisuals : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private SailingModel sailingModel;

        [Tooltip(
            "The container holding visual meshes (Hull, Mast, etc.). If null, a 'Visuals' container is automatically created/resolved."
        )]
        [SerializeField]
        private Transform visualRoot;

        [Header("Heel / Roll Dynamics")]
        [Tooltip("Degrees of roll heel per unit of lateral sail force.")]
        [SerializeField, Min(0f)]
        private float heelSensitivity = 3.5f;

        [Tooltip("Maximum roll heel angle in degrees.")]
        [SerializeField, Range(0f, 45f)]
        private float maxHeelAngle = 22f;

        [Tooltip("Smoothing speed for settling into and recovering from heel.")]
        [SerializeField, Min(0.1f)]
        private float heelSmoothSpeed = 5f;

        [Tooltip("Degrees of roll bank per deg/s of turning angular velocity.")]
        [SerializeField]
        private float turnRollSensitivity = 0.1f;

        [Header("Pitch / Bow Dynamics")]
        [Tooltip("Degrees of bow rise per unit of forward thrust.")]
        [SerializeField, Min(0f)]
        private float pitchSensitivity = 0.6f;

        [Tooltip("Maximum pitch angle in degrees.")]
        [SerializeField, Range(0f, 20f)]
        private float maxPitchAngle = 6f;

        [Tooltip("Smoothing speed for pitch transitions.")]
        [SerializeField, Min(0.1f)]
        private float pitchSmoothSpeed = 4f;

        [Header("Ocean Swell & Buoyancy Bobbing")]
        [SerializeField]
        private bool enableBobbing = true;

        [Tooltip("Base frequency of ocean water rocking.")]
        [SerializeField, Min(0f)]
        private float bobFrequency = 1.2f;

        [Tooltip("Vertical bobbing amplitude in meters.")]
        [SerializeField, Min(0f)]
        private float bobHeight = 0.035f;

        [Tooltip("Idle roll rocking amplitude in degrees.")]
        [SerializeField, Min(0f)]
        private float idleRollAngle = 1.2f;

        [Tooltip("Idle pitch rocking amplitude in degrees.")]
        [SerializeField, Min(0f)]
        private float idlePitchAngle = 0.8f;

        [Header("Wake Trails")]
        [SerializeField]
        private bool enableWake = true;

        [SerializeField]
        private TrailRenderer portWake;

        [SerializeField]
        private TrailRenderer starboardWake;

        [SerializeField]
        private Material wakeMaterial;

        [Tooltip("Minimum speed (m/s) before wake begins emitting.")]
        [SerializeField, Min(0f)]
        private float minWakeSpeed = 0.2f;

        [Tooltip("Maximum width of the wake at full speed.")]
        [SerializeField, Min(0f)]
        private float maxWakeWidth = 0.35f;

        [Tooltip("Lifetime of wake trail points in seconds.")]
        [SerializeField, Min(0.1f)]
        private float wakeLifetime = 2.5f;

        [Tooltip("Local offset for port wake trail emission point.")]
        [SerializeField]
        private Vector3 portWakeOffset = new Vector3(-0.55f, -0.08f, -1.45f);

        [Tooltip("Local offset for starboard wake trail emission point.")]
        [SerializeField]
        private Vector3 starboardWakeOffset = new Vector3(0.55f, -0.08f, -1.45f);

        // Internal State
        private float currentHeel;
        private float currentPitch;
        private float bobPhase;
        private Vector3 initialVisualLocalPosition;

        private void Awake()
        {
            if (sailingModel == null)
            {
                sailingModel = GetComponent<SailingModel>();
            }

            ResolveVisualRoot();
            SetupWakeTrails();
        }

        private void Start()
        {
            if (visualRoot != null)
            {
                initialVisualLocalPosition = visualRoot.localPosition;
            }
        }

        private void LateUpdate()
        {
            if (visualRoot == null || sailingModel == null)
            {
                return;
            }

            UpdateShipAttitude();
            UpdateWakeTrails();
        }

        private void ResolveVisualRoot()
        {
            if (visualRoot != null)
            {
                return;
            }

            // Check if there is an existing child named "Visuals"
            Transform existing = transform.Find("Visuals");
            if (existing != null)
            {
                visualRoot = existing;
                return;
            }

            // Create a dedicated Visuals container to keep physics and visuals clean
            GameObject container = new GameObject("Visuals");
            container.transform.SetParent(transform, false);
            container.transform.localPosition = Vector3.zero;
            container.transform.localRotation = Quaternion.identity;
            container.transform.localScale = Vector3.one;

            var childrenToReparent = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child == container.transform)
                {
                    continue;
                }

                // Hull, Mast, and other renderers belong in the visual container
                if (
                    child.name.Contains("Hull")
                    || child.name.Contains("Mast")
                    || child.GetComponentInChildren<Renderer>() != null
                )
                {
                    childrenToReparent.Add(child);
                }
            }

            foreach (Transform child in childrenToReparent)
            {
                child.SetParent(container.transform, true);
            }

            visualRoot = container.transform;
        }

        private void SetupWakeTrails()
        {
            if (!enableWake)
            {
                return;
            }

            if (portWake == null)
            {
                portWake = CreateWakeTrail("PortWake", portWakeOffset);
            }

            if (starboardWake == null)
            {
                starboardWake = CreateWakeTrail("StarboardWake", starboardWakeOffset);
            }
        }

        private TrailRenderer CreateWakeTrail(string name, Vector3 localOffset)
        {
            GameObject trailObj = new GameObject(name);
            trailObj.transform.SetParent(transform, false);
            trailObj.transform.localPosition = localOffset;

            TrailRenderer trail = trailObj.AddComponent<TrailRenderer>();
            trail.time = wakeLifetime;
            trail.startWidth = maxWakeWidth;
            trail.endWidth = 0.02f;
            trail.minVertexDistance = 0.1f;
            trail.autodestruct = false;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trail.receiveShadows = false;
            trail.alignment = LineAlignment.View;

            // Fading white foam gradient
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(0.9f, 0.95f, 1f), 1f),
                },
                new[]
                {
                    new GradientAlphaKey(0.6f, 0f),
                    new GradientAlphaKey(0.3f, 0.5f),
                    new GradientAlphaKey(0f, 1f),
                }
            );
            trail.colorGradient = gradient;

            // Tapering width curve
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 1f);
            curve.AddKey(0.4f, 0.7f);
            curve.AddKey(1f, 0.05f);
            trail.widthCurve = curve;

            if (wakeMaterial != null)
            {
                trail.material = wakeMaterial;
            }
            else
            {
                Shader shader =
                    Shader.Find("Universal Render Pipeline/Particles/Unlit")
                    ?? Shader.Find("Universal Render Pipeline/Unlit")
                    ?? Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    Material mat = new Material(shader) { name = "RuntimeWakeMaterial" };
                    trail.material = mat;
                }
            }

            trail.emitting = false;
            return trail;
        }

        private void UpdateShipAttitude()
        {
            float dt = Time.deltaTime;
            float speed = sailingModel.CurrentSpeed;

            // --- Heeling (Roll around Z axis) ---
            // Sailboat heels away from the wind: lateralForce > 0 rolls starboard (negative Z).
            float targetHeel = -sailingModel.CurrentLateralForce * heelSensitivity;
            // Bank into turns slightly
            targetHeel -= sailingModel.CurrentTurnRate * turnRollSensitivity;
            targetHeel = Mathf.Clamp(targetHeel, -maxHeelAngle, maxHeelAngle);

            currentHeel = Mathf.Lerp(
                currentHeel,
                targetHeel,
                1f - Mathf.Exp(-heelSmoothSpeed * dt)
            );

            // --- Pitch (Bow rise around X axis) ---
            // Forward thrust lifts the bow slightly (negative X in Unity).
            float targetPitch = -sailingModel.CurrentThrust * pitchSensitivity;
            targetPitch = Mathf.Clamp(targetPitch, -maxPitchAngle, maxPitchAngle);

            currentPitch = Mathf.Lerp(
                currentPitch,
                targetPitch,
                1f - Mathf.Exp(-pitchSmoothSpeed * dt)
            );

            // --- Ocean Swell & Floating Bobbing ---
            float bobRoll = 0f;
            float bobPitch = 0f;
            float bobY = 0f;

            if (enableBobbing)
            {
                float speedMultiplier = 1f + Mathf.Clamp(speed * 0.1f, 0f, 2f);
                bobPhase += dt * bobFrequency * speedMultiplier;

                bobRoll = Mathf.Sin(bobPhase) * idleRollAngle;
                bobPitch = Mathf.Cos(bobPhase * 0.85f) * idlePitchAngle;
                bobY = Mathf.Sin(bobPhase * 1.3f) * bobHeight;
            }

            // Apply orientation & buoyancy to visual root
            Quaternion targetRotation = Quaternion.Euler(
                currentPitch + bobPitch,
                0f,
                currentHeel + bobRoll
            );
            visualRoot.localRotation = targetRotation;
            visualRoot.localPosition = initialVisualLocalPosition + new Vector3(0f, bobY, 0f);
        }

        private void UpdateWakeTrails()
        {
            if (!enableWake)
            {
                return;
            }

            float speed = sailingModel.CurrentSpeed;
            bool shouldEmit = speed > minWakeSpeed;

            if (portWake != null)
            {
                portWake.emitting = shouldEmit;
                if (shouldEmit)
                {
                    portWake.widthMultiplier = Mathf.Clamp01(speed / 8f) * maxWakeWidth;
                }
            }

            if (starboardWake != null)
            {
                starboardWake.emitting = shouldEmit;
                if (shouldEmit)
                {
                    starboardWake.widthMultiplier = Mathf.Clamp01(speed / 8f) * maxWakeWidth;
                }
            }
        }
    }
}

