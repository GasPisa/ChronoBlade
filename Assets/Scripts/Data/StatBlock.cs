using UnityEngine;

namespace ChronoBlade.Data
{
    // Base (unmodified) values for the three player stats. Runtime modifiers layer on top
    // of this via PlayerRuntimeStats — this asset never changes at runtime.
    [CreateAssetMenu(menuName = "CHRONOBLADE/Stat Block", fileName = "StatBlock")]
    public class StatBlock : ScriptableObject
    {
        [Tooltip("Base max time capacity, in seconds.")]
        public float baseVigor = 30f;

        [Tooltip("Base time-per-kill multiplier.")]
        public float baseSiega = 1f;

        [Tooltip("Base move / attack speed multiplier.")]
        public float basePremura = 1f;

        public float GetBaseValue(StatType stat)
        {
            switch (stat)
            {
                case StatType.Vigor: return baseVigor;
                case StatType.Siega: return baseSiega;
                case StatType.Premura: return basePremura;
                default: return 0f;
            }
        }
    }
}
