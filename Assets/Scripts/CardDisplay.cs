using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace HighCardDuel
{
    public sealed class CardDisplay : MonoBehaviour
    {
        [SerializeField] private GameObject frontRoot;
        [SerializeField] private GameObject backRoot;
        [SerializeField] private Image frontBackgroundImage;
        [SerializeField] private Image backImage;
        [SerializeField] private Text topLeftRankLabel;
        [SerializeField] private Text bottomRightRankLabel;
        [SerializeField] private Image topLeftSuitIcon;
        [SerializeField] private Image centerSuitIcon;
        [SerializeField] private Image bottomRightSuitIcon;
        [SerializeField] private Sprite frontBackgroundSprite;
        [SerializeField] private Sprite backSprite;
        [SerializeField] private Sprite spadesSprite;
        [SerializeField] private Sprite heartsSprite;
        [SerializeField] private Sprite diamondsSprite;
        [SerializeField] private Sprite clubsSprite;
        [SerializeField] private Color redTextColor = new Color(0.72f, 0.04f, 0.06f);
        [SerializeField] private Color darkTextColor = new Color(0.08f, 0.08f, 0.08f);

        private RectTransform rectTransform;

        public void Configure(
            GameObject frontRoot,
            GameObject backRoot,
            Image frontBackgroundImage,
            Image backImage,
            Text topLeftRankLabel,
            Text bottomRightRankLabel,
            Image topLeftSuitIcon,
            Image centerSuitIcon,
            Image bottomRightSuitIcon,
            Sprite frontBackgroundSprite,
            Sprite backSprite,
            Sprite spadesSprite,
            Sprite heartsSprite,
            Sprite diamondsSprite,
            Sprite clubsSprite,
            Color redTextColor,
            Color darkTextColor)
        {
            this.frontRoot = frontRoot;
            this.backRoot = backRoot;
            this.frontBackgroundImage = frontBackgroundImage;
            this.backImage = backImage;
            this.topLeftRankLabel = topLeftRankLabel;
            this.bottomRightRankLabel = bottomRightRankLabel;
            this.topLeftSuitIcon = topLeftSuitIcon;
            this.centerSuitIcon = centerSuitIcon;
            this.bottomRightSuitIcon = bottomRightSuitIcon;
            this.frontBackgroundSprite = frontBackgroundSprite;
            this.backSprite = backSprite;
            this.spadesSprite = spadesSprite;
            this.heartsSprite = heartsSprite;
            this.diamondsSprite = diamondsSprite;
            this.clubsSprite = clubsSprite;
            this.redTextColor = redTextColor;
            this.darkTextColor = darkTextColor;
            rectTransform = transform as RectTransform;

            ConfigureStaticImages();
            ShowBack();
        }

        public void ShowCard(Card card)
        {
            var rankText = GetRankText(card.Rank);
            var textColor = card.IsRed ? redTextColor : darkTextColor;
            var suitSprite = GetSuitSprite(card.Suit);

            SetRankLabel(topLeftRankLabel, rankText, textColor);
            SetRankLabel(bottomRightRankLabel, rankText, textColor);
            SetSuitIcon(topLeftSuitIcon, suitSprite);
            SetSuitIcon(centerSuitIcon, suitSprite);
            SetSuitIcon(bottomRightSuitIcon, suitSprite);

            SetRootVisible(backRoot, false);
            SetRootVisible(frontRoot, true);
        }

        public void ShowBack()
        {
            SetRootVisible(frontRoot, false);
            SetRootVisible(backRoot, true);
            SetScaleX(1f);
        }

        public IEnumerator RevealCard(Card card, float halfFlipDuration)
        {
            ShowBack();

            yield return AnimateScaleX(1f, 0f, halfFlipDuration);
            ShowCard(card);
            yield return AnimateScaleX(0f, 1f, halfFlipDuration);
        }

        private IEnumerator AnimateScaleX(float from, float to, float duration)
        {
            if (duration <= 0f)
            {
                SetScaleX(to);
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var progress = Mathf.Clamp01(elapsed / duration);
                var easedProgress = Mathf.SmoothStep(0f, 1f, progress);
                SetScaleX(Mathf.Lerp(from, to, easedProgress));
                yield return null;
            }

            SetScaleX(to);
        }

        private void ConfigureStaticImages()
        {
            ConfigureImage(frontBackgroundImage, frontBackgroundSprite, false);
            ConfigureImage(backImage, backSprite, false);
        }

        private static void ConfigureImage(Image image, Sprite sprite, bool preserveAspect)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = sprite;
            image.color = Color.white;
            image.type = Image.Type.Simple;
            image.preserveAspect = preserveAspect;
            image.useSpriteMesh = false;
            image.raycastTarget = false;
            image.gameObject.SetActive(sprite != null);
        }

        private static void SetRankLabel(Text label, string value, Color color)
        {
            if (label == null)
            {
                return;
            }

            label.enabled = true;
            label.text = value;
            label.color = color;
            label.gameObject.SetActive(true);
            label.transform.SetAsLastSibling();
            label.SetAllDirty();
        }

        private static void SetSuitIcon(Image image, Sprite sprite)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = sprite;
            image.color = Color.white;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.useSpriteMesh = false;
            image.gameObject.SetActive(sprite != null);
        }

        private static void SetRootVisible(GameObject root, bool isVisible)
        {
            if (root != null)
            {
                root.SetActive(isVisible);
            }
        }

        private void SetScaleX(float scaleX)
        {
            if (rectTransform == null)
            {
                rectTransform = transform as RectTransform;
            }

            var targetTransform = rectTransform != null ? rectTransform : transform;
            var scale = targetTransform.localScale;
            scale.x = scaleX;
            targetTransform.localScale = scale;
        }

        private Sprite GetSuitSprite(Suit suit)
        {
            switch (suit)
            {
                case Suit.Spades:
                    return spadesSprite;
                case Suit.Hearts:
                    return heartsSprite;
                case Suit.Diamonds:
                    return diamondsSprite;
                case Suit.Clubs:
                    return clubsSprite;
                default:
                    return null;
            }
        }

        private static string GetRankText(Rank rank)
        {
            switch (rank)
            {
                case Rank.Ace:
                    return "A";
                case Rank.King:
                    return "K";
                case Rank.Queen:
                    return "Q";
                case Rank.Jack:
                    return "J";
                case Rank.Ten:
                    return "10";
                default:
                    return ((int)rank).ToString();
            }
        }
    }
}
