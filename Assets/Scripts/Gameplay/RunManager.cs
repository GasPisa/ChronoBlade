using System.Collections.Generic;
using ChronoBlade.Data;
using UnityEngine;

namespace ChronoBlade.Gameplay
{
    // Orchestrates the actual run loop: wave -> altar (spend time, pick an upgrade) ->
    // continue (harder wave) or bank (end the run) -> repeat, until time runs out.
    // The single source of truth for run state; WaveManager/EnemyAgent/PlayerController/GameHUD
    // all read or report through this instead of wiring directly to each other.
    public class RunManager : MonoBehaviour
    {
        public static RunManager Instance { get; private set; }

        [Header("Data")]
        public StatBlock statBlock;
        public List<UpgradeNodeDefinition> altarPool = new List<UpgradeNodeDefinition>();

        [Header("Scene refs")]
        public WaveManager waveManager;

        public PlayerRuntimeStats Stats { get; private set; }
        public RunPhase Phase { get; private set; } = RunPhase.Combat;
        public int WaveNumber { get; private set; } = 1;
        public int KillCount { get; private set; }
        public bool Banked { get; private set; }
        public bool HasPickedUpgradeThisAltar { get; private set; }
        public UpgradeNodeDefinition[] CurrentAltarOptions { get; private set; } = new UpgradeNodeDefinition[0];

        TimeEconomyController _economy;

        void Awake()
        {
            Instance = this;
            Stats = new PlayerRuntimeStats(statBlock);
            _economy = FindFirstObjectByType<TimeEconomyController>();
            if (_economy != null) _economy.OnTimeDepleted += () => EndRun(banked: false);
        }

        void Start()
        {
            waveManager.StartWave(WaveNumber);
        }

        // Called by RegisterKill (not EnemyAgent directly) so Siega and wave-clear detection
        // both happen through one funnel.
        public void RegisterKill(EnemyDefinition definition)
        {
            if (Phase != RunPhase.Combat) return;

            KillCount++;
            float siega = Stats.GetValue(StatType.Siega);
            _economy.RegisterKill(definition, siega);
            waveManager.ReportDeath();
        }

        public void OnWaveCleared()
        {
            if (Phase != RunPhase.Combat) return;

            Phase = RunPhase.Altar;
            HasPickedUpgradeThisAltar = false;
            CurrentAltarOptions = PickAltarOptions();
        }

        UpgradeNodeDefinition[] PickAltarOptions()
        {
            var pool = new List<UpgradeNodeDefinition>(altarPool);
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            int take = Mathf.Min(3, pool.Count);
            return pool.GetRange(0, take).ToArray();
        }

        public void ChooseUpgrade(int index)
        {
            if (Phase != RunPhase.Altar || HasPickedUpgradeThisAltar) return;
            if (index < 0 || index >= CurrentAltarOptions.Length) return;

            var node = CurrentAltarOptions[index];

            foreach (var mod in node.statModifiers)
            {
                Stats.AddModifier(node, mod.stat, mod.value, mod.type);
                if (mod.stat == StatType.Vigor && mod.type == ModifierType.Flat)
                {
                    _economy.IncreaseCapacity(mod.value);
                }
            }

            _economy.SpendTime(-node.timeCost, $"Altar: {node.displayName}");
            HasPickedUpgradeThisAltar = true;
        }

        public void ContinueToNextWave()
        {
            if (Phase != RunPhase.Altar || !HasPickedUpgradeThisAltar) return;

            WaveNumber++;
            Phase = RunPhase.Combat;
            waveManager.StartWave(WaveNumber);
        }

        public void BankAndEnd()
        {
            if (Phase != RunPhase.Altar || !HasPickedUpgradeThisAltar) return;
            EndRun(banked: true);
        }

        public void EndRun(bool banked)
        {
            if (Phase == RunPhase.Complete) return;

            Banked = banked;
            Phase = RunPhase.Complete;
        }
    }
}
