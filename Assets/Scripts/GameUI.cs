using UnityEngine;
using UnityEngine.UI;

namespace HighCardDuel
{
    public sealed class GameUI : MonoBehaviour
    {
        private Button startButton;
        private Text startButtonText;
        private Text statusText;

        public void Configure(Button startButton, Text startButtonText, Text statusText)
        {
            this.startButton = startButton;
            this.startButtonText = startButtonText;
            this.statusText = statusText;
        }

        public void Bind(GameController gameController)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(gameController.StartDuel);
        }

        public void SetStartEnabled(bool isEnabled)
        {
            startButton.interactable = isEnabled;
        }

        public void SetStartLabel(string label)
        {
            startButtonText.text = label;
        }

        public void SetStatus(string status)
        {
            statusText.text = status;
        }
    }
}
