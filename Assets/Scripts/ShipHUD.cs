using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public sealed class ShipHud : MonoBehaviour
{
    [SerializeField]
    private ShipController shipController;

    private PanelRenderer panelRenderer;
    private Label openingLabel;
    private Label rotationLabel;

    private void Awake()
    {
        panelRenderer = GetComponent<PanelRenderer>();
    }

    private void OnEnable()
    {
        panelRenderer.RegisterUIReloadCallback(OnUIReload);

        if (shipController != null)
        {
            shipController.StateChanged += RefreshLabels;
        }
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);

        if (shipController != null)
        {
            shipController.StateChanged -= RefreshLabels;
        }
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement, int version)
    {
        openingLabel = rootElement.Q<Label>("opening-label");
        rotationLabel = rootElement.Q<Label>("rotation-label");

        RefreshLabels();
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
