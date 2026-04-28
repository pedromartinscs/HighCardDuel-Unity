using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HighCardDuel
{
    public sealed class HighCardDuelBootstrap : MonoBehaviour
    {
        private const string CardFrontAssetPath = "Assets/Art/Cards/Backgrounds/PMCS_CardFront_Empty.png";
        private const string CardFrontFallbackAssetPath = "Assets/Art/Cards/Fronts/PMCS_CardFront_Empty.png";
        private const string CardBackAssetPath = "Assets/Art/Cards/Backs/PMCS_CardBack_Red.png";
        private const string SpadesAssetPath = "Assets/Art/Cards/Suits/Suit_Spades.png";
        private const string HeartsAssetPath = "Assets/Art/Cards/Suits/Suit_Hearts.png";
        private const string DiamondsAssetPath = "Assets/Art/Cards/Suits/Suit_Diamonds.png";
        private const string ClubsAssetPath = "Assets/Art/Cards/Suits/Suit_Clubs.png";
        private const string BackgroundAssetPath = "Assets/Art/Backgrounds/PMCS_CasinoTable.png";
        private const string BackgroundFallbackAssetPath = "Assets/Art/Background/cassino_background.png";

        private Sprite cardFrontSprite;
        private Sprite cardBackSprite;
        private Sprite spadesSprite;
        private Sprite heartsSprite;
        private Sprite diamondsSprite;
        private Sprite clubsSprite;
        private Sprite backgroundSprite;
        private Font defaultFont;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateDemo()
        {
            if (Object.FindFirstObjectByType<GameController>() != null)
            {
                return;
            }

            var bootstrapObject = new GameObject("High Card Duel Bootstrap");
            var bootstrap = bootstrapObject.AddComponent<HighCardDuelBootstrap>();
            bootstrap.Build();
        }

        private void Build()
        {
            EnsureMainCamera();
            var audioManager = ConfigureAudioManager();

            var canvas = CreateCanvas();
            var uiRoot = canvas.transform;

            CreateBackground(uiRoot);
            CreateText(uiRoot, "Title", "High Card Duel", 44, new Color(0.96f, 0.98f, 0.94f), TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -54f), new Vector2(620f, 70f), false);

            var roundText = CreateText(uiRoot, "Round", "Round 0/26", 24, new Color(0.83f, 0.91f, 0.86f), TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(360f, 42f), false);

            var playerScoreText = CreateText(uiRoot, "Player Score", "Player: 0", 28, Color.white, TextAnchor.MiddleLeft, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-330f, -112f), new Vector2(250f, 46f), false);
            var cpuScoreText = CreateText(uiRoot, "CPU Score", "CPU: 0", 28, Color.white, TextAnchor.MiddleRight, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(330f, -112f), new Vector2(250f, 46f), false);

            CreateText(uiRoot, "Player Label", "PLAYER", 22, new Color(0.76f, 0.92f, 1f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-235f, 182f), new Vector2(240f, 42f), false);
            CreateText(uiRoot, "CPU Label", "CPU", 22, new Color(1f, 0.84f, 0.76f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(235f, 182f), new Vector2(240f, 42f), false);

            var playerCardDisplay = CreateCardDisplay(uiRoot, "Player Card", new Vector2(-235f, 0f));
            var cpuCardDisplay = CreateCardDisplay(uiRoot, "CPU Card", new Vector2(235f, 0f));

            var statusText = CreateText(uiRoot, "Status", "Ready", 28, new Color(0.96f, 0.98f, 0.94f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 106f), new Vector2(760f, 70f), true);
            var startButton = CreateButton(uiRoot, "Start Button", "Start", new Vector2(0f, 48f), out var startButtonText);

            var scoreDisplay = gameObject.AddComponent<ScoreDisplay>();
            scoreDisplay.Configure(playerScoreText, cpuScoreText, roundText);

            var gameUI = gameObject.AddComponent<GameUI>();
            gameUI.Configure(startButton, startButtonText, statusText);

            var gameController = gameObject.AddComponent<GameController>();
            gameController.Configure(playerCardDisplay, cpuCardDisplay, scoreDisplay, gameUI, audioManager);
            gameUI.Bind(gameController);

            EnsureEventSystem();
        }

        private Canvas CreateCanvas()
        {
            var canvasObject = new GameObject("High Card Duel Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        private static void EnsureMainCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                camera = Object.FindFirstObjectByType<Camera>();
            }

            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                cameraObject.transform.position = new Vector3(0f, 0f, -10f);
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.gameObject.name = "Main Camera";
            camera.gameObject.SetActive(true);
            camera.enabled = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.05f, 0.22f, 0.16f);
            camera.cullingMask = -1;
            camera.targetDisplay = 0;
            camera.depth = 0f;

            if (!camera.CompareTag("MainCamera"))
            {
                camera.tag = "MainCamera";
            }

            if (Object.FindFirstObjectByType<AudioListener>() == null)
            {
                camera.gameObject.AddComponent<AudioListener>();
            }
        }

        private AudioManager ConfigureAudioManager()
        {
            var audioManager = Object.FindFirstObjectByType<AudioManager>();
            if (audioManager == null)
            {
                audioManager = gameObject.AddComponent<AudioManager>();
            }

            audioManager.Configure();
            return audioManager;
        }

        private void CreateBackground(Transform parent)
        {
            var rect = CreateRect("Background", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = GetSprite(ref backgroundSprite, BackgroundAssetPath, BackgroundFallbackAssetPath);
            image.type = Image.Type.Simple;
            image.useSpriteMesh = false;
            image.preserveAspect = true;
            image.color = image.sprite == null ? new Color(0.05f, 0.22f, 0.16f) : Color.white;
            image.raycastTarget = false;

            if (image.sprite != null)
            {
                var fitter = rect.gameObject.AddComponent<AspectRatioFitter>();
                fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fitter.aspectRatio = image.sprite.rect.width / image.sprite.rect.height;
            }
        }

        private CardDisplay CreateCardDisplay(Transform parent, string name, Vector2 anchoredPosition)
        {
            var cardRect = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, new Vector2(250f, 330f));

            var frontRoot = CreateRect("Front Root", cardRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var backRoot = CreateRect("Back Root", cardRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var frontBackground = CreateImage(frontRoot, "Front Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            frontBackground.sprite = GetSprite(ref cardFrontSprite, CardFrontAssetPath, CardFrontFallbackAssetPath);
            frontBackground.preserveAspect = false;

            var backImage = CreateImage(backRoot, "Back Image", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            backImage.sprite = GetSprite(ref cardBackSprite, CardBackAssetPath);
            backImage.preserveAspect = false;

            var topLeftSuit = CreateImage(frontRoot, "Top Left Suit", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(38f, -76f), new Vector2(34f, 34f));
            var centerSuit = CreateImage(frontRoot, "Center Suit", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(92f, 92f));
            var bottomRightSuit = CreateImage(frontRoot, "Bottom Right Suit", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-38f, 76f), new Vector2(34f, 34f));
            var topLeftRank = CreateText(frontRoot, "Top Left Rank", "A", 44, Color.black, TextAnchor.MiddleCenter, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(42f, -34f), new Vector2(78f, 58f), false);
            var bottomRightRank = CreateText(frontRoot, "Bottom Right Rank", "A", 44, Color.black, TextAnchor.MiddleCenter, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-42f, 34f), new Vector2(78f, 58f), false);
            topLeftRank.fontStyle = FontStyle.Bold;
            bottomRightRank.fontStyle = FontStyle.Bold;

            bottomRightSuit.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
            bottomRightRank.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
            topLeftRank.transform.SetAsLastSibling();
            bottomRightRank.transform.SetAsLastSibling();

            var display = cardRect.gameObject.AddComponent<CardDisplay>();
            display.Configure(
                frontRoot.gameObject,
                backRoot.gameObject,
                frontBackground,
                backImage,
                topLeftRank,
                bottomRightRank,
                topLeftSuit,
                centerSuit,
                bottomRightSuit,
                GetSprite(ref cardFrontSprite, CardFrontAssetPath, CardFrontFallbackAssetPath),
                GetSprite(ref cardBackSprite, CardBackAssetPath),
                GetSprite(ref spadesSprite, SpadesAssetPath),
                GetSprite(ref heartsSprite, HeartsAssetPath),
                GetSprite(ref diamondsSprite, DiamondsAssetPath),
                GetSprite(ref clubsSprite, ClubsAssetPath),
                new Color(0.74f, 0.05f, 0.07f),
                new Color(0.08f, 0.08f, 0.08f));

            return display;
        }

        private Sprite GetSprite(ref Sprite cachedSprite, params string[] assetPaths)
        {
            if (cachedSprite != null)
            {
                return cachedSprite;
            }

#if UNITY_EDITOR
            foreach (var assetPath in assetPaths)
            {
                if (string.IsNullOrWhiteSpace(assetPath))
                {
                    continue;
                }

                cachedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (cachedSprite == null)
                {
                    var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                    if (texture != null)
                    {
                        cachedSprite = Sprite.Create(
                            texture,
                            new Rect(0f, 0f, texture.width, texture.height),
                            new Vector2(0.5f, 0.5f),
                            100f,
                            0,
                            SpriteMeshType.FullRect);
                    }
                }

                if (cachedSprite != null)
                {
                    return cachedSprite;
                }
            }
#endif

            return cachedSprite;
        }

        private Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition, out Text buttonText)
        {
            var buttonRect = CreateRect(name, parent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), anchoredPosition, new Vector2(210f, 58f));
            var image = buttonRect.gameObject.AddComponent<Image>();
            image.color = new Color(0.95f, 0.65f, 0.22f);

            var button = buttonRect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            var colors = button.colors;
            colors.normalColor = new Color(0.95f, 0.65f, 0.22f);
            colors.highlightedColor = new Color(1f, 0.76f, 0.33f);
            colors.pressedColor = new Color(0.79f, 0.49f, 0.14f);
            colors.disabledColor = new Color(0.47f, 0.49f, 0.45f);
            button.colors = colors;

            buttonText = CreateText(buttonRect, "Label", label, 24, new Color(0.09f, 0.11f, 0.11f), TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, false);
            buttonText.fontStyle = FontStyle.Bold;

            return button;
        }

        private Image CreateImage(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            var rect = CreateRect(name, parent, anchorMin, anchorMax, anchoredPosition, sizeDelta);
            var image = rect.gameObject.AddComponent<Image>();
            image.type = Image.Type.Simple;
            image.useSpriteMesh = false;
            image.raycastTarget = false;
            return image;
        }

        private Text CreateText(
            Transform parent,
            string name,
            string value,
            int fontSize,
            Color color,
            TextAnchor alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            bool bestFit)
        {
            var rect = CreateRect(name, parent, anchorMin, anchorMax, anchoredPosition, sizeDelta);
            var text = rect.gameObject.AddComponent<Text>();
            text.text = value;
            text.font = GetDefaultFont();
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            text.resizeTextForBestFit = bestFit;
            text.resizeTextMinSize = 18;
            text.resizeTextMaxSize = fontSize;
            return text;
        }

        private RectTransform CreateRect(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            var rectObject = new GameObject(name, typeof(RectTransform));
            var rect = rectObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            rect.pivot = new Vector2(0.5f, 0.5f);
            return rect;
        }

        private Font GetDefaultFont()
        {
            if (defaultFont != null)
            {
                return defaultFont;
            }

            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
            {
                defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            if (defaultFont == null)
            {
                defaultFont = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Arial" }, 16);
            }

            return defaultFont;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }
}
