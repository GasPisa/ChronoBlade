using System.Collections.Generic;

namespace ChronoBlade.Data
{
    // Runtime (base + modifiers) view over a StatBlock asset. One instance per play session —
    // never persisted as an asset, unlike the StatBlock it wraps.
    public class PlayerRuntimeStats
    {
        readonly struct AppliedModifier
        {
            public readonly object Source;
            public readonly StatType Stat;
            public readonly ModifierType Type;
            public readonly float Value;

            public AppliedModifier(object source, StatType stat, ModifierType type, float value)
            {
                Source = source;
                Stat = stat;
                Type = type;
                Value = value;
            }
        }

        readonly StatBlock _baseStats;
        readonly List<AppliedModifier> _modifiers = new List<AppliedModifier>();

        public PlayerRuntimeStats(StatBlock baseStats)
        {
            _baseStats = baseStats;
        }

        // (base + sum of Flat modifiers) * (1 + sum of Percent modifiers)
        public float GetValue(StatType stat)
        {
            float baseValue = _baseStats != null ? _baseStats.GetBaseValue(stat) : 0f;
            float flatSum = 0f;
            float percentSum = 0f;

            for (int i = 0; i < _modifiers.Count; i++)
            {
                var modifier = _modifiers[i];
                if (modifier.Stat != stat) continue;

                if (modifier.Type == ModifierType.Flat) flatSum += modifier.Value;
                else percentSum += modifier.Value;
            }

            return (baseValue + flatSum) * (1f + percentSum);
        }

        public void AddModifier(object source, StatType stat, float value, ModifierType type)
        {
            _modifiers.Add(new AppliedModifier(source, stat, type, value));
        }

        public void RemoveModifiersFromSource(object source)
        {
            _modifiers.RemoveAll(m => Equals(m.Source, source));
        }
    }
}
