using UnityEngine;

namespace JumpTurn
{
    public sealed class PlayerController : MonoBehaviour
    {
        private const float MoveSpeed = 7.4f;
        private const float GroundAcceleration = 78f;
        private const float AirAcceleration = 46f;
        private const float JumpSpeed = 17.5f;
        private const float CoyoteTime = 0.12f;
        private const float JumpBufferTime = 0.12f;

        private GameInput _input;
        private JumpTurnGame _game;
        private Rigidbody2D _body;
        private BoxCollider2D _collider;
        private SpriteRenderer _renderer;
        private SpriteAnimator2D _animator;
        private LayerMask _stageMask;
        private float _lastGroundedTime;
        private float _jumpBufferedUntil;
        private bool _controlLocked;
        private bool _wasGrounded;
        private AnimationState _animationState;
        private float _facing = 1f;

        public bool IsGrounded { get; private set; }
        public Rigidbody2D Body => _body;
        /// <summary>1 表示朝右，-1 表示朝左。</summary>
        public float Facing => _facing;

        private enum AnimationState
        {
            Idle,
            Run,
            Jump,
            Fall
        }

        public void Initialize(GameInput input, JumpTurnGame game)
        {
            _input = input;
            _game = game;
            _stageMask = LayerMask.GetMask("Stage");
            gameObject.layer = LayerMask.NameToLayer("Player");

            _body = gameObject.AddComponent<Rigidbody2D>();
            _body.gravityScale = 2f;
            _body.freezeRotation = true;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _body.interpolation = RigidbodyInterpolation2D.Interpolate;

            _collider = gameObject.AddComponent<BoxCollider2D>();
            _collider.size = new Vector2(0.76f, 1.42f);
            _collider.offset = new Vector2(0f, -0.36f);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(transform, false);
            visual.transform.localPosition = new Vector3(0f, -0.12f, 0f);
            visual.transform.localScale = new Vector3(1.9f, 1.9f, 1f);
            _renderer = visual.AddComponent<SpriteRenderer>();
            // 走本色：MonoTint 只取 alpha，会把这套 1-bit 素材的黑描边和眼睛一起压成纯白剪影。
            _renderer.sharedMaterial = RuntimeArt.SpriteUnlitMaterial;
            _renderer.sortingOrder = 6;
            _animator = gameObject.AddComponent<SpriteAnimator2D>();
            _animator.Initialize(_renderer, RuntimeArt.GetBunnyIdle(), 9f);

            var shadow = new GameObject("Shadow");
            shadow.transform.SetParent(transform, false);
            shadow.transform.localPosition = new Vector3(0f, -1.02f, 0f);
            shadow.transform.localScale = new Vector3(0.9f, 0.16f, 1f);
            SpriteRenderer shadowRenderer = shadow.AddComponent<SpriteRenderer>();
            shadowRenderer.sprite = RuntimeArt.WhiteSprite;
            shadowRenderer.color = new Color(0f, 0f, 0f, 0.28f);
            shadowRenderer.sortingOrder = 1;

            var fairyObject = new GameObject("FairyCompanion");
            fairyObject.transform.SetParent(transform.parent, false);
            fairyObject.AddComponent<FairyCompanion>().Initialize(transform);
        }

        public void SetControlLocked(bool locked)
        {
            _controlLocked = locked;
            if (!locked || _body == null)
            {
                return;
            }

            _body.linearVelocity = Vector2.zero;
        }

        public void SetRotationLock(bool locked)
        {
            _controlLocked = locked;
            if (_body == null || _collider == null)
            {
                return;
            }

            _body.linearVelocity = Vector2.zero;
            _collider.enabled = !locked;
            _body.simulated = !locked;
        }

        public void Respawn(Vector2 position)
        {
            transform.position = position;
            gameObject.SetActive(true);
            if (_collider != null)
            {
                _collider.enabled = true;
            }

            if (_body != null)
            {
                _body.simulated = true;
                _body.linearVelocity = Vector2.zero;
                _body.angularVelocity = 0f;
            }

            _controlLocked = false;
            _lastGroundedTime = -10f;
            _jumpBufferedUntil = -10f;
            _animationState = AnimationState.Idle;
        }

        public void QueueJump()
        {
            if (_game != null && _game.State == GameRunState.Playing && !_controlLocked)
            {
                _jumpBufferedUntil = Time.time + JumpBufferTime;
            }
        }

        public bool IsOverlappingStage()
        {
            if (_collider == null || !_collider.enabled)
            {
                Vector2 center = (Vector2)transform.position + new Vector2(0f, -0.36f);
                return Physics2D.OverlapBox(center, new Vector2(0.7f, 1.34f), 0f, _stageMask) != null;
            }

            Bounds bounds = _collider.bounds;
            return Physics2D.OverlapBox(bounds.center, bounds.size * 0.92f, 0f, _stageMask) != null;
        }

        public void Kill(DeathCause cause)
        {
            if (_game == null || _game.State != GameRunState.Playing)
            {
                return;
            }

            _game.HandlePlayerDeath(cause);
        }

        private void Update()
        {
            if (_input == null || _game == null)
            {
                return;
            }

            if (_input.JumpPressed)
            {
                QueueJump();
            }

            UpdateAnimation();
            if (transform.position.y < -13.5f && _game.State == GameRunState.Playing)
            {
                Kill(DeathCause.Fall);
            }
        }

        private void FixedUpdate()
        {
            if (_body == null || _game == null || _game.State != GameRunState.Playing || _controlLocked)
            {
                return;
            }

            Bounds bounds = _collider.bounds;
            Vector2 groundProbeOrigin = new(bounds.center.x, bounds.min.y + 0.04f);
            IsGrounded = Physics2D.BoxCast(
                groundProbeOrigin,
                new Vector2(bounds.size.x * 0.82f, 0.08f),
                0f,
                Vector2.down,
                0.12f,
                _stageMask);
            if (IsGrounded)
            {
                _lastGroundedTime = Time.time;
            }

            float horizontal = Mathf.Clamp(_input.Move.x, -1f, 1f);
            float targetSpeed = horizontal * MoveSpeed;
            float acceleration = IsGrounded ? GroundAcceleration : AirAcceleration;
            float nextX = Mathf.MoveTowards(_body.linearVelocity.x, targetSpeed, acceleration * Time.fixedDeltaTime);
            _body.linearVelocity = new Vector2(nextX, _body.linearVelocity.y);

            if (Mathf.Abs(horizontal) > 0.05f)
            {
                _facing = Mathf.Sign(horizontal);
                _animator.SetFacingLeft(_facing < 0f);
            }

            bool canJump = Time.time <= _lastGroundedTime + CoyoteTime;
            if (Time.time <= _jumpBufferedUntil && canJump)
            {
                _body.linearVelocity = new Vector2(_body.linearVelocity.x, JumpSpeed);
                _jumpBufferedUntil = -10f;
                _lastGroundedTime = -10f;
                _game.Audio.Play(SfxId.Jump);
            }

            if (!_input.JumpHeld && _body.linearVelocity.y > 0f)
            {
                _body.linearVelocity = new Vector2(_body.linearVelocity.x, _body.linearVelocity.y * 0.55f);
            }

            if (!_wasGrounded && IsGrounded && _body.linearVelocity.y <= 0.1f)
            {
                _game.Audio.Play(SfxId.Land);
            }

            _wasGrounded = IsGrounded;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<Hazard2D>() != null)
            {
                Kill(DeathCause.Hazard);
            }
        }

        private void UpdateAnimation()
        {
            if (_animator == null || _body == null)
            {
                return;
            }

            AnimationState next;
            if (!IsGrounded && _body.linearVelocity.y > 0.2f)
            {
                next = AnimationState.Jump;
            }
            else if (!IsGrounded && _body.linearVelocity.y < -0.2f)
            {
                next = AnimationState.Fall;
            }
            else if (Mathf.Abs(_body.linearVelocity.x) > 0.35f)
            {
                next = AnimationState.Run;
            }
            else
            {
                next = AnimationState.Idle;
            }

            if (next == _animationState)
            {
                return;
            }

            _animationState = next;
            switch (next)
            {
                case AnimationState.Run:
                    _animator.Play(RuntimeArt.GetBunnyRun(), 12f);
                    break;
                case AnimationState.Jump:
                    _animator.Play(RuntimeArt.GetBunnyJump(), 10f, false);
                    break;
                case AnimationState.Fall:
                    _animator.Play(RuntimeArt.GetBunnyFall(), 10f, false);
                    break;
                default:
                    _animator.Play(RuntimeArt.GetBunnyIdle(), 8f);
                    break;
            }
        }
    }
}
