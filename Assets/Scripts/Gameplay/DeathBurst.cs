using UnityEngine;

namespace ChronoBlade.Gameplay
{
    // Small HDR-colored particle burst so a kill reads as an event, not a silent disappearance.
    // Built entirely from code — no particle asset to author/import.
    public static class DeathBurst
    {
        public static void Spawn(Vector3 position, Color color)
        {
            var go = new GameObject("DeathBurst");
            go.transform.position = position;

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.4f;
            main.loop = false;
            main.startLifetime = 0.35f;
            main.startSpeed = 4f;
            main.startSize = 0.18f;
            main.startColor = color * 1.6f; // HDR-bright so Bloom picks it up
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 10) });
            emission.rateOverTime = 0f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.05f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));

            ps.Play();
        }
    }
}
