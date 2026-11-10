using Project.Scripts.Inventory.UI;
using Project.Scripts.PlayerScripts;
using UnityEngine;

namespace Project.Scripts.Services
{
    public sealed class InventoryUIBinder : MonoBehaviour
    {
        [SerializeField] private PlayerContext player;
        [SerializeField] private InventoryUIController inventoryUI;

        private void Start()
        {
            if (!player || !inventoryUI)
            {
                Debug.LogError("InventoryUIBinder: назначьте Player и Inventory UI.", this);
                return;
            }

            inventoryUI.Initialize(player.Inventory);
        }
    }
}