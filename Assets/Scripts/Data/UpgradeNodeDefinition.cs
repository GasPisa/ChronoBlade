using System.Collections.Generic;
using UnityEngine;

namespace ChronoBlade.Data
{
    // A single node in the altar progression tree (authored visually by spec 05's tree editor).
    [CreateAssetMenu(menuName = "CHRONOBLADE/Upgrade Node Definition", fileName = "NewUpgradeNode")]
    public class UpgradeNodeDefinition : ScriptableObject
    {
        public string nodeId;
        public string displayName;

        [Tooltip("Time cost to purchase this node at an altar.")]
        public float timeCost;

        public List<StatModifierData> statModifiers = new List<StatModifierData>();

        [Tooltip("nodeId values of upgrades that must be owned before this one can be purchased.")]
        public List<string> prerequisiteNodeIds = new List<string>();
    }
}
