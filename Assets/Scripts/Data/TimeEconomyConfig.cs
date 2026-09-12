using UnityEngine;

namespace ChronoBlade.Data
{
    // Tunable knobs for the time economy: time is both the player's HP and currency.
    [CreateAssetMenu(menuName = "CHRONOBLADE/Time Economy Config", fileName = "TimeEconomyConfig")]
    public class TimeEconomyConfig : ScriptableObject
    {
        [Tooltip("Starting / base max time capacity, in seconds.")]
        public float startingTimeCapacity = 30f;

        [Tooltip("Time drained per second even when nothing happens.")]
        public float passiveDrainPerSecond = 0.5f;

        [Tooltip("Default time cost of a hit when the source EnemyDefinition doesn't override it (damageOnHit).")]
        public float hitTimeCost = 2f;

        [Tooltip("Time-remaining threshold below which low-time warnings (UI/audio) should trigger.")]
        public float lowTimeWarningThreshold = 5f;
    }
}
