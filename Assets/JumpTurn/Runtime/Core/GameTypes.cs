using System;
using UnityEngine;

namespace JumpTurn
{
    public enum GameRunState
    {
        Title,
        Playing,
        Paused,
        Dying,
        Completed
    }

    public enum StagePhase
    {
        Positive,
        Negative
    }

    public enum RotationDirection
    {
        CounterClockwise = 1,
        Clockwise = -1
    }

    public enum DeathCause
    {
        Hazard,
        Fall,
        Crush
    }

    public enum SurfaceStyle
    {
        Stone,
        Moss,
        Coral,
        Teal
    }

    public enum HazardKind
    {
        Spikes,
        Patrol,
        Saw
    }

    public readonly struct GameSnapshot
    {
        public GameSnapshot(
            GameRunState state,
            string roomName,
            float elapsedSeconds,
            int deaths,
            int shards,
            StagePhase phase,
            string hint,
            float bestSeconds,
            bool roomUnlocked)
        {
            State = state;
            RoomName = roomName;
            ElapsedSeconds = elapsedSeconds;
            Deaths = deaths;
            Shards = shards;
            Phase = phase;
            Hint = hint;
            BestSeconds = bestSeconds;
            RoomUnlocked = roomUnlocked;
        }

        public GameRunState State { get; }
        public string RoomName { get; }
        public float ElapsedSeconds { get; }
        public int Deaths { get; }
        public int Shards { get; }
        public StagePhase Phase { get; }
        public string Hint { get; }
        public float BestSeconds { get; }
        public bool RoomUnlocked { get; }
    }

    public static class QuarterTurn
    {
        public static int Add(int current, int delta)
        {
            int value = (current + delta) % 4;
            return value < 0 ? value + 4 : value;
        }

        public static float ToDegrees(int quarterTurns)
        {
            return ((quarterTurns % 4) + 4) % 4 * 90f;
        }
    }

    public static class PhaseRules
    {
        public static bool IsColliderEnabled(StagePhase surfacePhase, StagePhase activePhase)
        {
            return surfacePhase == activePhase;
        }
    }

    public static class RunTimeFormat
    {
        public static string Format(float seconds)
        {
            if (float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds < 0f)
            {
                seconds = 0f;
            }

            int totalCentiseconds = Mathf.FloorToInt(seconds * 100f);
            int minutes = totalCentiseconds / 6000;
            int wholeSeconds = totalCentiseconds / 100 % 60;
            int centiseconds = totalCentiseconds % 100;
            return $"{minutes:00}:{wholeSeconds:00}.{centiseconds:00}";
        }
    }

    public static class ShardRules
    {
        public static int RequiredForRoom(int roomIndex)
        {
            return Mathf.Clamp(roomIndex, 0, 3);
        }

        public static bool IsRoomUnlocked(int roomIndex, int collectedShards)
        {
            return collectedShards >= RequiredForRoom(roomIndex);
        }
    }

    [Serializable]
    public sealed class PlatformSpec
    {
        public Vector2 center;
        public Vector2 size;
        public SurfaceStyle style;
        public StagePhase phase;
        public bool phaseSurface;

        public PlatformSpec(float x, float y, float width, float height, SurfaceStyle style, StagePhase phase, bool phaseSurface = false)
        {
            center = new Vector2(x, y);
            size = new Vector2(width, height);
            this.style = style;
            this.phase = phase;
            this.phaseSurface = phaseSurface;
        }
    }

    [Serializable]
    public sealed class HazardSpec
    {
        public HazardKind kind;
        public Vector2 center;
        public Vector2 size;
        public Vector2 travel;
        public float speed;

        public HazardSpec(HazardKind kind, float x, float y, float width, float height, float travelX = 0f, float travelY = 0f, float speed = 1f)
        {
            this.kind = kind;
            center = new Vector2(x, y);
            size = new Vector2(width, height);
            travel = new Vector2(travelX, travelY);
            this.speed = speed;
        }
    }

    [Serializable]
    public sealed class ShardSpec
    {
        public int id;
        public Vector2 position;

        public ShardSpec(int id, float x, float y)
        {
            this.id = id;
            position = new Vector2(x, y);
        }
    }

    [Serializable]
    public sealed class RoomPalette
    {
        public Color background;
        public Color grid;
        public Color stone;
        public Color moss;
        public Color coral;
        public Color teal;
        public Color negative;

        public Color GetTint(SurfaceStyle style, StagePhase phase)
        {
            if (style == SurfaceStyle.Stone)
            {
                return stone;
            }

            return phase == StagePhase.Negative ? negative : style switch
            {
                SurfaceStyle.Coral => coral,
                SurfaceStyle.Teal => teal,
                _ => moss
            };
        }
    }

    [Serializable]
    public sealed class RoomSpec
    {
        public int index;
        public string name;
        public string hint;
        public Vector2 spawn;
        public Vector2 exit;
        public StagePhase startPhase;
        public int startQuarterTurns;
        public PlatformSpec[] platforms;
        public HazardSpec[] hazards;
        public ShardSpec[] shards;
        public RoomPalette palette;
    }
}
