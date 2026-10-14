using UnityEngine;
using UnityEngine.InputSystem;

namespace Sailgoat
{
    public sealed class ShipInputController : MonoBehaviour
    {
        [SerializeField]
        private ShipController shipController;

        [SerializeField]
        private InputActionReference sailOpeningAction;

        [SerializeField]
        private InputActionReference sailRotationAction;

        [SerializeField, Min(0f)]
        private float openingChangePerSecond = 50f;

        [SerializeField, Min(0f)]
        private float rotationChangePerSecond = 45f;

        private void OnEnable()
        {
            sailOpeningAction.action?.Enable();
            sailRotationAction.action?.Enable();
        }

        private void OnDisable()
        {
            sailOpeningAction.action?.Disable();
            sailRotationAction.action?.Disable();
        }

        private void Update()
        {
            if (shipController == null)
            {
                return;
            }

            float openingInput = sailOpeningAction?.action?.ReadValue<float>() ?? 0f;
            float rotationInput = sailRotationAction?.action?.ReadValue<float>() ?? 0f;

            shipController.ChangeSailOpening(
                openingInput * openingChangePerSecond * Time.deltaTime
            );
            shipController.ChangeSailRotation(
                rotationInput * rotationChangePerSecond * Time.deltaTime
            );
        }
    }
}
