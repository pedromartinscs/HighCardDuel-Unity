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
            if (Object.FindAnyObjectByType<GameController>() != null)
            {
                return;
            }

            var bootstrapObject = new GameObject("High Card Survival Bootstrap");
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

            var roundText = CreateText(uiRoot, "Round", "Round 0 / 13", 20, new Color(0.83f, 0.91f, 0.86f), TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-455f, -64f), new Vector2(230f, 30f), false);
            var potText = CreateText(uiRoot, "Pot", "Pot: 0", 24, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -4f), new Vector2(190f, 34f), false);
            var requiredCallText = CreateText(uiRoot, "Required Call", "Call: Check", 19, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(430f, -32f), new Vector2(300f, 28f), false);
            var matchesText = CreateText(uiRoot, "Matches Survived", "Matches survived: 0", 20, new Color(0.83f, 0.91f, 0.86f), TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(430f, -64f), new Vector2(300f, 30f), false);
            var bestText = CreateText(uiRoot, "Best", "Best: 0 matches | 100 chips", 16, new Color(1f, 0.86f, 0.55f), TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(235f, -99f), new Vector2(350f, 26f), false);

            var statusText = CreateText(uiRoot, "Status", "Ready", 19, new Color(0.96f, 0.98f, 0.94f), TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(235f, -130f), new Vector2(360f, 32f), true);

            var cardDisplays = new CardDisplay[4];
            var playerNameTexts = new Text[4];
            var playerChipTexts = new Text[4];
            var playerBetTexts = new Text[4];
            var playerCueTexts = new Text[4];

            CreatePlayerSeat(uiRoot, "You", true, new Vector2(0.5f, 0f), new Vector2(0f, 196f), 0.58f, new Color(0.76f, 0.92f, 1f), out cardDisplays[0], out playerNameTexts[0], out playerChipTexts[0], out playerBetTexts[0], out playerCueTexts[0]);
            CreatePlayerSeat(uiRoot, "CPU 1", false, new Vector2(0.5f, 0.5f), new Vector2(-455f, 38f), 0.48f, new Color(1f, 0.84f, 0.76f), out cardDisplays[1], out playerNameTexts[1], out playerChipTexts[1], out playerBetTexts[1], out playerCueTexts[1]);
            CreatePlayerSeat(uiRoot, "CPU 2", false, new Vector2(0.5f, 1f), new Vector2(0f, -168f), 0.48f, new Color(1f, 0.84f, 0.76f), out cardDisplays[2], out playerNameTexts[2], out playerChipTexts[2], out playerBetTexts[2], out playerCueTexts[2]);
            CreatePlayerSeat(uiRoot, "CPU 3", false, new Vector2(0.5f, 0.5f), new Vector2(455f, 38f), 0.48f, new Color(1f, 0.84f, 0.76f), out cardDisplays[3], out playerNameTexts[3], out playerChipTexts[3], out playerBetTexts[3], out playerCueTexts[3]);

            var startButton = CreateButton(uiRoot, "Start Button", "Start Survival", new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(230f, 46f), new Color(0.95f, 0.65f, 0.22f), out var startButtonText);
            var callButton = CreateButton(uiRoot, "Call Button", "Check", new Vector2(0.5f, 0f), new Vector2(-180f, 24f), new Vector2(156f, 44f), new Color(0.95f, 0.65f, 0.22f), out var callButtonText);
            var raiseButton = CreateButton(uiRoot, "Raise Button", "Raise +1", new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(156f, 44f), new Color(0.95f, 0.65f, 0.22f), out var raiseButtonText);
            var foldButton = CreateButton(uiRoot, "Fold Button", "Fold", new Vector2(0.5f, 0f), new Vector2(180f, 24f), new Vector2(156f, 44f), new Color(0.78f, 0.34f, 0.24f), out var foldButtonText);

            var endPanel = CreateEndPanel(uiRoot, out var endTitleText, out var endSummaryText, out var playAnotherMatchButton, out var startOverButton, out var quitButton);

            var gameUI = gameObject.AddComponent<GameUI>();
            gameUI.Configure(
                startButton,
                startButtonText,
                callButton,
                callButtonText,
                raiseButton,
                raiseButtonText,
                foldButton,
                foldButtonText,
                playAnotherMatchButton,
                startOverButton,
                quitButton,
                endPanel,
                endTitleText,
                endSummaryText,
                statusText,
                roundText,
                potText,
                requiredCallText,
                matchesText,
                bestText,
                playerNameTexts,
                playerChipTexts,
                playerBetTexts,
                playerCueTexts);

            var gameController = gameObject.AddComponent<GameController>();
            gameController.Configure(cardDisplays, gameUI, audioManager);
            gameUI.Bind(gameController);

            EnsureEventSystem();
        }

        private Canvas CreateCanvas()
        {
            var canvasObject = new GameObject("High Card Survival Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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
                camera = Object.FindAnyObjectByType<Camera>();
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

            if (Object.FindAnyObjectByType<AudioListener>() == null)
            {
                camera.gameObject.AddComponent<AudioListener>();
            }
        }

        private AudioManager ConfigureAudioManager()
        {
            var audioManager = Object.FindAnyObjectByType<AudioManager>();
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

            var shade = CreateImage(parent, "Table Shade", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            shade.color = new Color(0f, 0f, 0f, 0.18f);
        }

        private void CreatePlayerSeat(
            Transform parent,
            string playerName,
            bool isHuman,
            Vector2 anchor,
            Vector2 anchoredPosition,
            float cardScale,
            Color accentColor,
            out CardDisplay cardDisplay,
            out Text nameText,
            out Text chipText,
            out Text betText,
            out Text cueText)
        {
            var seatSize = isHuman ? new Vector2(350f, 270f) : new Vector2(250f, 220f);
            var seat = CreateRect($"{playerName} Seat", parent, anchor, anchor, anchoredPosition, seatSize);

            var backing = CreateImage(seat, "Seat Backing", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            backing.color = isHuman ? new Color(0.04f, 0.21f, 0.24f, 0.12f) : new Color(0.16f, 0.08f, 0.06f, 0.08f);

            nameText = CreateText(seat, "Name", playerName, isHuman ? 23 : 19, accentColor, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, isHuman ? 118f : 96f), new Vector2(seatSize.x, 28f), false);
            nameText.fontStyle = FontStyle.Bold;

            chipText = CreateText(seat, "Chips", "100 chips", isHuman ? 19 : 16, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, isHuman ? 94f : 73f), new Vector2(seatSize.x, 24f), false);
            cueText = CreateText(seat, "Cue", isHuman ? "Your card is private." : "Hidden", isHuman ? 16 : 13, isHuman ? new Color(1f, 0.93f, 0.65f) : new Color(0.82f, 0.86f, 0.82f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, isHuman ? 70f : 56f), new Vector2(seatSize.x, 24f), true);
            betText = CreateText(seat, "Round Bet", "Ready", isHuman ? 15 : 13, new Color(0.86f, 0.92f, 0.86f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, isHuman ? -116f : 37f), new Vector2(seatSize.x, 22f), false);

            var scaleRoot = CreateRect("Card Scale Root", seat, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, isHuman ? -50f : -58f), new Vector2(250f, 330f));
            scaleRoot.localScale = new Vector3(cardScale, cardScale, 1f);
            cardDisplay = CreateCardDisplay(scaleRoot, "Card", Vector2.zero);
        }

        private GameObject CreateEndPanel(
            Transform parent,
            out Text titleText,
            out Text summaryText,
            out Button playAnotherMatchButton,
            out Button startOverButton,
            out Button quitButton)
        {
            var panel = CreateRect("End Match Panel", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(660f, 350f));
            var panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.color = new Color(0.03f, 0.08f, 0.07f, 0.93f);

            titleText = CreateText(panel, "Title", "Match Survived", 34, new Color(1f, 0.88f, 0.55f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 118f), new Vector2(560f, 56f), false);
            titleText.fontStyle = FontStyle.Bold;
            summaryText = CreateText(panel, "Summary", "", 22, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 35f), new Vector2(560f, 118f), true);

            playAnotherMatchButton = CreateButton(panel, "Play Another Match", "Play Another", new Vector2(0.5f, 0.5f), new Vector2(-215f, -106f), new Vector2(190f, 50f), new Color(0.95f, 0.65f, 0.22f), out _);
            startOverButton = CreateButton(panel, "Start Over", "Start Over", new Vector2(0.5f, 0.5f), new Vector2(0f, -106f), new Vector2(170f, 50f), new Color(0.86f, 0.79f, 0.62f), out _);
            quitButton = CreateButton(panel, "Quit For Now", "Quit for Now", new Vector2(0.5f, 0.5f), new Vector2(205f, -106f), new Vector2(180f, 50f), new Color(0.78f, 0.34f, 0.24f), out _);

            panel.gameObject.SetActive(false);
            return panel.gameObject;
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

        private Button CreateButton(
            Transform parent,
            string name,
            string label,
            Vector2 anchor,
            Vector2 anchoredPosition,
            Vector2 size,
            Color normalColor,
            out Text buttonText)
        {
            var buttonRect = CreateRect(name, parent, anchor, anchor, anchoredPosition, size);
            var image = buttonRect.gameObject.AddComponent<Image>();
            image.color = normalColor;

            var button = buttonRect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            var colors = button.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = Color.Lerp(normalColor, Color.white, 0.18f);
            colors.pressedColor = Color.Lerp(normalColor, Color.black, 0.18f);
            colors.disabledColor = new Color(0.43f, 0.45f, 0.42f);
            button.colors = colors;

            buttonText = CreateText(buttonRect, "Label", label, 22, new Color(0.09f, 0.11f, 0.11f), TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, true);
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
            text.resizeTextMinSize = 12;
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
            if (Object.FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }
}
