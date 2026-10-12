using Project.Scripts.Stats;

namespace Project.Scripts.Converters
{
    /// <summary>
    /// Статический форматировщик статистик <see cref="StatModifier"/> в текст.
    /// </summary>
    public static class StatTextFormatter
    {
        /// <summary>
        /// Получает текстовое имя статистики для каждого типа <see cref="StatType"/>.
        /// </summary>
        /// <param name="stat">Тип статистики.</param>
        /// <returns>Строка с именем статистики.</returns>
        public static string GetName(StatType stat)
        {
            return stat switch
            {
                StatType.MaxHealth => "Максимальное здоровье",
                StatType.MaxMana => "Максимальная мана",
                StatType.Defense => "Защита",

                StatType.MeleeDamage => "Урон ближнего боя",
                StatType.MagicDamage => "Магический урон",
                StatType.RangedDamage => "Урон дальнего боя",

                StatType.AttackSpeed => "Скорость атаки",
                StatType.MovementSpeed => "Скорость передвижения",

                StatType.CritChance => "Шанс критического удара",
                StatType.CritMultiplier => "Множитель критического урона",

                _ => stat.ToString()
            };
        }

        /// <summary>
        /// Форматирует <see cref="StatModifier"/> в необходимую строку.
        /// </summary>
        /// <param name="modifier">Модификатор статистики.</param>
        /// <returns>Строка с полным описанием конкретной статистики.</returns>
        public static string Format(StatModifier modifier)
        {
            string name = GetName(modifier.StatType);

            return modifier.StatType switch
            {
                StatType.MaxHealth =>
                    $"{name}: {FormatAbsolute(modifier.Value)}",

                StatType.MaxMana =>
                    $"{name}: {FormatAbsolute(modifier.Value)}",

                StatType.Defense =>
                    $"{name}: {FormatAbsolute(modifier.Value)}",

                StatType.MeleeDamage =>
                    $"{name}: {FormatPercent(modifier.Value)}",

                StatType.MagicDamage =>
                    $"{name}: {FormatPercent(modifier.Value)}",

                StatType.RangedDamage =>
                    $"{name}: {FormatPercent(modifier.Value)}",

                StatType.AttackSpeed =>
                    $"{name}: {FormatPercent(modifier.Value)}",

                StatType.MovementSpeed =>
                    $"{name}: {FormatPercent(modifier.Value)}",

                StatType.CritChance =>
                    $"{name}: {FormatPercent(modifier.Value)}",

                StatType.CritMultiplier =>
                    $"{name}: {FormatMultiplier(modifier.Value)}",

                _ =>
                    $"{name}: {modifier.Value}"
            };
        }

        /// <summary>
        /// Приводит строку статистики к общему формату.
        /// </summary>
        /// <param name="value">Значение статистики.</param>
        /// <returns>Строка общего формата.</returns>
        private static string FormatAbsolute(float value)
        {
            return value >= 0
                ? $"+{value:0.##}"
                : $"{value:0.##}";
        }

        /// <summary>
        /// Приводит строку статистики к формату "В процентах".
        /// </summary>
        /// <param name="value">Значение статистики.</param>
        /// <returns>Строка формата "В процентах".</returns>
        private static string FormatPercent(float value)
        {
            return value >= 0
                ? $"+{value:0.##}%"
                : $"{value:0.##}%";
        }

        /// <summary>
        /// Приводит строку статистики к формату "Множитель".
        /// </summary>
        /// <param name="value">Значение статистики.</param>
        /// <returns>Строка формата "Множитель".</returns>
        private static string FormatMultiplier(float value)
        {
            return value >= 0
                ? $"+{value:0.##}x"
                : $"{value:0.##}x";
        }
    }
}