using UnityEngine;

namespace Sailgoat
{
    /// <summary>
    /// VIBECODED, including the water grid shader! Just for more fun and clarity when testing
    /// Snaps the water plane to follow the ship in discrete world grid intervals,
    /// providing an infinite flat ocean without moving the world grid lines.
    /// Also exposes grid and water visual controls directly in the Inspector.
    /// </summary>
    [ExecuteAlways]
    public sealed class WaterPlane : MonoBehaviour
    {
        [Header("Target Tracking")]
        [Tooltip("Target to follow (usually the Ship). If null, finds the Ship automatically.")]
        [SerializeField]
        private Transform target;

        [Tooltip("Grid interval for snapping the plane in world meters.")]
        [SerializeField, Min(0.1f)]
        private float snapInterval = 2f;

        [Header("Grid & Water Appearance")]
        [SerializeField]
        private Color waterColor = new Color(0f, 0.67f, 1f, 1f);

        [SerializeField]
        private Color gridLineColor = new Color(0.35f, 0.85f, 1f, 1f);

        [SerializeField, Min(0.1f)]
        private float gridSizeMeters = 2f;

        [SerializeField, Range(0.005f, 0.15f)]
        private float lineThickness = 0.04f;

        private Renderer waterRenderer;

        private static readonly int WaterColorId = Shader.PropertyToID("_WaterColor");
        private static readonly int GridColorId = Shader.PropertyToID("_GridColor");
        private static readonly int CellSizeId = Shader.PropertyToID("_CellSize");
        private static readonly int LineWidthId = Shader.PropertyToID("_LineWidth");

        private void Awake()
        {
            ApplyProperties();

            if (Application.isPlaying && target == null)
            {
                ShipController ship = Object.FindAnyObjectByType<ShipController>();
                if (ship != null)
                {
                    target = ship.transform;
                }
            }
        }

        private void OnValidate()
        {
            ApplyProperties();
        }

        public void ApplyProperties()
        {
            if (waterRenderer == null)
            {
                waterRenderer = GetComponent<Renderer>();
            }

            if (waterRenderer == null)
            {
                return;
            }

            Material mat = Application.isPlaying
                ? waterRenderer.material
                : waterRenderer.sharedMaterial;
            if (mat != null && mat.HasProperty(WaterColorId))
            {
                mat.SetColor(WaterColorId, waterColor);
                mat.SetColor(GridColorId, gridLineColor);
                mat.SetFloat(CellSizeId, gridSizeMeters);
                mat.SetFloat(LineWidthId, lineThickness);
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 currentPos = transform.position;
            float interval = Mathf.Max(0.1f, snapInterval);
            float snappedX = Mathf.Floor(target.position.x / interval) * interval;
            float snappedZ = Mathf.Floor(target.position.z / interval) * interval;

            transform.position = new Vector3(snappedX, currentPos.y, snappedZ);
        }
    }
}

