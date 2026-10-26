using System;
using Project.Scripts.Stats;
using UnityEngine;

namespace Project.Scripts.PlayerScripts
{
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerMana : MonoBehaviour
    {
        private PlayerStats stats;

        [SerializeField] private float currentMana;

        public float CurrentMana => currentMana;

        public float MaxMana =>
            stats.GetStat(StatType.MaxMana);

        public event Action<float, float> ManaChanged;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();

            currentMana = MaxMana;
        }

        private void OnEnable()
        {
            stats.StatChanged += OnStatChanged;
        }

        private void OnDisable()
        {
            stats.StatChanged -= OnStatChanged;
        }

        public bool Spend(float amount)
        {
            if (amount <= 0f || currentMana < amount)
                return false;

            currentMana -= amount;

            ManaChanged?.Invoke(
                currentMana,
                MaxMana
            );

            return true;
        }

        public bool Restore(float amount)
        {
            if (amount <= 0f || currentMana >= MaxMana)
                return false;

            currentMana = Mathf.Min(
                currentMana + amount,
                MaxMana
            );

            ManaChanged?.Invoke(
                currentMana,
                MaxMana
            );

            return true;
        }

        public void Fill()
        {
            if (Mathf.Approximately(currentMana, MaxMana))
                return;

            currentMana = MaxMana;

            ManaChanged?.Invoke(
                currentMana,
                MaxMana
            );
        }

        private void OnStatChanged(StatType stat)
        {
            if (stat != StatType.MaxMana)
                return;

            currentMana = Mathf.Min(
                currentMana,
                MaxMana
            );

            ManaChanged?.Invoke(
                currentMana,
                MaxMana
            );
        }
    }
}