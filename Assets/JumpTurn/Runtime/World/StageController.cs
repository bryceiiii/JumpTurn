using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JumpTurn
{
    public sealed class StageController : MonoBehaviour
    {
        private readonly List<PhaseSurface> _phaseSurfaces = new();
        private Transform _rotatingRoot;
        private PlayerController _player;
        private JumpTurnGame _game;
        private RoomSpec _room;
        private int _quarterTurns;
        private bool _busy;

        public StagePhase ActivePhase { get; private set; }
        public bool IsBusy => _busy;
        public event Action<StagePhase> PhaseChanged;
        public event Action<int> RotationCompleted;
        public event Action Blocked;

        public void Initialize(JumpTurnGame game, PlayerController player, RoomSpec room, Transform rotatingRoot, IEnumerable<PhaseSurface> phaseSurfaces)
        {
            _game = game;
            _player = player;
            _room = room;
            _rotatingRoot = rotatingRoot;
            _phaseSurfaces.Clear();
            if (phaseSurfaces != null)
            {
                _phaseSurfaces.AddRange(phaseSurfaces);
            }

            ResetRoom();
        }

        public bool TryRotate(RotationDirection direction)
        {
            if (_busy || _game == null || _game.State != GameRunState.Playing)
            {
                return false;
            }

            StartCoroutine(RotateRoutine(direction));
            return true;
        }

        public bool TryTogglePhase()
        {
            if (_busy || _game == null || _game.State != GameRunState.Playing)
            {
                return false;
            }

            StagePhase next = ActivePhase == StagePhase.Positive ? StagePhase.Negative : StagePhase.Positive;
            StagePhase previous = ActivePhase;
            ApplyPhase(next);
            Physics2D.SyncTransforms();
            if (_player.IsOverlappingStage())
            {
                ApplyPhase(previous);
                Physics2D.SyncTransforms();
                _game.Audio.Play(SfxId.Blocked);
                Blocked?.Invoke();
                _game.NotifyBlocked("这里切换会卡住");
                return false;
            }

            _game.Audio.Play(SfxId.Phase);
            PhaseChanged?.Invoke(next);
            return true;
        }

        public void ResetRoom()
        {
            StopAllCoroutines();
            _busy = false;
            _quarterTurns = _room.startQuarterTurns;
            if (_rotatingRoot != null)
            {
                _rotatingRoot.localRotation = Quaternion.Euler(0f, 0f, QuarterTurn.ToDegrees(_quarterTurns));
            }

            ApplyPhase(_room.startPhase);
            Physics2D.SyncTransforms();
            if (_player != null)
            {
                _player.SetRotationLock(false);
            }
        }

        private IEnumerator RotateRoutine(RotationDirection direction)
        {
            _busy = true;
            _player.SetRotationLock(true);
            _game.SetStageInputLocked(true);
            _game.Audio.Play(SfxId.Rotate);

            float from = QuarterTurn.ToDegrees(_quarterTurns);
            float to = from + (int)direction * 90f;
            yield return AnimateRotation(from, to, 0.28f);

            if (_player.IsOverlappingStage())
            {
                _game.Audio.Play(SfxId.Blocked);
                Blocked?.Invoke();
                _game.NotifyBlocked("回廊受阻");
                yield return AnimateRotation(to, from, 0.18f);
            }
            else
            {
                _quarterTurns = QuarterTurn.Add(_quarterTurns, (int)direction);
                RotationCompleted?.Invoke(_quarterTurns);
            }

            _player.SetRotationLock(false);
            _game.SetStageInputLocked(false);
            _busy = false;
        }

        private IEnumerator AnimateRotation(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float smooth = t * t * (3f - 2f * t);
                float angle = Mathf.Lerp(from, to, smooth);
                _rotatingRoot.localRotation = Quaternion.Euler(0f, 0f, angle);
                yield return null;
            }

            _rotatingRoot.localRotation = Quaternion.Euler(0f, 0f, to);
            Physics2D.SyncTransforms();
        }

        private void ApplyPhase(StagePhase phase)
        {
            ActivePhase = phase;
            foreach (PhaseSurface surface in _phaseSurfaces)
            {
                if (surface != null)
                {
                    surface.Apply(PhaseRules.IsColliderEnabled(surface.Phase, phase));
                }
            }
        }
    }
}
