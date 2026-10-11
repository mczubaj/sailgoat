using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public sealed class ShipHud : MonoBehaviour
{
    [SerializeField]
    private ShipController shipController;

    [SerializeField]
    private VisualElementReference<Label> openingLabelReference = new();

    [SerializeField]
    private VisualElementReference<Label> rotationLabelReference = new();

    private Label openingLabel;
    private Label rotationLabel;

    private void OnEnable()
    {
        openingLabelReference.RegisterReferenceResolvedCallback(OnOpeningLabelResolved);
        openingLabelReference.RegisterReferenceUnloadedCallback(OnOpeningLabelUnloaded);

        rotationLabelReference.RegisterReferenceResolvedCallback(OnRotationLabelResolved);
        rotationLabelReference.RegisterReferenceUnloadedCallback(OnRotationLabelUnloaded);

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

        if (shipController != null)
        {
            shipController.StateChanged -= RefreshLabels;
        }

        openingLabel = null;
        rotationLabel = null;
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
}
