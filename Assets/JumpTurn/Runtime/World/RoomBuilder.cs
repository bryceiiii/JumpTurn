using System.Collections.Generic;
using UnityEngine;

namespace JumpTurn
{
    public sealed class RoomView
    {
        public GameObject Root;
        public StageController Stage;
        public ExitPortal Exit;
    }

    public static class RoomBuilder
    {
        public static RoomView Build(JumpTurnGame game, PlayerController player, RoomSpec room, Transform parent)
        {
            var view = new RoomView();
            view.Root = new GameObject("Room_" + room.index);
            view.Root.transform.SetParent(parent, false);

            var backdrop = new GameObject("Backdrop");
            backdrop.transform.SetParent(view.Root.transform, false);
            SpriteRenderer backdropRenderer = backdrop.AddComponent<SpriteRenderer>();
            backdropRenderer.sprite = RuntimeArt.CreateBackdrop(room.palette, room.index + 1);
            backdropRenderer.sortingOrder = -100;
            backdrop.AddComponent<BackdropCleanup>().Initialize(backdropRenderer);

            var rotatingStage = new GameObject("RotatingStage");
            rotatingStage.transform.SetParent(view.Root.transform, false);

            var phaseSurfaces = new List<PhaseSurface>();
            foreach (PlatformSpec platform in room.platforms)
            {
                BuildPlatform(rotatingStage.transform, room, platform, phaseSurfaces);
            }

            foreach (HazardSpec hazard in room.hazards)
            {
                BuildHazard(rotatingStage.transform, hazard);
            }

            foreach (ShardSpec shard in room.shards)
            {
                int id = shard.id;
                if (game.HasShard(id))
                {
                    continue;
                }

                var pickupObject = new GameObject("SkyShard_" + id);
                pickupObject.transform.SetParent(view.Root.transform, false);
                pickupObject.transform.localPosition = shard.position;
                pickupObject.AddComponent<SkyShardPickup>().Initialize(game, id);
            }

            var exitObject = new GameObject("ExitPortal");
            exitObject.transform.SetParent(view.Root.transform, false);
            exitObject.transform.localPosition = room.exit;
            view.Exit = exitObject.AddComponent<ExitPortal>();
            view.Exit.Initialize(game, room.index, ShardRules.RequiredForRoom(room.index));

            var stageObject = new GameObject("StageController");
            stageObject.transform.SetParent(view.Root.transform, false);
            view.Stage = stageObject.AddComponent<StageController>();
            view.Stage.Initialize(game, player, room, rotatingStage.transform, phaseSurfaces);
            return view;
        }

        private static void BuildPlatform(Transform parent, RoomSpec room, PlatformSpec spec, List<PhaseSurface> surfaces)
        {
            var platform = new GameObject(spec.phaseSurface ? "PhasePlatform" : "SolidPlatform");
            platform.transform.SetParent(parent, false);
            platform.transform.localPosition = spec.center;

            Color tint = room.palette.GetTint(spec.style, spec.phase);
            float width = Mathf.Max(0.25f, spec.size.x);
            float height = Mathf.Max(0.25f, spec.size.y);
            var renderers = new List<SpriteRenderer>();
            int columns = Mathf.Max(1, Mathf.CeilToInt(width));
            int rows = Mathf.Max(1, Mathf.CeilToInt(height));
            float tileScaleX = width / columns;
            float tileScaleY = height / rows;

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    var tile = new GameObject("Tile");
                    tile.transform.SetParent(platform.transform, false);
                    float localX = -width * 0.5f + tileScaleX * (x + 0.5f);
                    float localY = -height * 0.5f + tileScaleY * (y + 0.5f);
                    tile.transform.localPosition = new Vector3(localX, localY, 0f);
                    tile.transform.localScale = new Vector3(tileScaleX, tileScaleY, 1f);
                    SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
                    bool topRow = y == rows - 1;
                    renderer.sprite = topRow ? RuntimeArt.GrassTop : RuntimeArt.DirtFill;
                    renderer.color = topRow ? tint : Color.Lerp(tint, new Color(0.16f, 0.12f, 0.2f), 0.28f);
                    renderer.sharedMaterial = RuntimeArt.MonoTintMaterial;
                    renderer.sortingOrder = 0;
                    renderers.Add(renderer);
                }
            }

            var collider = platform.AddComponent<BoxCollider2D>();
            collider.size = spec.size;
            platform.layer = LayerMask.NameToLayer("Stage");

            if (spec.phaseSurface)
            {
                PhaseSurface phaseSurface = platform.AddComponent<PhaseSurface>();
                phaseSurface.Configure(spec.phase, collider, renderers);
                surfaces.Add(phaseSurface);
            }
        }

        private static void BuildHazard(Transform parent, HazardSpec spec)
        {
            var hazardObject = new GameObject(spec.kind.ToString());
            hazardObject.transform.SetParent(parent, false);
            hazardObject.transform.localPosition = spec.center;
            hazardObject.layer = LayerMask.NameToLayer("Hazard");

            var visual = new GameObject("Visual");
            visual.transform.SetParent(hazardObject.transform, false);
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 3;
            renderer.sharedMaterial = RuntimeArt.MonoTintMaterial;
            var collider = hazardObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            hazardObject.AddComponent<Hazard2D>();

            switch (spec.kind)
            {
                case HazardKind.Patrol:
                    renderer.sprite = RuntimeArt.GetSlime()[0];
                    collider.size = spec.size;
                    var animator = hazardObject.AddComponent<SpriteAnimator2D>();
                    animator.Initialize(renderer, RuntimeArt.GetSlime(), 9f);
                    hazardObject.AddComponent<MovingHazard>().Configure(spec.travel, spec.speed, false);
                    break;
                case HazardKind.Saw:
                    renderer.sprite = RuntimeArt.GetSaw()[0];
                    renderer.color = new Color(1f, 0.34f, 0.3f, 1f);
                    collider.size = spec.size;
                    var sawAnimator = hazardObject.AddComponent<SpriteAnimator2D>();
                    sawAnimator.Initialize(renderer, RuntimeArt.GetSaw(), 14f);
                    hazardObject.AddComponent<MovingHazard>().Configure(spec.travel, spec.speed, true);
                    break;
                default:
                    renderer.sprite = RuntimeArt.GetSpikes();
                    renderer.color = new Color(1f, 0.38f, 0.32f, 1f);
                    renderer.drawMode = SpriteDrawMode.Simple;
                    visual.transform.localScale = new Vector3(spec.size.x / Mathf.Max(0.1f, renderer.sprite.bounds.size.x), spec.size.y / Mathf.Max(0.1f, renderer.sprite.bounds.size.y), 1f);
                    collider.size = spec.size;
                    collider.offset = new Vector2(0f, spec.size.y * 0.15f);
                    break;
            }
        }
    }
}
