using UnityEngine;

namespace ChronoBlade.Gameplay
{
    // Fades and destroys a one-shot visual effect object (see PlayerController.SpawnAttackFlash).
    public class AttackFlashFade : MonoBehaviour
    {
        public float lifetime = 0.15f;

        float _elapsed;
        SpriteRenderer _sr;

        void Awake() => _sr = GetComponent<SpriteRenderer>();

        void Update()
        {
            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / lifetime);

            if (_sr != null)
            {
                var c = _sr.color;
                c.a = Mathf.Lerp(0.6f, 0f, t);
                _sr.color = c;
            }

            if (t >= 1f) Destroy(gameObject);
        }
    }
}
