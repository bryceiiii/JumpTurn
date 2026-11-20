using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace JumpTurn
{
    public sealed class GameUI : MonoBehaviour
    {
        private JumpTurnGame _game;
        private TMP_FontAsset _font;
        private GameObject _titleRoot;
        private GameObject _hudRoot;
        private GameObject _pauseRoot;
        private GameObject _completeRoot;
        private GameObject _deathRoot;
        private TMP_Text _timerText;
        private TMP_Text _shardText;
        private TMP_Text _deathsText;
        private TMP_Text _roomText;
        private TMP_Text _phaseText;
        private TMP_Text _hintText;
        private TMP_Text _bestText;
        private TMP_Text _deathText;
        private TMP_Text _completeTimeText;
        private TMP_Text _completeBestText;
        private Image _fadeImage;
        private GameObject _blockedMessageObject;
        private TMP_Text _blockedMessage;
        private float _blockedUntil;

        public void Initialize(JumpTurnGame game)
        {
            _game = game;
            EnsureEventSystem();
            _font = LoadFont();
            BuildCanvas();
            ShowTitle(true);
            ShowHud(false);
        }

        public void ShowTitle(bool visible)
        {
            _titleRoot.SetActive(visible);
        }

        public void ShowHud(bool visible)
        {
            _hudRoot.SetActive(visible);
        }

        public void ShowPause(bool visible)
        {
            _pauseRoot.SetActive(visible);
        }

        public void ShowDeath(bool visible)
        {
            _deathRoot.SetActive(visible);
        }

        public void ShowCompletion(float elapsed, float best, bool newBest)
        {
            _completeRoot.SetActive(true);
            _completeTimeText.text = "本局时间  " + RunTimeFormat.Format(elapsed);
            _completeBestText.text = newBest
                ? "NEW BEST  " + RunTimeFormat.Format(best)
                : "最佳时间  " + RunTimeFormat.Format(best);
        }

        public void HideCompletion()
        {
            _completeRoot.SetActive(false);
        }

        public void SetSnapshot(GameSnapshot snapshot)
        {
            _timerText.text = RunTimeFormat.Format(snapshot.ElapsedSeconds);
            _shardText.text = "天空碎片  " + snapshot.Shards + " / 3";
            _deathsText.text = "坠落  " + snapshot.Deaths;
            _roomText.text = snapshot.RoomName;
            _phaseText.text = snapshot.Phase == StagePhase.Positive ? "正相" : "反相";
            _phaseText.color = snapshot.Phase == StagePhase.Positive ? new Color(1f, 0.55f, 0.38f) : new Color(0.74f, 0.48f, 1f);
            _hintText.text = snapshot.Hint;
            _bestText.text = snapshot.BestSeconds > 0f
                ? "BEST " + RunTimeFormat.Format(snapshot.BestSeconds)
                : "BEST --:--.--";
        }

        public void SetFade(float alpha)
        {
            Color color = _fadeImage.color;
            color.a = Mathf.Clamp01(alpha);
            _fadeImage.color = color;
            _fadeImage.raycastTarget = alpha > 0.9f;
        }

        public void ShowBlocked(string message)
        {
            _blockedMessage.text = message;
            _blockedMessageObject.SetActive(true);
            _blockedUntil = Time.unscaledTime + 1.3f;
        }

        private void Update()
        {
            if (_blockedMessageObject != null && _blockedMessageObject.activeSelf && Time.unscaledTime > _blockedUntil)
            {
                _blockedMessageObject.SetActive(false);
            }
        }

        private void BuildCanvas()
        {
            var canvasObject = new GameObject("JumpTurn Canvas");
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(640f, 360f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            BuildTitle(canvasObject.transform);
            BuildHud(canvasObject.transform);
            BuildPause(canvasObject.transform);
            BuildCompletion(canvasObject.transform);
            BuildDeath(canvasObject.transform);

            var fadeObject = CreatePanel(canvasObject.transform, "Fade", new Color(0.015f, 0.02f, 0.04f, 0f), new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            _fadeImage = fadeObject.GetComponent<Image>();
            _fadeImage.raycastTarget = false;
        }

        private void BuildTitle(Transform canvas)
        {
            var overlay = CreatePanel(canvas, "Title", new Color(0.01f, 0.02f, 0.035f, 0.86f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _titleRoot = overlay;

            TMP_Text title = CreateLabel(overlay.transform, "GameTitle", "回 旋 跳", 58f, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-260f, 32f), new Vector2(260f, 110f), TextAlignmentOptions.Center);
            title.color = new Color(1f, 0.86f, 0.4f);
            title.fontStyle = FontStyles.Bold;
            title.outlineWidth = 0.22f;
            title.outlineColor = new Color(0.18f, 0.07f, 0.12f);

            TMP_Text subtitle = CreateLabel(overlay.transform, "Subtitle", "J U M P T U R N  /  天 空 失 窃 案", 13f, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-260f, 4f), new Vector2(260f, 32f), TextAlignmentOptions.Center);
            subtitle.color = new Color(0.56f, 0.92f, 0.84f);

            Button start = CreateButton(overlay.transform, "StartButton", "开始游戏", new Vector2(0f, -54f), new Vector2(164f, 38f), () => _game.StartRun());
            start.Select();
            CreateButton(overlay.transform, "QuitButton", "退出", new Vector2(0f, -102f), new Vector2(112f, 34f), Application.Quit);

            TMP_Text footer = CreateLabel(overlay.transform, "Footer", "原创机制 Demo · 像素素材 CC0", 11f, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-250f, 14f), new Vector2(250f, 34f), TextAlignmentOptions.Center);
            footer.color = new Color(0.67f, 0.72f, 0.77f, 0.8f);
        }

        private void BuildHud(Transform canvas)
        {
            var hud = new GameObject("HUD", typeof(RectTransform));
            hud.transform.SetParent(canvas, false);
            Stretch((RectTransform)hud.transform);
            _hudRoot = hud;

            GameObject timerPanel = CreatePanel(hud.transform, "TimerPanel", new Color(0.03f, 0.06f, 0.08f, 0.78f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(12f, -48f), new Vector2(154f, -14f));
            _timerText = CreateLabel(timerPanel.transform, "Timer", "00:00.00", 22f, Vector2.zero, Vector2.one, new Vector2(8f, 2f), new Vector2(-6f, -2f), TextAlignmentOptions.MidlineLeft);
            _timerText.color = Color.white;
            _timerText.fontStyle = FontStyles.Bold;
            _bestText = CreateLabel(timerPanel.transform, "Best", "BEST --:--.--", 9f, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(9f, 4f), new Vector2(-8f, 18f), TextAlignmentOptions.BottomLeft);
            _bestText.color = new Color(0.67f, 0.86f, 0.79f);

            GameObject phasePanel = CreatePanel(hud.transform, "PhasePanel", new Color(0.08f, 0.05f, 0.12f, 0.78f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(12f, -80f), new Vector2(96f, -54f));
            _phaseText = CreateLabel(phasePanel.transform, "Phase", "正相", 14f, Vector2.zero, Vector2.one, new Vector2(6f, 0f), new Vector2(-4f, 0f), TextAlignmentOptions.Center);

            _roomText = CreateLabel(hud.transform, "RoomName", "风铃庭院", 18f, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-160f, -42f), new Vector2(160f, -12f), TextAlignmentOptions.Center);
            _roomText.color = new Color(1f, 0.93f, 0.7f);
            _roomText.fontStyle = FontStyles.Bold;

            _shardText = CreateLabel(hud.transform, "Shards", "天空碎片  0 / 3", 13f, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-164f, -44f), new Vector2(-12f, -16f), TextAlignmentOptions.MidlineRight);
            _shardText.color = new Color(1f, 0.82f, 0.35f);
            _deathsText = CreateLabel(hud.transform, "Deaths", "坠落  0", 11f, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-130f, -72f), new Vector2(-12f, -48f), TextAlignmentOptions.MidlineRight);
            _deathsText.color = new Color(0.72f, 0.78f, 0.84f);

            GameObject hintPanel = CreatePanel(hud.transform, "HintPanel", new Color(0.02f, 0.04f, 0.06f, 0.78f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-250f, 12f), new Vector2(250f, 43f));
            _hintText = CreateLabel(hintPanel.transform, "Hint", string.Empty, 12f, Vector2.zero, Vector2.one, new Vector2(8f, 1f), new Vector2(-8f, -1f), TextAlignmentOptions.Center);
            _hintText.color = new Color(0.8f, 0.93f, 0.92f);

            _blockedMessageObject = CreatePanel(hud.transform, "BlockedMessage", new Color(0.45f, 0.1f, 0.12f, 0.9f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-110f, 74f), new Vector2(110f, 108f));
            _blockedMessage = CreateLabel(_blockedMessageObject.transform, "Text", string.Empty, 13f, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAlignmentOptions.Center);
            _blockedMessageObject.SetActive(false);
        }

        private void BuildPause(Transform canvas)
        {
            GameObject overlay = CreatePanel(canvas, "Pause", new Color(0.01f, 0.02f, 0.04f, 0.9f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _pauseRoot = overlay;
            TMP_Text title = CreateLabel(overlay.transform, "PauseTitle", "暂停", 42f, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-180f, 55f), new Vector2(180f, 115f), TextAlignmentOptions.Center);
            title.color = new Color(0.6f, 0.95f, 0.82f);
            CreateButton(overlay.transform, "Resume", "继续", new Vector2(0f, 8f), new Vector2(150f, 36f), () => _game.Resume());
            CreateButton(overlay.transform, "Restart", "重新开始", new Vector2(0f, -38f), new Vector2(150f, 36f), () => _game.RestartRun());
            CreateButton(overlay.transform, "Title", "返回标题", new Vector2(0f, -84f), new Vector2(150f, 36f), () => _game.ReturnToTitle());
            overlay.SetActive(false);
        }

        private void BuildCompletion(Transform canvas)
        {
            GameObject overlay = CreatePanel(canvas, "Completion", new Color(0.01f, 0.02f, 0.04f, 0.92f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _completeRoot = overlay;
            TMP_Text title = CreateLabel(overlay.transform, "CompleteTitle", "天 空 归 还", 44f, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-280f, 48f), new Vector2(280f, 116f), TextAlignmentOptions.Center);
            title.color = new Color(1f, 0.83f, 0.3f);
            title.fontStyle = FontStyles.Bold;
            _completeTimeText = CreateLabel(overlay.transform, "CompleteTime", string.Empty, 18f, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-180f, 4f), new Vector2(180f, 34f), TextAlignmentOptions.Center);
            _completeBestText = CreateLabel(overlay.transform, "CompleteBest", string.Empty, 13f, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-180f, -26f), new Vector2(180f, -2f), TextAlignmentOptions.Center);
            _completeBestText.color = new Color(0.61f, 0.92f, 0.79f);
            CreateButton(overlay.transform, "CompleteRetry", "再跑一次", new Vector2(0f, -76f), new Vector2(154f, 36f), () => _game.RestartRun());
            CreateButton(overlay.transform, "CompleteTitle", "返回标题", new Vector2(0f, -122f), new Vector2(154f, 34f), () => _game.ReturnToTitle());
            overlay.SetActive(false);
        }

        private void BuildDeath(Transform canvas)
        {
            GameObject overlay = CreatePanel(canvas, "Death", new Color(0.18f, 0.02f, 0.04f, 0.55f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _deathRoot = overlay;
            _deathText = CreateLabel(overlay.transform, "DeathText", "碎片散落了…", 30f, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-200f, -20f), new Vector2(200f, 50f), TextAlignmentOptions.Center);
            _deathText.color = new Color(1f, 0.64f, 0.54f);
            overlay.SetActive(false);
        }

        private Button CreateButton(Transform parent, string name, string text, Vector2 position, Vector2 size, Action onClick)
        {
            GameObject outer = CreatePanel(parent, name, new Color(0.3f, 0.75f, 0.66f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position - size * 0.5f, position + size * 0.5f);
            var button = outer.AddComponent<Button>();
            button.targetGraphic = outer.GetComponent<Image>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.08f, 0.13f, 0.15f, 0.96f);
            colors.highlightedColor = new Color(0.14f, 0.28f, 0.27f, 1f);
            colors.pressedColor = new Color(0.72f, 0.95f, 0.75f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.fadeDuration = 0.05f;
            button.colors = colors;
            button.onClick.AddListener(() =>
            {
                _game.Audio.Play(SfxId.Menu);
                onClick?.Invoke();
            });

            GameObject inner = CreatePanel(outer.transform, "Fill", new Color(0.04f, 0.075f, 0.09f, 0.95f), Vector2.zero, Vector2.one, new Vector2(2f, 2f), new Vector2(-2f, -2f));
            inner.GetComponent<Image>().raycastTarget = false;
            TMP_Text label = CreateLabel(inner.transform, "Label", text, 14f, Vector2.zero, Vector2.one, new Vector2(4f, 0f), new Vector2(-4f, 0f), TextAlignmentOptions.Center);
            label.color = new Color(0.85f, 0.96f, 0.92f);
            return button;
        }

        private TMP_Text CreateLabel(Transform parent, string name, string text, float size, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, TextAlignmentOptions alignment)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            var textComponent = obj.AddComponent<TextMeshProUGUI>();
            textComponent.font = _font;
            textComponent.text = text;
            textComponent.fontSize = size;
            textComponent.enableAutoSizing = false;
            textComponent.textWrappingMode = TextWrappingModes.NoWrap;
            textComponent.overflowMode = TextOverflowModes.Overflow;
            textComponent.alignment = alignment;
            textComponent.raycastTarget = false;
            RectTransform rect = textComponent.rectTransform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return textComponent;
        }

        private static GameObject CreatePanel(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            var image = obj.AddComponent<Image>();
            image.sprite = RuntimeArt.WhiteSprite;
            image.type = Image.Type.Simple;
            image.color = color;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return obj;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            var module = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }

        private static TMP_FontAsset LoadFont()
        {
            Font font = Resources.Load<Font>("UI/ZCOOLQingKeHuangYou-Regular");

            if (font != null)
            {
                TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(font);
                if (asset != null)
                {
                    asset.name = "JumpTurn Chinese Runtime";
                    if (asset.atlasTexture != null)
                    {
                        asset.atlasTexture.filterMode = FilterMode.Point;
                    }

                    return asset;
                }
            }

            return TMP_Settings.defaultFontAsset;
        }
    }
}
