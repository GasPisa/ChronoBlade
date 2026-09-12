using System.Collections.Generic;
using ChronoBlade.Data;
using UnityEngine;

namespace ChronoBlade.Gameplay
{
    // Spawns one discrete, escalating batch of enemies per wave (instead of a timer that never
    // stops) and reports back to RunManager when the batch is fully cleared.
    public class WaveManager : MonoBehaviour
    {
        public EnemyDefinition fodder;
        public EnemyDefinition tank;
        public EnemyDefinition fast;
        public float spawnRadius = 6.5f;

        int _aliveCount;

        public void StartWave(int waveNumber)
        {
            int count = 3 + waveNumber;
            _aliveCount = count;

            for (int i = 0; i < count; i++)
            {
                SpawnOne(PickDefinitionForWave(waveNumber));
            }
        }

        EnemyDefinition PickDefinitionForWave(int waveNumber)
        {
            if (waveNumber <= 2) return fodder;

            float roll = Random.value;
            if (roll < 0.5f) return fodder;
            if (roll < 0.8f) return fast;
            return tank;
        }

        void SpawnOne(EnemyDefinition def)
        {
            if (def == null) { _aliveCount--; return; }

            Vector2 dir = Random.insideUnitCircle.normalized;
            Vector3 pos = transform.position + (Vector3)(dir * spawnRadius);

            var go = new GameObject(def.displayName);
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSprite.Square();
            sr.color = ColorForArchetype(def.archetype);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            go.AddComponent<CircleCollider2D>();

            var agent = go.AddComponent<EnemyAgent>();
            agent.definition = def;

            float scale = def.archetype == EnemyArchetype.Tank ? 1.8f : 1.1f;
            go.transform.localScale = Vector3.one * scale;
        }

        // Called by EnemyAgent (via RunManager.RegisterKill) when one of this wave's enemies dies.
        public void ReportDeath()
        {
            _aliveCount--;
            if (_aliveCount <= 0) RunManager.Instance.OnWaveCleared();
        }

        static Color ColorForArchetype(EnemyArchetype archetype)
        {
            switch (archetype)
            {
                case EnemyArchetype.Fodder: return new Color(0.8f, 0.8f, 0.8f);
                case EnemyArchetype.Tank: return new Color(0.6f, 0.2f, 0.2f);
                case EnemyArchetype.Fast: return new Color(0.9f, 0.9f, 0.2f);
                case EnemyArchetype.Elite: return new Color(0.7f, 0.1f, 0.9f);
                default: return Color.white;
            }
        }
    }
}
