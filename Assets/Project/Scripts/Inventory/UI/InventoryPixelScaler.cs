using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.Inventory.UI
{
    [ExecuteAlways]
    public sealed class InventoryPixelScaler : CanvasScaler
    {
        [Tooltip("Whole-pixel scaling above the reference size. Smaller windows shrink to fit.")]
        public bool integerScale = true;

        protected override void HandleScaleWithScreenSize()
        {
            if (!integerScale) { base.HandleScaleWithScreenSize(); return; }
            Canvas canvas = GetComponent<Canvas>();
            Vector2 size = canvas.pixelRect.size;
            float fit = Mathf.Min(size.x / referenceResolution.x, size.y / referenceResolution.y);
            float scale = fit >= 1f ? Mathf.Floor(fit) : Mathf.Max(0.01f, fit);
            SetScaleFactor(scale);
            SetReferencePixelsPerUnit(referencePixelsPerUnit);
        }
    }
}
