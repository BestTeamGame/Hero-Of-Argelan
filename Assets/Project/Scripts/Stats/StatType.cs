using Project.Scripts.Converters;

namespace Project.Scripts.Stats
{
    /// <summary>
    /// Перечисление типов статистики.
    /// </summary>
    /// <remarks>
    /// Для каждого типа должен быть создан свой случай в <see cref="StatTextFormatter"/>.
    /// </remarks>
    public enum StatType
    {
        MaxHealth,
        MaxMana,
        Defense,
        
        MeleeDamage,
        MagicDamage,
        RangedDamage,
        
        AttackSpeed,
        MovementSpeed,
        
        CritChance,
        CritMultiplier
    }
}
