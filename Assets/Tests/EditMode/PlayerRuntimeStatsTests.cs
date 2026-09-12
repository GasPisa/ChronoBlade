using ChronoBlade.Data;
using NUnit.Framework;
using UnityEngine;

namespace ChronoBlade.Tests
{
    public class PlayerRuntimeStatsTests
    {
        StatBlock _statBlock;

        [SetUp]
        public void SetUp()
        {
            _statBlock = ScriptableObject.CreateInstance<StatBlock>();
            _statBlock.baseVigor = 30f;
            _statBlock.baseSiega = 1f;
            _statBlock.basePremura = 1f;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_statBlock);
        }

        [Test]
        public void GetValue_WithNoModifiers_ReturnsBaseValue()
        {
            var stats = new PlayerRuntimeStats(_statBlock);
            Assert.AreEqual(30f, stats.GetValue(StatType.Vigor));
        }

        [Test]
        public void AddModifier_Flat_AddsBeforePercent()
        {
            var stats = new PlayerRuntimeStats(_statBlock);
            object node = new object();

            stats.AddModifier(node, StatType.Vigor, 10f, ModifierType.Flat);
            stats.AddModifier(node, StatType.Vigor, 0.5f, ModifierType.Percent); // +50%

            // (30 + 10) * 1.5 = 60
            Assert.AreEqual(60f, stats.GetValue(StatType.Vigor));
        }

        [Test]
        public void RemoveModifiersFromSource_OnlyRemovesThatSourcesModifiers()
        {
            var stats = new PlayerRuntimeStats(_statBlock);
            object nodeA = new object();
            object nodeB = new object();

            stats.AddModifier(nodeA, StatType.Vigor, 10f, ModifierType.Flat);
            stats.AddModifier(nodeB, StatType.Vigor, 5f, ModifierType.Flat);

            stats.RemoveModifiersFromSource(nodeA);

            Assert.AreEqual(35f, stats.GetValue(StatType.Vigor)); // 30 + 5 (nodeB only)
        }

        [Test]
        public void GetValue_OnUnaffectedStat_IsUnchangedByOtherStatModifiers()
        {
            var stats = new PlayerRuntimeStats(_statBlock);
            stats.AddModifier(this, StatType.Vigor, 100f, ModifierType.Flat);

            Assert.AreEqual(1f, stats.GetValue(StatType.Siega));
        }
    }
}
