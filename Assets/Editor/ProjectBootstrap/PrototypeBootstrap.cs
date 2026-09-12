using ChronoBlade.Data;
using ChronoBlade.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectBootstrap
{
    // One-shot setup: creates the sample data assets (spec 01 acceptance criteria + spec 07's
    // altar nodes) and wires a minimal playable scene (player, time economy, waves, altar,
    // HUD) so the prototype can be pressed Play on immediately. Safe to delete after running.
    public static class PrototypeBootstrap
    {
        const string DataFolder = "Assets/Data";
        const string EnemiesFolder = "Assets/Data/Enemies";
        const string UpgradesFolder = "Assets/Data/Upgrades";
        const string ScenePath = "Assets/Scenes/SampleScene.unity";

        // Invoke with: -executeMethod ProjectBootstrap.PrototypeBootstrap.Run
        public static void Run()
        {
            var config = GetOrCreateConfig();
            var statBlock = GetOrCreateStatBlock();
            var fodder = GetOrCreateEnemy("fodder", "Fodder", EnemyArchetype.Fodder, maxHealth: 6f, moveSpeed: 2.5f, damageOnHit: 2f, timeRewardOnKill: 3f);
            var tank = GetOrCreateEnemy("tank", "Tank", EnemyArchetype.Tank, maxHealth: 25f, moveSpeed: 1.2f, damageOnHit: 4f, timeRewardOnKill: 8f);
            var fast = GetOrCreateEnemy("fast", "Fast", EnemyArchetype.Fast, maxHealth: 4f, moveSpeed: 4.5f, damageOnHit: 3f, timeRewardOnKill: 4f);

            var vigorNode = GetOrCreateUpgrade("vigor_up", "Vigor Up", timeCost: 6f, StatType.Vigor, 10f, ModifierType.Flat);
            var siegaNode = GetOrCreateUpgrade("siega_up", "Siega Up", timeCost: 8f, StatType.Siega, 0.25f, ModifierType.Percent);
            var premuraNode = GetOrCreateUpgrade("premura_up", "Premura Up", timeCost: 7f, StatType.Premura, 0.2f, ModifierType.Percent);

            BuildScene(config, statBlock, fodder, tank, fast, vigorNode, siegaNode, premuraNode);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[PrototypeBootstrap] Done: data assets created, SampleScene wired with the full loop for Play mode.");
            EditorApplication.Exit(0);
        }

        static TimeEconomyConfig GetOrCreateConfig()
        {
            const string path = DataFolder + "/TimeEconomyConfig.asset";
            var existing = AssetDatabase.LoadAssetAtPath<TimeEconomyConfig>(path);
            if (existing != null) return existing;

            var config = ScriptableObject.CreateInstance<TimeEconomyConfig>();
            config.startingTimeCapacity = 25f;
            config.passiveDrainPerSecond = 0.6f;
            config.hitTimeCost = 2f;
            config.lowTimeWarningThreshold = 6f;

            AssetDatabase.CreateAsset(config, path);
            return config;
        }

        static StatBlock GetOrCreateStatBlock()
        {
            const string path = DataFolder + "/StatBlock.asset";
            var existing = AssetDatabase.LoadAssetAtPath<StatBlock>(path);
            if (existing != null) return existing;

            var stats = ScriptableObject.CreateInstance<StatBlock>();
            stats.baseVigor = 25f;
            stats.baseSiega = 1f;
            stats.basePremura = 1f;

            AssetDatabase.CreateAsset(stats, path);
            return stats;
        }

        static EnemyDefinition GetOrCreateEnemy(string id, string displayName, EnemyArchetype archetype,
            float maxHealth, float moveSpeed, float damageOnHit, float timeRewardOnKill)
        {
            string path = $"{EnemiesFolder}/{id}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
            if (existing != null) return existing;

            var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            enemy.enemyId = id;
            enemy.displayName = displayName;
            enemy.archetype = archetype;
            enemy.maxHealth = maxHealth;
            enemy.moveSpeed = moveSpeed;
            enemy.damageOnHit = damageOnHit;
            enemy.timeRewardOnKill = timeRewardOnKill;

            AssetDatabase.CreateAsset(enemy, path);
            return enemy;
        }

        static UpgradeNodeDefinition GetOrCreateUpgrade(string id, string displayName, float timeCost,
            StatType stat, float value, ModifierType modifierType)
        {
            string path = $"{UpgradesFolder}/{id}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<UpgradeNodeDefinition>(path);
            if (existing != null) return existing;

            var node = ScriptableObject.CreateInstance<UpgradeNodeDefinition>();
            node.nodeId = id;
            node.displayName = displayName;
            node.timeCost = timeCost;
            node.statModifiers.Add(new StatModifierData { stat = stat, type = modifierType, value = value });

            AssetDatabase.CreateAsset(node, path);
            return node;
        }

        static void BuildScene(TimeEconomyConfig config, StatBlock statBlock,
            EnemyDefinition fodder, EnemyDefinition tank, EnemyDefinition fast,
            UpgradeNodeDefinition vigorNode, UpgradeNodeDefinition siegaNode, UpgradeNodeDefinition premuraNode)
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // Wipe anything we previously added on an earlier bootstrap run, keep Main Camera.
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == "Main Camera") continue;
                Object.DestroyImmediate(root);
            }

            var mainCamera = GameObject.Find("Main Camera");
            if (mainCamera != null)
            {
                mainCamera.transform.position = new Vector3(0f, 0f, -10f);
                var cam = mainCamera.GetComponent<Camera>();
                if (cam != null) { cam.orthographic = true; cam.orthographicSize = 6f; }
            }

            // --- Player ---
            var player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = Vector3.zero;

            var playerSr = player.AddComponent<SpriteRenderer>();
            playerSr.sprite = ProceduralSprite.Square();
            playerSr.color = new Color(0.2f, 0.6f, 1f);

            var playerRb = player.AddComponent<Rigidbody2D>();
            playerRb.gravityScale = 0f;
            playerRb.freezeRotation = true;

            player.AddComponent<CircleCollider2D>();
            player.AddComponent<PlayerController>();

            // --- Time economy ---
            var economyGo = new GameObject("TimeEconomy");
            var economy = economyGo.AddComponent<TimeEconomyController>();
            economy.AssignConfig(config);
            economyGo.AddComponent<TimeEconomyRunner>();
            economyGo.AddComponent<GameHUD>();

            // --- Waves ---
            var waveGo = new GameObject("WaveManager");
            var waveManager = waveGo.AddComponent<WaveManager>();
            waveManager.fodder = fodder;
            waveManager.tank = tank;
            waveManager.fast = fast;
            waveManager.spawnRadius = 6.5f;

            // --- Run loop orchestrator (must be created after WaveManager so it can reference it) ---
            var runGo = new GameObject("RunManager");
            var runManager = runGo.AddComponent<RunManager>();
            runManager.statBlock = statBlock;
            runManager.waveManager = waveManager;
            runManager.altarPool.Add(vigorNode);
            runManager.altarPool.Add(siegaNode);
            runManager.altarPool.Add(premuraNode);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
