using UnityEngine;

namespace JumpTurn
{
    public sealed class Hazard2D : MonoBehaviour
    {
    }

    public sealed class MovingHazard : MonoBehaviour
    {
        private Vector3 _origin;
        private Vector3 _travel;
        private float _speed;
        private float _phaseOffset;
        private bool _spin;
        private float _spinSpeed;

        public void Configure(Vector2 travel, float speed, bool spin)
        {
            _origin = transform.localPosition;
            _travel = travel;
            _speed = Mathf.Max(0.1f, speed);
            _phaseOffset = Mathf.Repeat(GetInstanceID() * 0.017f, Mathf.PI * 2f);
            _spin = spin;
            _spinSpeed = spin ? 210f : 0f;
        }

        private void Update()
        {
            float wave = (Mathf.Sin(Time.time * _speed + _phaseOffset) + 1f) * 0.5f;
            transform.localPosition = _origin + _travel * wave;
            if (_spin)
            {
                transform.Rotate(0f, 0f, _spinSpeed * Time.deltaTime);
            }
        }
    }

    public sealed class SkyShardPickup : MonoBehaviour
    {
        private Transform _visual;
        private JumpTurnGame _game;
        private int _id;
        private float _baseY;
        private bool _collected;

        public void Initialize(JumpTurnGame game, int id)
        {
            _game = game;
            _id = id;
            _visual = new GameObject("ShardVisual").transform;
            _visual.SetParent(transform, false);
            _baseY = _visual.localPosition.y;

            SpriteRenderer glow = _visual.gameObject.AddComponent<SpriteRenderer>();
            glow.sprite = RuntimeArt.GlowSprite;
            glow.color = new Color(1f, 0.78f, 0.25f, 0.55f);
            glow.sortingOrder = 5;
            glow.transform.localScale = Vector3.one * 1.2f;

            var starObject = new GameObject("Star");
            starObject.transform.SetParent(_visual, false);
            SpriteRenderer star = starObject.AddComponent<SpriteRenderer>();
            star.sprite = RuntimeArt.StarSprite;
            star.color = Color.white;
            star.sortingOrder = 6;
            star.transform.localScale = Vector3.one * 1.25f;

            var trigger = gameObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(0.9f, 0.9f);
            gameObject.layer = LayerMask.NameToLayer("Collectible");
        }

        private void Update()
        {
            if (_collected || _visual == null)
            {
                return;
            }

            float bob = Mathf.Sin(Time.time * 3.2f + _id) * 0.13f;
            _visual.localPosition = new Vector3(0f, _baseY + bob, 0f);
            _visual.Rotate(0f, 0f, 28f * Time.deltaTime);
            float pulse = 1f + Mathf.Sin(Time.time * 4.5f) * 0.12f;
            _visual.localScale = Vector3.one * pulse;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected || other.GetComponent<PlayerController>() == null)
            {
                return;
            }

            _collected = true;
            _game.CollectShard(_id);
            Destroy(gameObject);
        }
    }

    public sealed class ExitPortal : MonoBehaviour
    {
        private JumpTurnGame _game;
        private SpriteRenderer _renderer;
        private SpriteRenderer _glow;
        private int _roomIndex;
        private int _requiredShards;

        public bool Unlocked { get; private set; }

        public void Initialize(JumpTurnGame game, int roomIndex, int requiredShards)
        {
            _game = game;
            _roomIndex = roomIndex;
            _requiredShards = requiredShards;

            _glow = new GameObject("Glow").AddComponent<SpriteRenderer>();
            _glow.transform.SetParent(transform, false);
            _glow.sprite = RuntimeArt.GlowSprite;
            _glow.sortingOrder = 3;
            _glow.transform.localScale = Vector3.one * 2.4f;

            _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sprite = RuntimeArt.GetExit();
            _renderer.sharedMaterial = RuntimeArt.MonoTintMaterial;
            _renderer.sortingOrder = 4;
            _renderer.transform.localScale = Vector3.one * 0.78f;

            var trigger = gameObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1.4f, 1.8f);
            gameObject.layer = LayerMask.NameToLayer("Collectible");
            Refresh(game.ShardsCollected);
        }

        public void Refresh(int shardsCollected)
        {
            Unlocked = shardsCollected >= _requiredShards;
            Color color = Unlocked ? new Color(1f, 0.86f, 0.28f, 1f) : new Color(0.42f, 0.47f, 0.53f, 0.75f);
            _renderer.color = color;
            _glow.color = Unlocked ? new Color(1f, 0.7f, 0.2f, 0.38f) : new Color(0.4f, 0.45f, 0.6f, 0.12f);
        }

        private void Update()
        {
            if (_glow == null)
            {
                return;
            }

            float pulse = Unlocked ? 2.1f + Mathf.Sin(Time.time * 4f) * 0.3f : 1.7f;
            _glow.transform.localScale = Vector3.one * pulse;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerController>() == null)
            {
                return;
            }

            _game.HandleExitReached(_roomIndex, _requiredShards);
        }
    }

    public sealed class BackdropCleanup : MonoBehaviour
    {
        private SpriteRenderer _renderer;

        public void Initialize(SpriteRenderer renderer)
        {
            _renderer = renderer;
        }

        private void OnDestroy()
        {
            RuntimeArt.DestroyGeneratedSprite(_renderer);
        }
    }

    public sealed class FairyCompanion : MonoBehaviour
    {
        private Transform _target;
        private SpriteRenderer _renderer;
        private float _seed;

        public void Initialize(Transform target)
        {
            _target = target;
            _seed = 2.4f;
            _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sharedMaterial = RuntimeArt.MonoTintMaterial;
            _renderer.color = new Color(0.45f, 0.95f, 0.9f, 0.82f);
            _renderer.sortingOrder = 4;
            var animator = gameObject.AddComponent<SpriteAnimator2D>();
            animator.Initialize(_renderer, RuntimeArt.GetBird(), 10f);
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            Vector3 desired = _target.position + new Vector3(0.9f + Mathf.Sin(Time.time * 2.4f + _seed) * 0.16f, 0.85f + Mathf.Sin(Time.time * 3.1f + _seed) * 0.18f, 0f);
            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-8f * Time.deltaTime));
            _renderer.flipX = _target.localScale.x < 0f;
        }
    }
}
