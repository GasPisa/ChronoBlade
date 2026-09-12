using System;
using System.Collections.Generic;
using ChronoBlade.Data;
using UnityEngine;

namespace ChronoBlade.Gameplay
{
    // Owns the live time value at runtime. Thin — no tuning values hardcoded, everything comes
    // from the injected TimeEconomyConfig. Every change (drain, hit, kill, altar spend) funnels
    // through SpendTime so a single ledger captures the full history for the debug HUD.
    //
    // Deliberately decoupled from Update(): callers drive it via Tick(deltaTime) so the time
    // economy balancer (spec 02) can simulate this same class outside Play mode.
    public class TimeEconomyController : MonoBehaviour
    {
        const int LedgerCapacity = 32;

        [SerializeField] TimeEconomyConfig config;

        readonly Queue<TimeLedgerEntry> _ledger = new Queue<TimeLedgerEntry>(LedgerCapacity);

        public event Action<float> OnTimeChanged;
        public event Action OnTimeDepleted;
        public event Action<TimeLedgerEntry> OnTimeSpent;

        public float CurrentTime { get; private set; }
        public float MaxTime { get; private set; }
        public bool IsDepleted { get; private set; }
        public IReadOnlyCollection<TimeLedgerEntry> Ledger => _ledger;

        // maxTimeOverride lets Vigor (via PlayerRuntimeStats.GetValue) set the real cap; falls
        // back to the config's startingTimeCapacity when omitted.
        public void Initialize(TimeEconomyConfig economyConfig, float? maxTimeOverride = null)
        {
            config = economyConfig;
            MaxTime = maxTimeOverride ?? config.startingTimeCapacity;
            CurrentTime = MaxTime;
            IsDepleted = false;
            _ledger.Clear();

            OnTimeChanged?.Invoke(CurrentTime);
        }

        // Plain setter for editor/bootstrap tooling that wires the scene before Play mode
        // (avoids SerializedObject round-tripping just to assign a reference field).
        public void AssignConfig(TimeEconomyConfig economyConfig) => config = economyConfig;

        // Auto-initializes from the Inspector-assigned config when this component enters
        // Play mode via the scene (tests call Initialize() explicitly instead, before this
        // ever runs, so no double-init happens there).
        void Awake()
        {
            if (config != null && MaxTime <= 0f) Initialize(config);
        }

        public void Tick(float deltaTime)
        {
            if (config == null || IsDepleted) return;
            SpendTime(-config.passiveDrainPerSecond * deltaTime, "Passive drain");
        }

        // Raises the cap (e.g. an altar's Vigor upgrade) and grants the same amount of
        // current time, logged through SpendTime so it shows in the ledger like any other event.
        public void IncreaseCapacity(float amount)
        {
            if (amount <= 0f) return;
            MaxTime += amount;
            SpendTime(amount, "Vigor capacity increase");
        }

        public void RegisterHit(EnemyDefinition source)
        {
            float cost = source != null ? source.damageOnHit : config.hitTimeCost;
            string reason = source != null ? $"Hit by {source.displayName}" : "Hit";
            SpendTime(-cost, reason);
        }

        public void RegisterKill(EnemyDefinition enemy, float siegaMultiplier = 1f)
        {
            if (enemy == null) return;
            SpendTime(enemy.timeRewardOnKill * siegaMultiplier, $"{enemy.displayName} kill");
        }

        // The single funnel for every time change. amount is signed: negative spends
        // (drain / hits / altar purchases), positive rewards (kills). reason is logged to the
        // ledger for spec 06's HUD.
        public void SpendTime(float amount, string reason)
        {
            if (IsDepleted) return;

            CurrentTime = Mathf.Clamp(CurrentTime + amount, 0f, MaxTime);

            var entry = new TimeLedgerEntry(amount, reason, CurrentTime);
            if (_ledger.Count >= LedgerCapacity) _ledger.Dequeue();
            _ledger.Enqueue(entry);

            OnTimeSpent?.Invoke(entry);
            OnTimeChanged?.Invoke(CurrentTime);

            if (CurrentTime <= 0f && !IsDepleted)
            {
                IsDepleted = true;
                OnTimeDepleted?.Invoke();
            }
        }
    }
}
