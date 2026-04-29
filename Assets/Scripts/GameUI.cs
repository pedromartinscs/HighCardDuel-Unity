using UnityEngine;
using UnityEngine.UI;

namespace HighCardDuel
{
    public sealed class GameUI : MonoBehaviour
    {
        private Button startButton;
        private Text startButtonText;
        private Button callButton;
        private Text callButtonText;
        private Button raiseButton;
        private Text raiseButtonText;
        private Button foldButton;
        private Text foldButtonText;
        private Button playAnotherMatchButton;
        private Button startOverButton;
        private Button quitButton;

        private GameObject endPanel;
        private Text endTitleText;
        private Text endSummaryText;
        private Text statusText;
        private Text roundText;
        private Text potText;
        private Text requiredCallText;
        private Text matchesText;
        private Text bestText;
        private Text[] playerNameTexts;
        private Text[] playerChipTexts;
        private Text[] playerBetTexts;
        private Text[] playerCueTexts;

        public void Configure(
            Button startButton,
            Text startButtonText,
            Button callButton,
            Text callButtonText,
            Button raiseButton,
            Text raiseButtonText,
            Button foldButton,
            Text foldButtonText,
            Button playAnotherMatchButton,
            Button startOverButton,
            Button quitButton,
            GameObject endPanel,
            Text endTitleText,
            Text endSummaryText,
            Text statusText,
            Text roundText,
            Text potText,
            Text requiredCallText,
            Text matchesText,
            Text bestText,
            Text[] playerNameTexts,
            Text[] playerChipTexts,
            Text[] playerBetTexts,
            Text[] playerCueTexts)
        {
            this.startButton = startButton;
            this.startButtonText = startButtonText;
            this.callButton = callButton;
            this.callButtonText = callButtonText;
            this.raiseButton = raiseButton;
            this.raiseButtonText = raiseButtonText;
            this.foldButton = foldButton;
            this.foldButtonText = foldButtonText;
            this.playAnotherMatchButton = playAnotherMatchButton;
            this.startOverButton = startOverButton;
            this.quitButton = quitButton;
            this.endPanel = endPanel;
            this.endTitleText = endTitleText;
            this.endSummaryText = endSummaryText;
            this.statusText = statusText;
            this.roundText = roundText;
            this.potText = potText;
            this.requiredCallText = requiredCallText;
            this.matchesText = matchesText;
            this.bestText = bestText;
            this.playerNameTexts = playerNameTexts;
            this.playerChipTexts = playerChipTexts;
            this.playerBetTexts = playerBetTexts;
            this.playerCueTexts = playerCueTexts;

            SetBettingControlsVisible(false);
            SetEndScreenVisible(false, true);
        }

        public void Bind(GameController gameController)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(gameController.StartSurvival);

            callButton.onClick.RemoveAllListeners();
            callButton.onClick.AddListener(gameController.ChooseCall);

            raiseButton.onClick.RemoveAllListeners();
            raiseButton.onClick.AddListener(gameController.ChooseRaise);

            foldButton.onClick.RemoveAllListeners();
            foldButton.onClick.AddListener(gameController.ChooseFold);

            playAnotherMatchButton.onClick.RemoveAllListeners();
            playAnotherMatchButton.onClick.AddListener(gameController.PlayAnotherMatch);

            startOverButton.onClick.RemoveAllListeners();
            startOverButton.onClick.AddListener(gameController.StartOver);

            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(gameController.QuitForNow);
        }

        public void SetStartEnabled(bool isEnabled)
        {
            if (startButton != null)
            {
                startButton.interactable = isEnabled;
            }
        }

        public void SetStartVisible(bool isVisible)
        {
            if (startButton != null)
            {
                startButton.gameObject.SetActive(isVisible);
            }
        }

        public void SetStartLabel(string label)
        {
            SetText(startButtonText, label);
        }

        public void SetStatus(string status)
        {
            SetText(statusText, status);
        }

        public void SetPlayerInfo(int playerIndex, string playerName, int chips, int committed, bool inRound, bool folded)
        {
            SetTextAt(playerNameTexts, playerIndex, playerName);
            SetTextAt(playerChipTexts, playerIndex, $"{chips} chips");

            var state = inRound ? "Ready" : "Out";
            if (folded)
            {
                state = "Folded";
            }
            else if (committed > 0)
            {
                state = $"In pot: {committed}";
            }

            SetTextAt(playerBetTexts, playerIndex, state);
        }

        public void SetPlayerCue(int playerIndex, string cue)
        {
            SetTextAt(playerCueTexts, playerIndex, cue);
        }

        public void SetRoundInfo(
            int currentRound,
            int totalRounds,
            int pot,
            int requiredCall,
            int matchesSurvived,
            int bestMatches,
            int bestChips)
        {
            SetText(roundText, $"Round {currentRound} / {totalRounds}");
            SetText(potText, $"Pot: {pot}");
            SetText(requiredCallText, requiredCall <= 0 ? "Call: Check" : $"Call: {requiredCall}");
            SetText(matchesText, $"Matches survived: {matchesSurvived}");
            SetText(bestText, $"Best: {bestMatches} matches | {bestChips} chips");
        }

        public void SetActionLabels(string callLabel, string raiseLabel, string foldLabel)
        {
            SetText(callButtonText, callLabel);
            SetText(raiseButtonText, raiseLabel);
            SetText(foldButtonText, foldLabel);
        }

        public void SetBettingControlsVisible(bool isVisible)
        {
            SetButtonVisible(callButton, isVisible);
            SetButtonVisible(raiseButton, isVisible);
            SetButtonVisible(foldButton, isVisible);
        }

        public void SetBettingInteractable(bool canCall, bool canRaise, bool canFold)
        {
            SetButtonInteractable(callButton, canCall);
            SetButtonInteractable(raiseButton, canRaise);
            SetButtonInteractable(foldButton, canFold);
        }

        public void ShowEndScreen(string title, string summary, bool canPlayAnotherMatch)
        {
            SetText(endTitleText, title);
            SetText(endSummaryText, summary);
            SetEndScreenVisible(true, canPlayAnotherMatch);
        }

        public void HideEndScreen()
        {
            SetEndScreenVisible(false, true);
        }

        private void SetEndScreenVisible(bool isVisible, bool canPlayAnotherMatch)
        {
            if (endPanel != null)
            {
                endPanel.SetActive(isVisible);
            }

            if (playAnotherMatchButton != null)
            {
                playAnotherMatchButton.gameObject.SetActive(canPlayAnotherMatch);
                playAnotherMatchButton.interactable = canPlayAnotherMatch;
            }
        }

        private static void SetTextAt(Text[] textArray, int index, string value)
        {
            if (textArray == null || index < 0 || index >= textArray.Length)
            {
                return;
            }

            SetText(textArray[index], value);
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private static void SetButtonVisible(Button button, bool isVisible)
        {
            if (button != null)
            {
                button.gameObject.SetActive(isVisible);
            }
        }

        private static void SetButtonInteractable(Button button, bool isInteractable)
        {
            if (button != null)
            {
                button.interactable = isInteractable;
            }
        }
    }
}
