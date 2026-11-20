using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace JumpTurn.Tests
{
    public sealed class GameFlowPlayModeTests
    {
        [UnitySetUp]
        public IEnumerator LoadDemoScene()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync("JumpTurnDemo", LoadSceneMode.Single);
            yield return null;
        }

        [TearDown]
        public void ResetTime()
        {
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator StartPauseAndResumeFlow()
        {
            JumpTurnGame game = Object.FindFirstObjectByType<JumpTurnGame>();
            Assert.That(game, Is.Not.Null);

            game.StartRun();
            yield return null;
            Assert.That(game.State, Is.EqualTo(GameRunState.Playing));

            game.Pause();
            Assert.That(game.State, Is.EqualTo(GameRunState.Paused));
            Assert.That(Time.timeScale, Is.Zero);

            game.Resume();
            Assert.That(game.State, Is.EqualTo(GameRunState.Playing));
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }

        [UnityTest]
        public IEnumerator PlayerIsGroundedAtRoomStart()
        {
            JumpTurnGame game = Object.FindFirstObjectByType<JumpTurnGame>();
            game.StartRun();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.That(game.Player.IsGrounded, Is.True);
            Assert.That(game.State, Is.EqualTo(GameRunState.Playing));

            game.Player.QueueJump();
            yield return new WaitForFixedUpdate();
            Assert.That(game.Player.Body.linearVelocity.y, Is.GreaterThan(0f));
        }

        [UnityTest]
        public IEnumerator SpaceKeyMakesPlayerJump()
        {
            JumpTurnGame game = Object.FindFirstObjectByType<JumpTurnGame>();
            game.StartRun();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Keyboard keyboard = InputSystem.GetDevice<Keyboard>() ?? InputSystem.AddDevice<Keyboard>();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return new WaitForFixedUpdate();

            Assert.That(game.Player.Body.linearVelocity.y, Is.GreaterThan(0f));
            Assert.That(game.State, Is.EqualTo(GameRunState.Playing));
        }

        [UnityTest]
        public IEnumerator SpikeLaneCanBeCleared()
        {
            JumpTurnGame game = Object.FindFirstObjectByType<JumpTurnGame>();
            game.StartRun();
            yield return null;

            game.HandleExitReached(0, 0);
            yield return new WaitForSecondsRealtime(0.8f);
            game.CollectShard(1);
            game.HandleExitReached(1, 1);
            yield return new WaitForSecondsRealtime(0.8f);
            Assert.That(game.CurrentRoomIndex, Is.EqualTo(2));

            game.Player.Respawn(new Vector2(-2f, -0.58f));
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Keyboard keyboard = InputSystem.GetDevice<Keyboard>() ?? InputSystem.AddDevice<Keyboard>();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D, Key.Space));
            yield return null;
            yield return new WaitForSeconds(0.65f);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;

            Vector3 position = game.Player.transform.position;
            Assert.That(game.State, Is.EqualTo(GameRunState.Playing), $"state={game.State}, x={position.x:F2}, y={position.y:F2}");
            Assert.That(position.x, Is.GreaterThan(1.85f), $"x={position.x:F2}, y={position.y:F2}");
            Assert.That(position.y, Is.GreaterThan(-1.2f), $"x={position.x:F2}, y={position.y:F2}");
        }

        [UnityTest]
        public IEnumerator DeathRespawnsAtCurrentRoom()
        {
            JumpTurnGame game = Object.FindFirstObjectByType<JumpTurnGame>();
            game.StartRun();
            yield return null;

            game.Player.Kill(DeathCause.Fall);
            Assert.That(game.State, Is.EqualTo(GameRunState.Dying));
            yield return new WaitForSecondsRealtime(0.75f);

            Assert.That(game.State, Is.EqualTo(GameRunState.Playing));
            Assert.That(game.Deaths, Is.EqualTo(1));
            Assert.That(game.CurrentRoomIndex, Is.Zero);
        }

        [UnityTest]
        public IEnumerator PhaseToggleKeepsPlayerOutOfSolidGeometry()
        {
            JumpTurnGame game = Object.FindFirstObjectByType<JumpTurnGame>();
            game.StartRun();
            yield return null;

            StagePhase before = game.Stage.ActivePhase;
            game.Stage.TryTogglePhase();
            StagePhase after = game.Stage.ActivePhase;

            Assert.That(after, Is.Not.EqualTo(before));
            Assert.That(game.Player.IsOverlappingStage(), Is.False);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CompleteRunFlowsThroughAllRooms()
        {
            JumpTurnGame game = Object.FindFirstObjectByType<JumpTurnGame>();
            game.StartRun();
            yield return null;

            game.HandleExitReached(0, 0);
            yield return new WaitForSecondsRealtime(0.8f);
            Assert.That(game.CurrentRoomIndex, Is.EqualTo(1));
            Assert.That(game.State, Is.EqualTo(GameRunState.Playing));

            game.CollectShard(1);
            Assert.That(game.ShardsCollected, Is.EqualTo(1));
            game.HandleExitReached(1, 1);
            yield return new WaitForSecondsRealtime(0.8f);
            Assert.That(game.CurrentRoomIndex, Is.EqualTo(2));

            game.CollectShard(2);
            game.HandleExitReached(2, 2);
            yield return new WaitForSecondsRealtime(0.8f);
            Assert.That(game.CurrentRoomIndex, Is.EqualTo(3));

            game.CollectShard(3);
            yield return null;
            game.HandleExitReached(3, 3);
            yield return null;

            Assert.That(game.ShardsCollected, Is.EqualTo(3));
            Assert.That(game.State, Is.EqualTo(GameRunState.Completed));
            Assert.That(PlayerPrefs.HasKey("JumpTurn.BestTimeSeconds"), Is.True);
        }
    }
}
