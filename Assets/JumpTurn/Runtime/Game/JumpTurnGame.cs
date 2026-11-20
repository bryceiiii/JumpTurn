using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JumpTurn
{
    public sealed class JumpTurnGame : MonoBehaviour
    {
        private const string BestTimeKey = "JumpTurn.BestTimeSeconds";
        private readonly HashSet<int> _collectedShards = new();
        private GameInput _input;
        private PlayerController _player;
        private RoomView _room;
        private Camera _camera;
        private Coroutine _shakeRoutine;
        private float _elapsedSeconds;
        private int _deaths;
        private int _currentRoom;
        private float _bestSeconds;
        private bool _stageInputLocked;
        private bool _transitioning;

        public GameRunState State { get; private set; } = GameRunState.Title;
        public int ShardsCollected => _collectedShards.Count;
        public int Deaths => _deaths;
        public int CurrentRoomIndex => _currentRoom;
        public float ElapsedSeconds => _elapsedSeconds;
        public PlayerController Player => _player;
        public StageController Stage => _room?.Stage;
        public ProceduralAudio Audio { get; private set; }
        public GameUI UI { get; private set; }

        public void Initialize(Camera worldCamera)
        {
            _camera = worldCamera;
            _bestSeconds = PlayerPrefs.GetFloat(BestTimeKey, 0f);
            Physics2D.gravity = new Vector2(0f, -28f);
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 1;

            _input = new GameInput();
            Audio = gameObject.AddComponent<ProceduralAudio>();
            Audio.Initialize();
            UI = gameObject.AddComponent<GameUI>();
            UI.Initialize(this);

            var playerObject = new GameObject("Player");
            playerObject.transform.SetParent(transform, false);
            _player = playerObject.AddComponent<PlayerController>();
            _player.Initialize(_input, this);

            LoadRoom(0);
            SetState(GameRunState.Title);
        }

        public void StartRun()
        {
            StopAllCoroutines();
            _transitioning = false;
            Time.timeScale = 1f;
            _elapsedSeconds = 0f;
            _deaths = 0;
            _currentRoom = 0;
            _stageInputLocked = false;
            _collectedShards.Clear();
            UI.HideCompletion();
            UI.ShowDeath(false);
            UI.ShowPause(false);
            UI.ShowTitle(false);
            UI.ShowHud(true);
            UI.SetFade(0f);
            LoadRoom(0);
            SetState(GameRunState.Playing);
            RefreshUi();
        }

        public void Pause()
        {
            if (State != GameRunState.Playing || _transitioning)
            {
                return;
            }

            _player.SetControlLocked(true);
            SetState(GameRunState.Paused);
        }

        public void Resume()
        {
            if (State != GameRunState.Paused)
            {
                return;
            }

            SetState(GameRunState.Playing);
            _player.SetControlLocked(_stageInputLocked);
        }

        public void RestartRun()
        {
            StartRun();
        }

        public void ReturnToTitle()
        {
            StopAllCoroutines();
            Time.timeScale = 1f;
            _transitioning = false;
            _stageInputLocked = false;
            _collectedShards.Clear();
            _elapsedSeconds = 0f;
            _deaths = 0;
            _currentRoom = 0;
            UI.HideCompletion();
            UI.ShowDeath(false);
            UI.ShowPause(false);
            UI.SetFade(0f);
            LoadRoom(0);
            SetState(GameRunState.Title);
        }

        public void SetStageInputLocked(bool locked)
        {
            _stageInputLocked = locked;
        }

        public bool HasShard(int id)
        {
            return _collectedShards.Contains(id);
        }

        public void CollectShard(int id)
        {
            if (State != GameRunState.Playing || !_collectedShards.Add(id))
            {
                return;
            }

            Audio.Play(SfxId.Collect);
            ShakeCamera(0.08f, 0.12f);
            _room?.Exit?.Refresh(ShardsCollected);
            RefreshUi();
        }

        public void NotifyBlocked(string message)
        {
            UI.ShowBlocked(message);
            ShakeCamera(0.05f, 0.08f);
        }

        public void HandlePlayerDeath(DeathCause cause)
        {
            if (State != GameRunState.Playing || _transitioning)
            {
                return;
            }

            _deaths++;
            _player.SetRotationLock(true);
            Audio.Play(SfxId.Death);
            ShakeCamera(0.2f, 0.28f);
            SetState(GameRunState.Dying);
            RefreshUi();
            StartCoroutine(DeathRoutine());
        }

        public void HandleExitReached(int roomIndex, int requiredShards)
        {
            if (State != GameRunState.Playing || _transitioning || roomIndex != _currentRoom)
            {
                return;
            }

            if (ShardsCollected < requiredShards)
            {
                Audio.Play(SfxId.Blocked);
                NotifyBlocked("天空碎片还不够");
                return;
            }

            if (_currentRoom < RoomCatalog.Count - 1)
            {
                StartCoroutine(TransitionRoutine(_currentRoom + 1));
            }
            else
            {
                CompleteRun();
            }
        }

        private void Update()
        {
            if (_input == null)
            {
                return;
            }

            switch (State)
            {
                case GameRunState.Title:
                    if (_input.JumpPressed)
                    {
                        StartRun();
                    }
                    break;
                case GameRunState.Playing:
                    if (_input.PausePressed)
                    {
                        Pause();
                        break;
                    }

                    if (_input.RestartPressed)
                    {
                        RestartRun();
                        break;
                    }

                    if (!_stageInputLocked && !_transitioning && Stage != null)
                    {
                        if (_input.RotateLeftPressed)
                        {
                            Stage.TryRotate(RotationDirection.CounterClockwise);
                        }
                        else if (_input.RotateRightPressed)
                        {
                            Stage.TryRotate(RotationDirection.Clockwise);
                        }
                        else if (_input.PhasePressed)
                        {
                            Stage.TryTogglePhase();
                            RefreshUi();
                        }
                    }

                    if (!_transitioning)
                    {
                        _elapsedSeconds += Time.deltaTime;
                    }
                    break;
                case GameRunState.Paused:
                    if (_input.PausePressed)
                    {
                        Resume();
                    }
                    else if (_input.RestartPressed)
                    {
                        RestartRun();
                    }
                    break;
                case GameRunState.Completed:
                    if (_input.JumpPressed)
                    {
                        RestartRun();
                    }
                    else if (_input.PausePressed)
                    {
                        ReturnToTitle();
                    }
                    break;
            }

            RefreshUi();
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            _input?.Dispose();
        }

        private void LoadRoom(int index)
        {
            _currentRoom = Mathf.Clamp(index, 0, RoomCatalog.Count - 1);
            if (_room?.Root != null)
            {
                Destroy(_room.Root);
            }

            RoomSpec room = RoomCatalog.Get(_currentRoom);
            _room = RoomBuilder.Build(this, _player, room, transform);
            _player.Respawn(room.spawn);
            _player.SetControlLocked(State == GameRunState.Title || State == GameRunState.Paused || State == GameRunState.Completed);
            RefreshUi();
        }

        private IEnumerator DeathRoutine()
        {
            yield return new WaitForSecondsRealtime(0.55f);
            if (State != GameRunState.Dying)
            {
                yield break;
            }

            LoadRoom(_currentRoom);
            UI.ShowDeath(false);
            SetState(GameRunState.Playing);
            _player.SetControlLocked(false);
            RefreshUi();
        }

        private IEnumerator TransitionRoutine(int nextRoom)
        {
            if (_transitioning)
            {
                yield break;
            }

            _transitioning = true;
            _stageInputLocked = true;
            _player.SetControlLocked(true);
            yield return FadeTo(1f, 0.2f);
            LoadRoom(nextRoom);
            yield return new WaitForSecondsRealtime(0.08f);
            yield return FadeTo(0f, 0.24f);
            _stageInputLocked = false;
            _transitioning = false;
            _player.SetControlLocked(false);
            SetState(GameRunState.Playing);
            RefreshUi();
        }

        private IEnumerator FadeTo(float target, float duration)
        {
            float start = target > 0.5f ? 0f : 1f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                UI.SetFade(Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration)));
                yield return null;
            }

            UI.SetFade(target);
        }

        private void CompleteRun()
        {
            SetState(GameRunState.Completed);
            bool newBest = _bestSeconds <= 0f || _elapsedSeconds < _bestSeconds;
            if (newBest)
            {
                _bestSeconds = _elapsedSeconds;
                PlayerPrefs.SetFloat(BestTimeKey, _bestSeconds);
                PlayerPrefs.Save();
            }

            Audio.Play(SfxId.Complete);
            UI.ShowCompletion(_elapsedSeconds, _bestSeconds, newBest);
            RefreshUi();
        }

        private void SetState(GameRunState next)
        {
            State = next;
            Time.timeScale = next == GameRunState.Paused ? 0f : 1f;
            UI?.ShowTitle(next == GameRunState.Title);
            UI?.ShowHud(next != GameRunState.Title);
            UI?.ShowPause(next == GameRunState.Paused);
            UI?.ShowDeath(next == GameRunState.Dying);
            if (next == GameRunState.Title)
            {
                UI?.HideCompletion();
                _player?.SetControlLocked(true);
            }
            else if (next == GameRunState.Playing)
            {
                _player?.SetControlLocked(_stageInputLocked);
            }
        }

        private void RefreshUi()
        {
            if (UI == null)
            {
                return;
            }

            RoomSpec room = RoomCatalog.Get(_currentRoom);
            int required = ShardRules.RequiredForRoom(_currentRoom);
            UI.SetSnapshot(new GameSnapshot(
                State,
                room.name,
                _elapsedSeconds,
                _deaths,
                ShardsCollected,
                Stage != null ? Stage.ActivePhase : room.startPhase,
                room.hint,
                _bestSeconds,
                ShardsCollected >= required));
        }

        private void ShakeCamera(float duration, float strength)
        {
            if (_camera == null)
            {
                return;
            }

            if (_shakeRoutine != null)
            {
                StopCoroutine(_shakeRoutine);
            }

            _shakeRoutine = StartCoroutine(ShakeRoutine(duration, strength));
        }

        private IEnumerator ShakeRoutine(float duration, float strength)
        {
            Vector3 origin = _camera.transform.position;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float remaining = 1f - Mathf.Clamp01(elapsed / duration);
                Vector2 offset = Random.insideUnitCircle * strength * remaining;
                _camera.transform.position = origin + new Vector3(offset.x, offset.y, 0f);
                yield return null;
            }

            _camera.transform.position = origin;
            _shakeRoutine = null;
        }
    }
}
