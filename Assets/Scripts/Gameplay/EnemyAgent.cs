using ChronoBlade.Data;
using UnityEngine;

namespace ChronoBlade.Gameplay
{
    // Minimal enemy: chases the player, drains time on contact (with a cooldown so it doesn't
    // melt the clock every physics tick), and rewards time when killed via EnemyDefinition data.
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyAgent : MonoBehaviour
    {
        public EnemyDefinition definition;

        const float HitCooldown = 0.75f;

        Rigidbody2D _rb;
        Transform _player;
        TimeEconomyController _economy;
        float _lastHitTime = -999f;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _economy = FindFirstObjectByType<TimeEconomyController>();
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) _player = playerObj.transform;
        }

        void FixedUpdate()
        {
            if (_player == null || definition == null) return;

            Vector2 direction = (_player.position - transform.position);
            if (direction.sqrMagnitude > 0.01f)
            {
                _rb.linearVelocity = direction.normalized * definition.moveSpeed;
            }
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            if (!collision.collider.CompareTag("Player")) return;
            if (Time.time - _lastHitTime < HitCooldown) return;

            _lastHitTime = Time.time;
            _economy?.RegisterHit(definition);
        }

        public void Kill()
        {
            var sr = GetComponent<SpriteRenderer>();
            DeathBurst.Spawn(transform.position, sr != null ? sr.color : Color.white);
            RunManager.Instance?.RegisterKill(definition);
            Destroy(gameObject);
        }
    }
}
