using System.Linq;
using ChronoBlade.Data;
using ChronoBlade.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace ChronoBlade.Tests
{
    public class TimeEconomyControllerTests
    {
        TimeEconomyConfig _config;
        EnemyDefinition _fodder;
        GameObject _go;
        TimeEconomyController _controller;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<TimeEconomyConfig>();
            _config.startingTimeCapacity = 20f;
            _config.passiveDrainPerSecond = 1f;
            _config.hitTimeCost = 3f;
            _config.lowTimeWarningThreshold = 5f;

            _fodder = ScriptableObject.CreateInstance<EnemyDefinition>();
            _fodder.enemyId = "fodder_test";
            _fodder.displayName = "Test Fodder";
            _fodder.archetype = EnemyArchetype.Fodder;
            _fodder.damageOnHit = 2f;
            _fodder.timeRewardOnKill = 4f;

            _go = new GameObject("TimeEconomyControllerTest");
            _controller = _go.AddComponent<TimeEconomyController>();
            _controller.Initialize(_config);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
            Object.DestroyImmediate(_config);
            Object.DestroyImmediate(_fodder);
        }

        [Test]
        public void Initialize_SetsCurrentTimeToStartingCapacity()
        {
            Assert.AreEqual(20f, _controller.CurrentTime);
            Assert.AreEqual(20f, _controller.MaxTime);
            Assert.IsFalse(_controller.IsDepleted);
        }

        [Test]
        public void Tick_AppliesPassiveDrain_AndFiresOnTimeChanged()
        {
            float? reported = null;
            _controller.OnTimeChanged += t => reported = t;

            _controller.Tick(2f); // 2s * 1/s drain

            Assert.AreEqual(18f, _controller.CurrentTime);
            Assert.AreEqual(18f, reported);
        }

        [Test]
        public void RegisterHit_UsesEnemyDamageOverride_NotConfigDefault()
        {
            _controller.RegisterHit(_fodder);
            Assert.AreEqual(18f, _controller.CurrentTime); // 20 - 2 (fodder override), not -3
        }

        [Test]
        public void RegisterHit_FallsBackToConfigHitTimeCost_WhenNoEnemyGiven()
        {
            _controller.RegisterHit(null);
            Assert.AreEqual(17f, _controller.CurrentTime); // 20 - 3 (config default)
        }

        [Test]
        public void RegisterKill_AddsTimeRewardOnKill()
        {
            _controller.SpendTime(-10f, "setup");
            _controller.RegisterKill(_fodder);
            Assert.AreEqual(14f, _controller.CurrentTime); // 10 + 4
        }

        [Test]
        public void RegisterKill_RespectsSiegaMultiplier()
        {
            _controller.SpendTime(-10f, "setup");
            _controller.RegisterKill(_fodder, siegaMultiplier: 1.5f);
            Assert.AreEqual(16f, _controller.CurrentTime); // 10 + (4 * 1.5)
        }

        [Test]
        public void SpendTime_AltarCost_DrainsTimeAndLogsReason()
        {
            _controller.SpendTime(-8f, "Altar: Vigor II");

            Assert.AreEqual(12f, _controller.CurrentTime);
            var last = _controller.Ledger.Last();
            Assert.AreEqual(-8f, last.Delta);
            Assert.AreEqual("Altar: Vigor II", last.Reason);
            Assert.AreEqual(12f, last.ResultingTime);
        }

        [Test]
        public void CurrentTime_NeverExceedsMaxTime()
        {
            _controller.SpendTime(1000f, "overflow test");
            Assert.AreEqual(20f, _controller.CurrentTime);
        }

        [Test]
        public void CurrentTime_ReachingZero_FiresOnTimeDepleted_ExactlyOnce()
        {
            int depletedCount = 0;
            _controller.OnTimeDepleted += () => depletedCount++;

            _controller.SpendTime(-25f, "lethal hit");
            Assert.AreEqual(0f, _controller.CurrentTime);
            Assert.IsTrue(_controller.IsDepleted);
            Assert.AreEqual(1, depletedCount);

            _controller.SpendTime(-1f, "should be ignored once depleted");
            Assert.AreEqual(1, depletedCount, "OnTimeDepleted must only fire once");
        }
    }
}
