using System;
using System.Collections;
using UnityEngine;

namespace HighCardDuel
{
    public sealed class GameController : MonoBehaviour
    {
        private const int TotalRounds = 26;

        [SerializeField] private float shuffleDelaySeconds = 0.5f;
        [SerializeField] private float roundStartPauseSeconds = 0.35f;
        [SerializeField] private float cardFlipHalfDuration = 0.28f;
        [SerializeField] private float betweenCardRevealDelaySeconds = 0.35f;
        [SerializeField] private float scoreUpdateDelaySeconds = 0.25f;
        [SerializeField] private float roundDelaySeconds = 1.2f;

        private readonly System.Random random = new System.Random();

        private CardDisplay playerCardDisplay;
        private CardDisplay cpuCardDisplay;
        private ScoreDisplay scoreDisplay;
        private GameUI gameUI;
        private AudioManager audioManager;
        private Coroutine playRoutine;

        public void Configure(
            CardDisplay playerCardDisplay,
            CardDisplay cpuCardDisplay,
            ScoreDisplay scoreDisplay,
            GameUI gameUI,
            AudioManager audioManager = null)
        {
            this.playerCardDisplay = playerCardDisplay;
            this.cpuCardDisplay = cpuCardDisplay;
            this.scoreDisplay = scoreDisplay;
            this.gameUI = gameUI;
            this.audioManager = audioManager;

            scoreDisplay.ResetScores(TotalRounds);
            gameUI.SetStatus("Ready");
            gameUI.SetStartLabel("Start");
            gameUI.SetStartEnabled(true);
        }

        public void StartDuel()
        {
            if (playRoutine != null)
            {
                return;
            }

            playRoutine = StartCoroutine(PlayDuel());
        }

        private IEnumerator PlayDuel()
        {
            var playerScore = 0;
            var cpuScore = 0;

            gameUI.SetStartEnabled(false);
            gameUI.SetStatus("Shuffling deck...");
            scoreDisplay.ResetScores(TotalRounds);
            playerCardDisplay.ShowBack();
            cpuCardDisplay.ShowBack();

            var fullDeck = Deck.CreateStandard52();
            fullDeck.Shuffle(random);
            var splitDecks = fullDeck.SplitEvenly();
            var playerDeck = splitDecks.Item1;
            var cpuDeck = splitDecks.Item2;

            yield return new WaitForSeconds(shuffleDelaySeconds);

            for (var round = 1; round <= TotalRounds; round++)
            {
                playerCardDisplay.ShowBack();
                cpuCardDisplay.ShowBack();

                var playerCard = playerDeck.Draw();
                var cpuCard = cpuDeck.Draw();

                scoreDisplay.SetRound(round, TotalRounds);
                gameUI.SetStatus($"Round {round}");
                yield return new WaitForSeconds(roundStartPauseSeconds);

                gameUI.SetStatus($"Round {round}: player card");
                audioManager?.PlayCardFlip();
                yield return playerCardDisplay.RevealCard(playerCard, cardFlipHalfDuration);

                yield return new WaitForSeconds(betweenCardRevealDelaySeconds);

                gameUI.SetStatus($"Round {round}: CPU card");
                audioManager?.PlayCardFlip();
                yield return cpuCardDisplay.RevealCard(cpuCard, cardFlipHalfDuration);

                var roundStatus = ScoreRound(playerCard, cpuCard, ref playerScore, ref cpuScore);

                gameUI.SetStatus($"Round {round}: {roundStatus}");
                yield return new WaitForSeconds(scoreUpdateDelaySeconds);

                scoreDisplay.SetScores(playerScore, cpuScore);
                audioManager?.PlayScorePoint();

                yield return new WaitForSeconds(roundDelaySeconds);
            }

            gameUI.SetStatus(BuildFinalStatus(playerScore, cpuScore));
            audioManager?.PlayVictory();
            gameUI.SetStartLabel("Play Again");
            gameUI.SetStartEnabled(true);
            playRoutine = null;
        }

        private static string ScoreRound(Card playerCard, Card cpuCard, ref int playerScore, ref int cpuScore)
        {
            if (playerCard.Value > cpuCard.Value)
            {
                playerScore++;
                return $"Player wins with {playerCard} over {cpuCard}";
            }

            if (cpuCard.Value > playerCard.Value)
            {
                cpuScore++;
                return $"CPU wins with {cpuCard} over {playerCard}";
            }

            playerScore++;
            cpuScore++;
            return $"Tie on {playerCard.Rank}. Both score";
        }

        private static string BuildFinalStatus(int playerScore, int cpuScore)
        {
            if (playerScore > cpuScore)
            {
                return $"Player wins {playerScore}-{cpuScore}";
            }

            if (cpuScore > playerScore)
            {
                return $"CPU wins {cpuScore}-{playerScore}";
            }

            return $"Draw {playerScore}-{cpuScore}";
        }
    }
}
