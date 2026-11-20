using UnityEngine;

namespace JumpTurn
{
    public sealed class SpriteAnimator2D : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        private Sprite[] _frames;
        private float _framesPerSecond = 10f;
        private bool _loop = true;
        private float _startTime;

        public void Initialize(SpriteRenderer target, Sprite[] frames, float framesPerSecond, bool loop = true)
        {
            _renderer = target;
            Play(frames, framesPerSecond, loop);
        }

        public void Play(Sprite[] frames, float framesPerSecond, bool loop = true)
        {
            if (frames == null || frames.Length == 0 || _renderer == null)
            {
                return;
            }

            _frames = frames;
            _framesPerSecond = Mathf.Max(0.01f, framesPerSecond);
            _loop = loop;
            _startTime = Time.time;
            _renderer.sprite = _frames[0];
        }

        public void SetFacing(float horizontalDirection)
        {
            if (_renderer == null || Mathf.Abs(horizontalDirection) < 0.01f)
            {
                return;
            }

            _renderer.flipX = horizontalDirection > 0f;
        }

        public void SetFacingLeft(bool left)
        {
            if (_renderer != null)
            {
                _renderer.flipX = !left;
            }
        }

        private void Update()
        {
            if (_renderer == null || _frames == null || _frames.Length == 0)
            {
                return;
            }

            int frame = Mathf.FloorToInt((Time.time - _startTime) * _framesPerSecond);
            if (_loop)
            {
                frame %= _frames.Length;
            }
            else
            {
                frame = Mathf.Min(frame, _frames.Length - 1);
            }

            _renderer.sprite = _frames[frame];
        }
    }
}
