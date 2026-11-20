using System.Collections.Generic;
using UnityEngine;

namespace JumpTurn
{
    public sealed class PhaseSurface : MonoBehaviour
    {
        private readonly List<SpriteRenderer> _renderers = new();
        private readonly List<Color> _baseColors = new();
        private Collider2D _collider;

        public StagePhase Phase { get; private set; }

        public void Configure(StagePhase phase, Collider2D surfaceCollider, IEnumerable<SpriteRenderer> renderers)
        {
            Phase = phase;
            _collider = surfaceCollider;
            _renderers.Clear();
            _baseColors.Clear();
            foreach (SpriteRenderer renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                _renderers.Add(renderer);
                _baseColors.Add(renderer.color);
            }
        }

        public void Apply(bool active)
        {
            if (_collider != null)
            {
                _collider.enabled = active;
            }

            for (int i = 0; i < _renderers.Count; i++)
            {
                SpriteRenderer renderer = _renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                Color color = _baseColors[i];
                color.a = active ? _baseColors[i].a : _baseColors[i].a * 0.16f;
                renderer.color = color;
            }
        }
    }
}
