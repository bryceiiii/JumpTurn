using UnityEngine;

namespace JumpTurn
{
    public static class RoomCatalog
    {
        private static readonly RoomSpec[] Rooms =
        {
            BuildCourtyard(),
            BuildRotatingHall(),
            BuildPhaseCellar(),
            BuildSkyCore()
        };

        public static int Count => Rooms.Length;

        public static RoomSpec Get(int index)
        {
            return Rooms[Mathf.Clamp(index, 0, Rooms.Length - 1)];
        }

        private static RoomSpec BuildCourtyard()
        {
            return new RoomSpec
            {
                index = 0,
                name = "风铃庭院",
                hint = "A / D 移动   SPACE 跳跃",
                spawn = new Vector2(-8f, -3.2f),
                exit = new Vector2(8.2f, -2.65f),
                startPhase = StagePhase.Positive,
                startQuarterTurns = 0,
                palette = MakePalette(
                    Hex("071a20"), Hex("153c42"), Hex("8b9aa3"), Hex("48caaa"),
                    Hex("ff8b61"), Hex("52d6d2"), Hex("916ad9")),
                platforms = new[]
                {
                    new PlatformSpec(-6.5f, -4.9f, 10f, 1f, SurfaceStyle.Moss, StagePhase.Positive),
                    new PlatformSpec(-1f, -3.45f, 3.2f, 0.5f, SurfaceStyle.Coral, StagePhase.Positive),
                    new PlatformSpec(3.2f, -2.25f, 3.4f, 0.5f, SurfaceStyle.Teal, StagePhase.Positive),
                    new PlatformSpec(7.8f, -3.5f, 5f, 0.5f, SurfaceStyle.Stone, StagePhase.Positive)
                },
                hazards = new HazardSpec[0],
                shards = new ShardSpec[0]
            };
        }

        private static RoomSpec BuildRotatingHall()
        {
            return new RoomSpec
            {
                index = 1,
                name = "折转回廊",
                hint = "Q / E 旋转回廊   收集天空碎片",
                spawn = new Vector2(0f, 3.1f),
                exit = new Vector2(8f, 2.95f),
                startPhase = StagePhase.Positive,
                startQuarterTurns = 0,
                palette = MakePalette(
                    Hex("160c1f"), Hex("3b183f"), Hex("a69aa8"), Hex("56d6b5"),
                    Hex("ff7a78"), Hex("58cfc4"), Hex("b26be8")),
                platforms = new[]
                {
                    new PlatformSpec(0f, 0f, 4f, 4f, SurfaceStyle.Stone, StagePhase.Positive),
                    new PlatformSpec(2f, -6f, 0.5f, 6f, SurfaceStyle.Coral, StagePhase.Positive, true)
                },
                hazards = new[]
                {
                    new HazardSpec(HazardKind.Saw, 0f, -3.1f, 1.4f, 1.4f, 4f, 0f, 1.8f)
                },
                shards = new[]
                {
                    new ShardSpec(1, 6f, 2.9f)
                }
            };
        }

        private static RoomSpec BuildPhaseCellar()
        {
            return new RoomSpec
            {
                index = 2,
                name = "反相地窖",
                hint = "SHIFT 切换正相 / 反相",
                spawn = new Vector2(-8f, -3.2f),
                exit = new Vector2(9.1f, -3.15f),
                startPhase = StagePhase.Positive,
                startQuarterTurns = 0,
                palette = MakePalette(
                    Hex("0a1028"), Hex("252459"), Hex("94a2b8"), Hex("68e1b6"),
                    Hex("ff7ea7"), Hex("54d0de"), Hex("b275ff")),
                platforms = new[]
                {
                    new PlatformSpec(-8.3f, -4.9f, 6.4f, 1f, SurfaceStyle.Stone, StagePhase.Positive),
                    new PlatformSpec(-4.8f, -3.2f, 4.2f, 0.5f, SurfaceStyle.Coral, StagePhase.Positive, true),
                    new PlatformSpec(0f, -1.9f, 4.8f, 0.5f, SurfaceStyle.Teal, StagePhase.Positive, true),
                    new PlatformSpec(3.8f, -3.55f, 6.2f, 0.5f, SurfaceStyle.Moss, StagePhase.Negative, true),
                    new PlatformSpec(9f, -4.1f, 5f, 1f, SurfaceStyle.Stone, StagePhase.Negative)
                },
                hazards = new[]
                {
                    new HazardSpec(HazardKind.Spikes, 0.7f, -1.45f, 1f, 0.35f),
                    new HazardSpec(HazardKind.Patrol, 7.8f, -3.1f, 1.15f, 1f, 3.4f, 0f, 1.5f)
                },
                shards = new[]
                {
                    new ShardSpec(2, 2f, -1.05f)
                }
            };
        }

        private static RoomSpec BuildSkyCore()
        {
            return new RoomSpec
            {
                index = 3,
                name = "天穹核心",
                hint = "旋转与相位必须一起使用",
                spawn = new Vector2(0f, 3.1f),
                exit = new Vector2(9f, 7.25f),
                startPhase = StagePhase.Positive,
                startQuarterTurns = 0,
                palette = MakePalette(
                    Hex("130f08"), Hex("463018"), Hex("b8a17d"), Hex("ddc35b"),
                    Hex("ff9b4a"), Hex("78d6c5"), Hex("ef6ed7")),
                platforms = new[]
                {
                    new PlatformSpec(0f, 0f, 4f, 4f, SurfaceStyle.Stone, StagePhase.Positive),
                    new PlatformSpec(2f, -6f, 0.5f, 6f, SurfaceStyle.Moss, StagePhase.Positive, true),
                    new PlatformSpec(4f, -7f, 0.5f, 2f, SurfaceStyle.Coral, StagePhase.Negative, true),
                    new PlatformSpec(6f, -10f, 0.5f, 2f, SurfaceStyle.Teal, StagePhase.Negative, true)
                },
                hazards = new[]
                {
                    new HazardSpec(HazardKind.Saw, 2f, -2.4f, 1.4f, 1.4f, 0f, 2.8f, 1.7f)
                },
                shards = new[]
                {
                    new ShardSpec(3, 8f, 2.6f)
                }
            };
        }

        private static RoomPalette MakePalette(Color background, Color grid, Color stone, Color moss, Color coral, Color teal, Color negative)
        {
            return new RoomPalette
            {
                background = background,
                grid = grid,
                stone = stone,
                moss = moss,
                coral = coral,
                teal = teal,
                negative = negative
            };
        }

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString("#" + value, out Color color);
            return color;
        }
    }
}
