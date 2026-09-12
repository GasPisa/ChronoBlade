using System;

namespace ChronoBlade.Data
{
    // Declarative stat modifier authored on an UpgradeNodeDefinition (or any future buff source).
    // A consumer applies this at runtime via PlayerRuntimeStats.AddModifier(source, stat, value, type).
    [Serializable]
    public class StatModifierData
    {
        public StatType stat;
        public ModifierType type;
        public float value;
    }
}
