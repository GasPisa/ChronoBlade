using UnityEngine;
using UnityEngine.InputSystem;

namespace ChronoBlade.Gameplay
{
    // Minimal top-down player: WASD/stick move, Space/South-button attack (destroys nearby enemies).
    // Placeholder-primitive friendly — no animation/art dependency, just enough to be playable.
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float attackRadius = 1.2f;
        public LayerMask enemyLayer = ~0;

        Rigidbody2D _rb;
        Vector2 _moveInput;
        TimeEconomyController _economy;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _economy = FindFirstObjectByType<TimeEconomyController>();
            transform.localScale = Vector3.one * 1.2f;
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            float x = 0f, y = 0f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) y -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) y += 1f;

            _moveInput = new Vector2(x, y).normalized;

            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                Attack();
            }
        }

        void FixedUpdate()
        {
            float premura = RunManager.Instance != null
                ? RunManager.Instance.Stats.GetValue(Data.StatType.Premura)
                : 1f;
            _rb.linearVelocity = _moveInput * moveSpeed * premura;
        }

        void Attack()
        {
            SpawnAttackFlash();

            var hits = Physics2D.OverlapCircleAll(transform.position, attackRadius, enemyLayer);
            foreach (var hit in hits)
            {
                var enemy = hit.GetComponent<EnemyAgent>();
                if (enemy != null) enemy.Kill();
            }
        }

        // Quick fading ring so the attack has visible feedback even before real VFX exist.
        void SpawnAttackFlash()
        {
            var flash = new GameObject("AttackFlash");
            flash.transform.position = transform.position;
            flash.transform.localScale = Vector3.one * (attackRadius * 2f);

            var sr = flash.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSprite.Square();
            sr.color = new Color(1f, 1f, 0.6f, 0.6f);
            sr.sortingOrder = 10;

            var fade = flash.AddComponent<AttackFlashFade>();
            fade.lifetime = 0.15f;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackRadius);
        }
    }
}
