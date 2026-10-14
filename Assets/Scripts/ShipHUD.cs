using UnityEngine;
using UnityEngine.UIElements;

namespace Sailgoat
{
    [RequireComponent(typeof(PanelRenderer))]
    public sealed class ShipHud : MonoBehaviour
    {
        [SerializeField]
        private ShipController shipController;

        [SerializeField]
        private SailingModel sailingModel;

        [SerializeField]
        private VisualElementReference<Label> openingLabelReference = new();

        [SerializeField]
        private VisualElementReference<Label> rotationLabelReference = new();

        [SerializeField]
        private VisualElementReference<VisualElement> windArrowReference = new();

        [Header("Debug")]
        [Tooltip("Toggle wind direction arrow visibility on the HUD (defaults to off).")]
        [SerializeField]
        private bool showWindArrow;

        public bool ShowWindArrow
        {
            get => showWindArrow;
            set
            {
                showWindArrow = value;
                ApplyWindArrowVisibility();
            }
        }

        private Label openingLabel;
        private Label rotationLabel;
        private VisualElement windArrow;

        private void OnEnable()
        {
            openingLabelReference.RegisterReferenceResolvedCallback(OnOpeningLabelResolved);
            openingLabelReference.RegisterReferenceUnloadedCallback(OnOpeningLabelUnloaded);

            rotationLabelReference.RegisterReferenceResolvedCallback(OnRotationLabelResolved);
            rotationLabelReference.RegisterReferenceUnloadedCallback(OnRotationLabelUnloaded);

            windArrowReference.RegisterReferenceResolvedCallback(OnWindArrowResolved);
            windArrowReference.RegisterReferenceUnloadedCallback(OnWindArrowUnloaded);

            if (shipController != null)
            {
                shipController.StateChanged += RefreshLabels;
            }
        }

        private void OnDisable()
        {
            openingLabelReference.UnregisterReferenceResolvedCallback(OnOpeningLabelResolved);
            openingLabelReference.UnregisterReferenceUnloadedCallback(OnOpeningLabelUnloaded);

            rotationLabelReference.UnregisterReferenceResolvedCallback(OnRotationLabelResolved);
            rotationLabelReference.UnregisterReferenceUnloadedCallback(OnRotationLabelUnloaded);

            windArrowReference.UnregisterReferenceResolvedCallback(OnWindArrowResolved);
            windArrowReference.UnregisterReferenceUnloadedCallback(OnWindArrowUnloaded);

            if (shipController != null)
            {
                shipController.StateChanged -= RefreshLabels;
            }

            openingLabel = null;
            rotationLabel = null;
            windArrow = null;
        }

        private void Update()
        {
            UpdateWindDisplay();
        }

        private void OnValidate()
        {
            ApplyWindArrowVisibility();
        }

        private void OnOpeningLabelResolved(Label label)
        {
            openingLabel = label;
            RefreshLabels();
        }

        private void OnOpeningLabelUnloaded(Label label)
        {
            if (openingLabel == label)
            {
                openingLabel = null;
            }
        }

        private void OnRotationLabelResolved(Label label)
        {
            rotationLabel = label;
            RefreshLabels();
        }

        private void OnRotationLabelUnloaded(Label label)
        {
            if (rotationLabel == label)
            {
                rotationLabel = null;
            }
        }

        private void OnWindArrowResolved(VisualElement arrow)
        {
            windArrow = arrow;
            ApplyWindArrowVisibility();
            UpdateWindDisplay();
        }

        private void OnWindArrowUnloaded(VisualElement arrow)
        {
            if (windArrow == arrow)
            {
                windArrow = null;
            }
        }

        private void ApplyWindArrowVisibility()
        {
            if (windArrow != null)
            {
                windArrow.style.display = showWindArrow ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void RefreshLabels()
        {
            if (shipController == null)
            {
                return;
            }

            if (openingLabel != null)
            {
                openingLabel.text = $"Sail opening: {shipController.SailOpening:0}%";
            }

            if (rotationLabel != null)
            {
                rotationLabel.text = $"Sail rotation: {shipController.SailRotation:0}°";
            }
        }

        private void UpdateWindDisplay()
        {
            if ((sailingModel == null) || (windArrow == null) || !showWindArrow)
            {
                return;
            }

            windArrow.style.rotate = new Rotate(Angle.Degrees(sailingModel.WindDirectionDegrees));
        }
    }
}
