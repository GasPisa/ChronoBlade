using ChronoBlade.Data;
using ChronoBlade.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
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
                if (cam != null)
                {
                    cam.orthographic = true;
                    cam.orthographicSize = 6f;
                    cam.clearFlags = CameraClearFlags.SolidColor;
                    cam.backgroundColor = new Color(0.04f, 0.03f, 0.09f); // dark indigo, not flat black
                    cam.allowHDR = true;
                }

                var camData = mainCamera.GetComponent<UniversalAdditionalCameraData>();
                if (camData != null) camData.renderPostProcessing = true;

                foreach (var old in mainCamera.GetComponents<CameraFollow>()) Object.DestroyImmediate(old);
                mainCamera.AddComponent<CameraFollow>();
            }

            // --- Player ---
            var player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = Vector3.zero;

            var playerSr = player.AddComponent<SpriteRenderer>();
            playerSr.sprite = ProceduralSprite.Square();
            playerSr.color = new Color(0.35f, 1.1f, 1.9f); // HDR-bright so Bloom glows it

            var playerRb = player.AddComponent<Rigidbody2D>();
            playerRb.gravityScale = 0f;
            playerRb.freezeRotation = true;

            player.AddComponent<CircleCollider2D>();
            player.AddComponent<PlayerController>();

            var followCam = mainCamera != null ? mainCamera.GetComponent<CameraFollow>() : null;
            if (followCam != null) followCam.target = player.transform;

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

            SetupPostProcessing();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        // Adds a VolumeComponent to the profile AND registers it as a persistent sub-asset —
        // VolumeProfile.Add<T>() alone only lives in memory; without AddObjectToAsset it does
        // not survive a save/reload (which is why it silently vanished by Play mode).
        static T GetOrAddVolumeComponent<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (profile.TryGet<T>(out T existing)) return existing;

            var component = profile.Add<T>(true);
            AssetDatabase.AddObjectToAsset(component, profile);
            return component;
        }

        static void SetupPostProcessing()
        {
            // Always rebuilt fresh (like the scene) rather than loaded-if-exists: a VolumeProfile
            // that already has sub-assets AddObjectToAsset'd into it is fragile to touch again
            // across separate batch invocations, so don't risk a stale/orphaned reference.
            const string profilePath = DataFolder + "/PostProcessProfile.asset";
            AssetDatabase.DeleteAsset(profilePath);

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, profilePath);

            var bloom = GetOrAddVolumeComponent<Bloom>(profile);
            bloom.threshold.overrideState = true; bloom.threshold.value = 1f;
            bloom.intensity.overrideState = true; bloom.intensity.value = 1.4f;
            bloom.scatter.overrideState = true; bloom.scatter.value = 0.7f;

            var vignette = GetOrAddVolumeComponent<Vignette>(profile);
            vignette.intensity.overrideState = true; vignette.intensity.value = 0.35f;
            vignette.smoothness.overrideState = true; vignette.smoothness.value = 0.6f;
            vignette.color.overrideState = true; vignette.color.value = new Color(0.02f, 0f, 0.05f);

            var colorAdjustments = GetOrAddVolumeComponent<ColorAdjustments>(profile);
            colorAdjustments.saturation.overrideState = true; colorAdjustments.saturation.value = 15f;
            colorAdjustments.contrast.overrideState = true; colorAdjustments.contrast.value = 12f;

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            var existingVolumeGo = GameObject.Find("GlobalPostProcess");
            if (existingVolumeGo != null) Object.DestroyImmediate(existingVolumeGo);

            var volumeGo = new GameObject("GlobalPostProcess");
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.weight = 1f;
            volume.sharedProfile = profile; // .profile is a runtime-only copy; this is what serializes
        }
    }
}
