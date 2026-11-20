using System;
using System.Collections.Generic;
using UnityEngine;

namespace JumpTurn
{
    public static class RuntimeArt
    {
        private const float PixelsPerUnit = 16f;
        private static readonly Dictionary<string, Texture2D> Textures = new();
        private static readonly Dictionary<string, Sprite[]> Sheets = new();
        private static Sprite _whiteSprite;
        private static Sprite _starSprite;
        private static Sprite _glowSprite;
        private static Sprite _grassTop;
        private static Sprite _dirtFill;
        private static Material _monoTintMaterial;

        public static Sprite WhiteSprite => _whiteSprite ??= BuildWhiteSprite();
        public static Sprite StarSprite => _starSprite ??= BuildStarSprite();
        public static Sprite GlowSprite => _glowSprite ??= BuildGlowSprite();
        public static Sprite GrassTop => _grassTop ??= Slice(LoadTexture("Tilemap"), new Rect(240f, 128f, 16f, 16f));
        public static Sprite DirtFill => _dirtFill ??= Slice(LoadTexture("Tilemap"), new Rect(160f, 128f, 16f, 16f));
        public static Material MonoTintMaterial => _monoTintMaterial ??= CreateMonoMaterial();

        public static Sprite[] GetBunnyIdle()
        {
            Sprite[] all = GetSheet("Character", 16, 16);
            return new[] { all[0] };
        }

        public static Sprite[] GetBunnyRun()
        {
            return GetSheet("Character", 16, 16);
        }

        public static Sprite[] GetBunnyJump()
        {
            Sprite[] all = GetSheet("Character", 16, 16);
            return new[] { all[Mathf.Min(3, all.Length - 1)] };
        }

        public static Sprite[] GetBunnyFall()
        {
            Sprite[] all = GetSheet("Character", 16, 16);
            return new[] { all[Mathf.Min(5, all.Length - 1)] };
        }

        public static Sprite[] GetSlime()
        {
            return GetSheet("Ghost", 16, 16);
        }

        public static Sprite[] GetBird()
        {
            return GetSheet("Ghost", 16, 16);
        }

        public static Sprite[] GetSaw()
        {
            return new[] { BuildSawSprite() };
        }

        public static Sprite GetSpikes()
        {
            return BuildSpikeSprite();
        }

        public static Sprite GetExit()
        {
            return Slice(LoadTexture("Tilemap"), new Rect(224f, 224f, 16f, 16f));
        }

        public static Sprite CreateBackdrop(RoomPalette palette, int seed)
        {
            const int width = 640;
            const int height = 360;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "JumpTurn Backdrop"
            };
            var pixels = new Color32[width * height];
            Color32 background = palette.background;
            Color32 grid = palette.grid;
            grid.a = 90;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool line = x % 32 == 0 || y % 32 == 0;
                    pixels[y * width + x] = line ? grid : background;
                }
            }

            var random = new System.Random(seed * 997 + 41);
            for (int i = 0; i < 56; i++)
            {
                int x = random.Next(2, width - 2);
                int y = random.Next(2, height - 2);
                int size = random.NextDouble() > 0.88 ? 2 : 1;
                Color star = Color.Lerp(Color.white, palette.grid, (float)random.NextDouble() * 0.55f);
                star.a = 0.35f + (float)random.NextDouble() * 0.5f;
                for (int yy = 0; yy < size; yy++)
                {
                    for (int xx = 0; xx < size; xx++)
                    {
                        pixels[(y + yy) * width + x + xx] = star;
                    }
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
        }

        public static void DestroyGeneratedSprite(SpriteRenderer renderer)
        {
            if (renderer == null || renderer.sprite == null)
            {
                return;
            }

            Texture2D texture = renderer.sprite.texture;
            if (texture != null && texture.name == "JumpTurn Backdrop")
            {
                UnityEngine.Object.Destroy(texture);
            }
        }

        private static Sprite[] GetSheet(string resourceName, int frameWidth, int frameHeight)
        {
            if (Sheets.TryGetValue(resourceName, out Sprite[] cached))
            {
                return cached;
            }

            Texture2D texture = LoadTexture(resourceName);
            int count = Mathf.Max(1, texture.width / frameWidth);
            var frames = new Sprite[count];
            for (int i = 0; i < count; i++)
            {
                frames[i] = Slice(texture, new Rect(i * frameWidth, texture.height - frameHeight, frameWidth, frameHeight));
            }

            Sheets[resourceName] = frames;
            return frames;
        }

        private static Texture2D LoadTexture(string resourceName)
        {
            if (Textures.TryGetValue(resourceName, out Texture2D cached))
            {
                return cached;
            }

            var texture = Resources.Load<Texture2D>("Kenney1Bit/" + resourceName);
            if (texture == null)
            {
                texture = BuildFallbackTexture(resourceName);
            }

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = resourceName == "Tilemap" ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            texture.anisoLevel = 0;
            Textures[resourceName] = texture;
            return texture;
        }

        private static Sprite Slice(Texture2D texture, Rect rect)
        {
            return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
        }

        private static Texture2D BuildFallbackTexture(string name)
        {
            var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false) { name = "Fallback " + name };
            var pixels = new Color32[256];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(255, 80, 170, 255);
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        private static Sprite BuildWhiteSprite()
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false) { name = "JumpTurn White" };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 16f);
        }

        private static Sprite BuildStarSprite()
        {
            const int size = 16;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "JumpTurn Star" };
            var pixels = new Color32[size * size];
            Vector2 center = new(size * 0.5f - 0.5f, size * 0.5f - 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x - center.x);
                    float dy = Mathf.Abs(y - center.y);
                    float distance = dx + dy * 0.42f;
                    float vertical = dy + dx * 0.42f;
                    bool inside = dx < 1.4f || dy < 1.4f || distance < 5.5f || vertical < 3.4f;
                    pixels[y * size + x] = inside ? new Color32(255, 225, 98, 255) : new Color32(0, 0, 0, 0);
                }
            }

            texture.SetPixels32(pixels);
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 16f);
        }

        private static Sprite BuildGlowSprite()
        {
            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "JumpTurn Glow" };
            var pixels = new Color32[size * size];
            Vector2 center = new(size * 0.5f - 0.5f, size * 0.5f - 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    bool ring = distance > 12.8f && distance < 15.2f;
                    pixels[y * size + x] = ring
                        ? new Color32(255, 225, 110, (byte)Mathf.Lerp(220, 30, (distance - 12.8f) / 2.4f))
                        : new Color32(0, 0, 0, 0);
                }
            }

            texture.SetPixels32(pixels);
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 16f);
        }

        private static Sprite BuildSawSprite()
        {
            const int size = 16;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "JumpTurn Saw" };
            var pixels = new Color32[size * size];
            Vector2 center = new(size * 0.5f - 0.5f, size * 0.5f - 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 delta = new Vector2(x, y) - center;
                    float distance = delta.magnitude;
                    float angle = Mathf.Atan2(delta.y, delta.x);
                    float teeth = 6.2f + Mathf.Cos(angle * 8f) * 1.35f;
                    bool inside = distance < teeth && distance > 2.1f;
                    pixels[y * size + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
                }
            }

            texture.SetPixels32(pixels);
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 16f);
        }

        private static Sprite BuildSpikeSprite()
        {
            const int width = 16;
            const int height = 16;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = "JumpTurn Spikes" };
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool baseRow = y < 3;
                    int localX = x % 5;
                    int spikeHeight = 11 - Mathf.Abs(localX - 2) * 3;
                    bool triangle = localX <= 4 && y >= 2 && y <= spikeHeight;
                    pixels[y * width + x] = baseRow || triangle
                        ? new Color32(255, 255, 255, 255)
                        : new Color32(0, 0, 0, 0);
                }
            }

            texture.SetPixels32(pixels);
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 16f);
        }

        private static Material CreateMonoMaterial()
        {
            Shader shader = Resources.Load<Shader>("Shaders/MonoTint");
            if (shader == null)
            {
                shader = Shader.Find("JumpTurn/MonoTint");
            }

            return shader != null ? new Material(shader) { name = "JumpTurn MonoTint" } : null;
        }
    }
}
