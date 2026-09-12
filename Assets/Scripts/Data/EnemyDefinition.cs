using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChronoBlade.Data
{
    public enum EnemyArchetype
    {
        Fodder,
        Tank,
        Fast,
        Elite,
    }

    [Serializable]
    public class TimeFragmentLootEntry
    {
        public int minFragments = 1;
        public int maxFragments = 3;
        [Range(0f, 1f)] public float dropChance = 1f;
    }

    // Time Fragment drop data. Populated for Elite archetypes only (see EnemyDefinition.lootTable).
    [Serializable]
    public class LootTable
    {
        public List<TimeFragmentLootEntry> entries = new List<TimeFragmentLootEntry>();
    }

    [CreateAssetMenu(menuName = "CHRONOBLADE/Enemy Definition", fileName = "NewEnemyDefinition")]
    public class EnemyDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string enemyId;
        public string displayName;
        public EnemyArchetype archetype;

        [Header("Combat")]
        public float maxHealth = 10f;
        public float moveSpeed = 3f;
        [Tooltip("Time cost inflicted on the player when this enemy lands a hit.")]
        public float damageOnHit = 1f;
        [Tooltip("Time rewarded to the player on kill — the risk/reward number the balancer graphs.")]
        public float timeRewardOnKill = 1f;

        [Header("Loot (Elite archetype only)")]
        public LootTable lootTable = new LootTable();
    }
}
