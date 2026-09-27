using Project.Scripts.Inventory.Items;
using UnityEngine;

namespace Project.Scripts.Inventory
{
    public class PlayerInventory : MonoBehaviour
    {
        [Header("Weapons")]
        [SerializeField] private WeaponData weaponSlot1;
        [SerializeField] private WeaponData weaponSlot2;

        // [Header("Skill")]
        // [SerializeField] private ActiveSkillData activeSkill;

        // [Header("Armor")]
        // [SerializeField] private ArmorData headArmor;
        // [SerializeField] private ArmorData bodyArmor;

        // [Header("Artifacts")]
        // [SerializeField] private ArtifactData[] artifacts = new ArtifactData[3];

        [Header("Consumables")]
        [SerializeField] private int healthPotions;
        [SerializeField] private int manaPotions;

        [Header("Resources")]
        [SerializeField] private int coins;
        [SerializeField] private int arrows;

        // [Header("Key Items")]
        // [SerializeField] private List<KeyItemData> keyItems = new();
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
