using UnityEngine;
using UnityEngine.UI;

namespace HighCardDuel
{
    public sealed class ScoreDisplay : MonoBehaviour
    {
        private Text playerScoreText;
        private Text cpuScoreText;
        private Text roundText;

        public void Configure(Text playerScoreText, Text cpuScoreText, Text roundText)
        {
            this.playerScoreText = playerScoreText;
            this.cpuScoreText = cpuScoreText;
            this.roundText = roundText;
        }

        public void ResetScores(int totalRounds)
        {
            SetScores(0, 0);
            SetRound(0, totalRounds);
        }

        public void SetScores(int playerScore, int cpuScore)
        {
            playerScoreText.text = $"Player: {playerScore}";
            cpuScoreText.text = $"CPU: {cpuScore}";
        }

        public void SetRound(int currentRound, int totalRounds)
        {
            roundText.text = $"Round {currentRound}/{totalRounds}";
        }
    }
}
