using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HighCardDuel
{
    public sealed class GameController : MonoBehaviour
    {
        private const int PlayerCount = 4;
        private const int TotalRounds = 13;
        private const int StartingChips = 100;
        private const int AnteAmount = 1;
        private const int RaiseStep = 1;
        private const int MaxRaisePerRound = 6;
        private const string BestMatchesKey = "HighCardSurvival.BestMatches";
        private const string BestChipsKey = "HighCardSurvival.BestChips";

        [SerializeField] private float shuffleDelaySeconds = 0.5f;
        [SerializeField] private float roundStartPauseSeconds = 0.35f;
        [SerializeField] private float cardFlipHalfDuration = 0.28f;
        [SerializeField] private float betweenCardRevealDelaySeconds = 0.25f;
        [SerializeField] private float bettingActionDelaySeconds = 0.45f;
        [SerializeField] private float scoreUpdateDelaySeconds = 0.35f;
        [SerializeField] private float roundDelaySeconds = 1.1f;

        private readonly System.Random random = new System.Random();
        private readonly List<MatchPlayer> players = new List<MatchPlayer>(PlayerCount);

        private GameUI gameUI;
        private AudioManager audioManager;
        private Coroutine playRoutine;
        private HumanBetAction pendingHumanAction;
        private bool waitingForHumanAction;
        private int currentRound;
        private int humanChips = StartingChips;
        private int matchesSurvived;
        private int bestMatchesSurvived;
        private int bestChipCount = StartingChips;
        private int pot;
        private int carryOverPot;
        private int requiredCommitment;
        private int raisedThisRound;

        private enum HumanBetAction
        {
            None,
            Call,
            Raise,
            Fold
        }

        private enum CpuBetAction
        {
            Call,
            Raise,
            Fold
        }

        private sealed class MatchPlayer
        {
            public readonly string Name;
            public readonly bool IsHuman;
            public readonly CardDisplay CardDisplay;
            public readonly List<Card> Hand = new List<Card>(TotalRounds);

            public int Chips;
            public int Committed;
            public Card CurrentCard;
            public bool InRound;
            public bool Folded;
            public bool HasActed;

            public MatchPlayer(string name, bool isHuman, CardDisplay cardDisplay)
            {
                Name = name;
                IsHuman = isHuman;
                CardDisplay = cardDisplay;
            }
        }

        public void Configure(CardDisplay[] cardDisplays, GameUI gameUI, AudioManager audioManager = null)
        {
            if (cardDisplays == null || cardDisplays.Length < PlayerCount)
            {
                throw new ArgumentException("High Card Survival needs four card displays.", nameof(cardDisplays));
            }

            this.gameUI = gameUI;
            this.audioManager = audioManager;

            players.Clear();
            players.Add(new MatchPlayer("You", true, cardDisplays[0]));
            players.Add(new MatchPlayer("CPU 1", false, cardDisplays[1]));
            players.Add(new MatchPlayer("CPU 2", false, cardDisplays[2]));
            players.Add(new MatchPlayer("CPU 3", false, cardDisplays[3]));

            LoadRecords();
            ResetTablePreview();

            gameUI.SetStatus("Ready for High Card Survival.");
            gameUI.SetStartLabel("Start Survival");
            gameUI.SetStartEnabled(true);
            gameUI.SetStartVisible(true);
            gameUI.SetBettingControlsVisible(false);
            gameUI.HideEndScreen();
        }

        public void StartSurvival()
        {
            if (playRoutine != null)
            {
                return;
            }

            humanChips = StartingChips;
            matchesSurvived = 0;
            BeginMatch();
        }

        public void PlayAnotherMatch()
        {
            if (playRoutine != null || humanChips <= 0)
            {
                return;
            }

            BeginMatch();
        }

        public void StartOver()
        {
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                playRoutine = null;
            }

            humanChips = StartingChips;
            matchesSurvived = 0;
            BeginMatch();
        }

        public void QuitForNow()
        {
            gameUI.SetStatus("Thanks for playing High Card Survival.");
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void ChooseCall()
        {
            ChooseHumanAction(HumanBetAction.Call);
        }

        public void ChooseRaise()
        {
            ChooseHumanAction(HumanBetAction.Raise);
        }

        public void ChooseFold()
        {
            ChooseHumanAction(HumanBetAction.Fold);
        }

        private void ChooseHumanAction(HumanBetAction action)
        {
            if (!waitingForHumanAction)
            {
                return;
            }

            pendingHumanAction = action;
        }

        private void BeginMatch()
        {
            carryOverPot = 0;
            pot = 0;
            gameUI.HideEndScreen();
            gameUI.SetStartVisible(false);
            gameUI.SetBettingControlsVisible(false);
            playRoutine = StartCoroutine(PlayMatch());
        }

        private IEnumerator PlayMatch()
        {
            ResetPlayersForMatch();
            DealMatchHands();
            UpdateAllPlayerInfo();
            UpdateRoundInfo();

            gameUI.SetStatus("Shuffling a new 52-card deck...");
            yield return new WaitForSeconds(shuffleDelaySeconds);

            for (currentRound = 1; currentRound <= TotalRounds; currentRound++)
            {
                if (humanChips <= 0)
                {
                    ShowGameOver();
                    yield break;
                }

                yield return PlayRound();

                if (humanChips <= 0)
                {
                    ShowGameOver();
                    yield break;
                }
            }

            matchesSurvived++;
            UpdateRecords();
            ShowEndOfMatch();
        }

        private IEnumerator PlayRound()
        {
            PrepareRound();
            UpdateAllPlayerInfo();
            UpdateRoundInfo();

            gameUI.SetStatus($"Round {currentRound}: everyone antes 1 chip.");
            yield return new WaitForSeconds(roundStartPauseSeconds);

            yield return RunAntePhase();

            if (CountContenders() <= 1)
            {
                yield return ResolveRound();
                yield break;
            }

            yield return ShowPrivateCardPhase();
            yield return RunBettingPhase();
            gameUI.SetBettingControlsVisible(false);

            yield return RevealRemainingCards();
            yield return ResolveRound();
        }

        private void PrepareRound()
        {
            pot = carryOverPot;
            carryOverPot = 0;
            requiredCommitment = 0;
            raisedThisRound = 0;

            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];
                player.Committed = 0;
                player.Folded = false;
                player.HasActed = false;
                player.InRound = player.Chips > 0;
                player.CurrentCard = player.Hand[currentRound - 1];
                player.CardDisplay.ShowBack();
                gameUI.SetPlayerCue(i, player.IsHuman ? "Your card is private." : "Hidden");
            }
        }

        private IEnumerator RunAntePhase()
        {
            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (!player.InRound)
                {
                    gameUI.SetPlayerCue(i, "Out of chips");
                    continue;
                }

                var paid = PayChips(player, AnteAmount);
                player.InRound = paid > 0;

                if (paid > 0)
                {
                    gameUI.SetStatus(player.IsHuman ? "You ante 1 chip." : $"{player.Name} antes 1 chip.");
                }
                else
                {
                    gameUI.SetStatus($"{player.Name} cannot ante and sits out.");
                }

                UpdateAllPlayerInfo();
                UpdateRoundInfo();
                yield return new WaitForSeconds(bettingActionDelaySeconds);
            }

            requiredCommitment = AnteAmount;
        }

        private IEnumerator ShowPrivateCardPhase()
        {
            for (var i = 1; i < players.Count; i++)
            {
                if (players[i].InRound)
                {
                    players[i].CardDisplay.ShowBack();
                    gameUI.SetPlayerCue(i, "Hidden until reveal");
                }
            }

            var human = players[0];
            gameUI.SetPlayerCue(0, "Your card - only you can see it");
            gameUI.SetStatus("Private card view: only your card is face-up.");
            audioManager?.PlayCardFlip();
            yield return human.CardDisplay.RevealCard(human.CurrentCard, cardFlipHalfDuration);
            yield return new WaitForSeconds(betweenCardRevealDelaySeconds);
        }

        private IEnumerator RunBettingPhase()
        {
            gameUI.SetBettingControlsVisible(true);
            gameUI.SetBettingInteractable(false, false, false);

            var playerIndex = 0;
            while (!IsBettingResolved())
            {
                var player = players[playerIndex];

                if (player.InRound && !player.Folded && player.Chips <= 0 && player.Committed < requiredCommitment)
                {
                    FoldPlayer(player);
                    gameUI.SetStatus(player.IsHuman ? "You cannot match the bet and folded." : $"{player.Name} cannot match and folded.");
                    yield return new WaitForSeconds(bettingActionDelaySeconds);
                }
                else if (CanPlayerAct(player))
                {
                    if (player.IsHuman)
                    {
                        yield return WaitForHumanBet(player);
                    }
                    else
                    {
                        yield return RunCpuBet(player);
                    }
                }

                playerIndex = (playerIndex + 1) % players.Count;
                yield return null;
            }

            gameUI.SetBettingInteractable(false, false, false);
        }

        private IEnumerator WaitForHumanBet(MatchPlayer human)
        {
            pendingHumanAction = HumanBetAction.None;
            waitingForHumanAction = true;

            var callAmount = GetCallAmount(human);
            var canCall = callAmount <= human.Chips;
            var canRaise = CanRaise(human);

            gameUI.SetActionLabels(callAmount == 0 ? "Check" : $"Call {callAmount}", $"Raise +{RaiseStep}", "Fold");
            gameUI.SetBettingInteractable(canCall, canRaise, true);
            gameUI.SetStatus(callAmount == 0 ? "Your move: check, raise, or fold." : $"Your move: call {callAmount}, raise, or fold.");
            UpdateRoundInfo();

            while (pendingHumanAction == HumanBetAction.None)
            {
                yield return null;
            }

            waitingForHumanAction = false;
            gameUI.SetBettingInteractable(false, false, false);

            switch (pendingHumanAction)
            {
                case HumanBetAction.Call:
                    if (canCall)
                    {
                        CallPlayer(human);
                    }
                    else
                    {
                        FoldPlayer(human);
                        gameUI.SetStatus("You could not call and folded.");
                    }

                    break;
                case HumanBetAction.Raise:
                    if (canRaise)
                    {
                        RaisePlayer(human);
                    }
                    else
                    {
                        gameUI.SetStatus("Raise is capped for this round.");
                        human.HasActed = true;
                    }

                    break;
                case HumanBetAction.Fold:
                    FoldPlayer(human);
                    gameUI.SetStatus("You folded.");
                    break;
            }

            pendingHumanAction = HumanBetAction.None;
            UpdateAllPlayerInfo();
            UpdateRoundInfo();
            yield return new WaitForSeconds(bettingActionDelaySeconds);
        }

        private IEnumerator RunCpuBet(MatchPlayer cpu)
        {
            yield return new WaitForSeconds(bettingActionDelaySeconds);

            var action = ChooseCpuAction(cpu);
            switch (action)
            {
                case CpuBetAction.Raise:
                    RaisePlayer(cpu);
                    break;
                case CpuBetAction.Fold:
                    FoldPlayer(cpu);
                    gameUI.SetStatus($"{cpu.Name} folded.");
                    break;
                default:
                    CallPlayer(cpu);
                    break;
            }

            UpdateAllPlayerInfo();
            UpdateRoundInfo();
        }

        private CpuBetAction ChooseCpuAction(MatchPlayer cpu)
        {
            var callAmount = GetCallAmount(cpu);
            var value = cpu.CurrentCard.Value;

            if (callAmount > cpu.Chips)
            {
                return CpuBetAction.Fold;
            }

            if (CanRaise(cpu))
            {
                if (value >= (int)Rank.King)
                {
                    return CpuBetAction.Raise;
                }

                if (value >= (int)Rank.Queen && callAmount <= 1 && raisedThisRound <= 2)
                {
                    return CpuBetAction.Raise;
                }

                if (value >= (int)Rank.Jack && callAmount == 0 && ((currentRound + (int)cpu.CurrentCard.Suit) % 2 == 0))
                {
                    return CpuBetAction.Raise;
                }
            }

            if (callAmount == 0)
            {
                return CpuBetAction.Call;
            }

            if (value >= (int)Rank.Ten)
            {
                return CpuBetAction.Call;
            }

            if (value >= (int)Rank.Eight && callAmount <= 2)
            {
                return CpuBetAction.Call;
            }

            if (value >= (int)Rank.Six && callAmount <= 1)
            {
                return CpuBetAction.Call;
            }

            return CpuBetAction.Fold;
        }

        private IEnumerator RevealRemainingCards()
        {
            gameUI.SetStatus("Reveal: remaining players show their cards.");

            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (!player.InRound || player.Folded)
                {
                    gameUI.SetPlayerCue(i, player.Folded ? "Folded" : "Out");
                    continue;
                }

                gameUI.SetPlayerCue(i, "Revealed");

                if (player.IsHuman)
                {
                    player.CardDisplay.ShowCard(player.CurrentCard);
                    continue;
                }

                audioManager?.PlayCardFlip();
                yield return player.CardDisplay.RevealCard(player.CurrentCard, cardFlipHalfDuration);
                yield return new WaitForSeconds(betweenCardRevealDelaySeconds);
            }
        }

        private IEnumerator ResolveRound()
        {
            var contenders = GetContenders();
            if (contenders.Count == 0)
            {
                carryOverPot = pot;
                gameUI.SetStatus("No winner this round. Pot carries over.");
                yield return new WaitForSeconds(roundDelaySeconds);
                yield break;
            }

            var winners = FindRoundWinners(contenders);
            var payout = pot / winners.Count;
            var leftover = pot % winners.Count;

            for (var i = 0; i < winners.Count; i++)
            {
                winners[i].Chips += payout;
                if (winners[i].IsHuman)
                {
                    humanChips = winners[i].Chips;
                }
            }

            carryOverPot = leftover;
            pot = leftover;
            UpdateAllPlayerInfo();
            UpdateRecords();
            UpdateRoundInfo();

            if (winners.Count == 1)
            {
                var winner = winners[0];
                if (contenders.Count == 1)
                {
                    gameUI.SetStatus(winner.IsHuman ? "You won the pot after everyone else folded!" : $"{winner.Name} wins after everyone else folded.");
                }
                else
                {
                    gameUI.SetStatus(winner.IsHuman ? $"You won the pot with {GetRankName(winner.CurrentCard)}!" : $"{winner.Name} wins with {GetRankName(winner.CurrentCard)}.");
                }
            }
            else
            {
                gameUI.SetStatus(leftover > 0 ? $"Tie! Pot split. {leftover} chip carries over." : "Tie! Pot split.");
            }

            audioManager?.PlayScorePoint();
            yield return new WaitForSeconds(scoreUpdateDelaySeconds);
            yield return new WaitForSeconds(roundDelaySeconds);
        }

        private void ResetPlayersForMatch()
        {
            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];
                player.Chips = player.IsHuman ? humanChips : StartingChips;
                player.Committed = 0;
                player.Folded = false;
                player.HasActed = false;
                player.InRound = player.Chips > 0;
                player.Hand.Clear();
                player.CardDisplay.ShowBack();
                gameUI.SetPlayerCue(i, player.IsHuman ? "Your card is private." : "Hidden");
            }
        }

        private void DealMatchHands()
        {
            var deck = Deck.CreateStandard52();
            deck.Shuffle(random);

            for (var round = 0; round < TotalRounds; round++)
            {
                for (var playerIndex = 0; playerIndex < players.Count; playerIndex++)
                {
                    players[playerIndex].Hand.Add(deck.Draw());
                }
            }
        }

        private void ResetTablePreview()
        {
            currentRound = 0;
            pot = 0;
            carryOverPot = 0;
            requiredCommitment = 0;

            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];
                player.Chips = player.IsHuman ? humanChips : StartingChips;
                player.Committed = 0;
                player.Folded = false;
                player.InRound = true;
                player.CardDisplay.ShowBack();
                gameUI.SetPlayerCue(i, player.IsHuman ? "Your card is private." : "Hidden");
            }

            UpdateAllPlayerInfo();
            UpdateRoundInfo();
        }

        private bool CanPlayerAct(MatchPlayer player)
        {
            if (!player.InRound || player.Folded || CountContenders() <= 1 || player.Chips <= 0)
            {
                return false;
            }

            return !player.HasActed || player.Committed < requiredCommitment;
        }

        private bool IsBettingResolved()
        {
            if (CountContenders() <= 1)
            {
                return true;
            }

            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (!player.InRound || player.Folded || player.Chips <= 0)
                {
                    continue;
                }

                if (!player.HasActed || player.Committed < requiredCommitment)
                {
                    return false;
                }
            }

            return true;
        }

        private void CallPlayer(MatchPlayer player)
        {
            var callAmount = GetCallAmount(player);
            if (callAmount > 0)
            {
                PayChips(player, callAmount);
                gameUI.SetStatus(player.IsHuman ? $"You called {callAmount}." : $"{player.Name} called.");
            }
            else
            {
                gameUI.SetStatus(player.IsHuman ? "You checked." : $"{player.Name} checked.");
            }

            player.HasActed = true;
        }

        private void RaisePlayer(MatchPlayer player)
        {
            var callAmount = GetCallAmount(player);
            var raiseAmount = GetRaiseAmount(player);
            if (raiseAmount <= 0 || callAmount + raiseAmount > player.Chips)
            {
                player.HasActed = true;
                return;
            }

            PayChips(player, callAmount + raiseAmount);
            requiredCommitment += raiseAmount;
            raisedThisRound += raiseAmount;

            for (var i = 0; i < players.Count; i++)
            {
                var other = players[i];
                if (other != player && other.InRound && !other.Folded)
                {
                    other.HasActed = false;
                }
            }

            player.HasActed = true;
            gameUI.SetStatus(player.IsHuman ? $"You raised by {raiseAmount}." : $"{player.Name} raised.");
        }

        private void FoldPlayer(MatchPlayer player)
        {
            player.Folded = true;
            player.HasActed = true;
            gameUI.SetPlayerCue(players.IndexOf(player), "Folded");
        }

        private int PayChips(MatchPlayer player, int amount)
        {
            var paid = Mathf.Clamp(amount, 0, player.Chips);
            player.Chips -= paid;
            player.Committed += paid;
            pot += paid;

            if (player.IsHuman)
            {
                humanChips = player.Chips;
            }

            return paid;
        }

        private bool CanRaise(MatchPlayer player)
        {
            if (!player.InRound || player.Folded || raisedThisRound >= MaxRaisePerRound)
            {
                return false;
            }

            var callAmount = GetCallAmount(player);
            var raiseAmount = GetRaiseAmount(player);
            return raiseAmount > 0 && callAmount + raiseAmount <= player.Chips;
        }

        private int GetRaiseAmount(MatchPlayer player)
        {
            if (player.Chips <= 0)
            {
                return 0;
            }

            return Mathf.Min(RaiseStep, MaxRaisePerRound - raisedThisRound);
        }

        private int GetCallAmount(MatchPlayer player)
        {
            if (player == null)
            {
                return 0;
            }

            return Mathf.Max(0, requiredCommitment - player.Committed);
        }

        private int CountContenders()
        {
            var count = 0;
            for (var i = 0; i < players.Count; i++)
            {
                if (players[i].InRound && !players[i].Folded)
                {
                    count++;
                }
            }

            return count;
        }

        private List<MatchPlayer> GetContenders()
        {
            var contenders = new List<MatchPlayer>(PlayerCount);
            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player.InRound && !player.Folded)
                {
                    contenders.Add(player);
                }
            }

            return contenders;
        }

        private static List<MatchPlayer> FindRoundWinners(List<MatchPlayer> contenders)
        {
            var winners = new List<MatchPlayer>(contenders.Count);
            var highestValue = 0;

            for (var i = 0; i < contenders.Count; i++)
            {
                var player = contenders[i];
                if (player.CurrentCard.Value > highestValue)
                {
                    winners.Clear();
                    winners.Add(player);
                    highestValue = player.CurrentCard.Value;
                }
                else if (player.CurrentCard.Value == highestValue)
                {
                    winners.Add(player);
                }
            }

            return winners;
        }

        private void UpdateAllPlayerInfo()
        {
            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];
                gameUI.SetPlayerInfo(i, player.Name, player.Chips, player.Committed, player.InRound, player.Folded);
            }
        }

        private void UpdateRoundInfo()
        {
            gameUI.SetRoundInfo(
                currentRound,
                TotalRounds,
                pot,
                GetCallAmount(players.Count > 0 ? players[0] : null),
                matchesSurvived,
                bestMatchesSurvived,
                bestChipCount);
        }

        private void ShowEndOfMatch()
        {
            playRoutine = null;
            gameUI.SetBettingControlsVisible(false);
            gameUI.SetStartVisible(false);
            gameUI.SetStatus($"Match survived with {humanChips} chips.");
            audioManager?.PlayVictory();

            var summary = $"You survived all {TotalRounds} rounds.\nChips: {humanChips}\nMatches survived: {matchesSurvived}\nBest: {bestMatchesSurvived} matches, {bestChipCount} chips";
            gameUI.ShowEndScreen("Match Survived", summary, true);
        }

        private void ShowGameOver()
        {
            playRoutine = null;
            humanChips = Mathf.Max(0, humanChips);
            UpdateRecords();
            UpdateAllPlayerInfo();
            UpdateRoundInfo();
            gameUI.SetBettingControlsVisible(false);
            gameUI.SetStartVisible(false);
            gameUI.SetStatus("Game Over. You ran out of chips.");

            var summary = $"You survived {matchesSurvived} match(es).\nHighest chip count: {bestChipCount}\nBest matches survived: {bestMatchesSurvived}";
            gameUI.ShowEndScreen("Game Over", summary, false);
        }

        private void LoadRecords()
        {
            bestMatchesSurvived = PlayerPrefs.GetInt(BestMatchesKey, 0);
            bestChipCount = PlayerPrefs.GetInt(BestChipsKey, StartingChips);
        }

        private void UpdateRecords()
        {
            if (matchesSurvived > bestMatchesSurvived)
            {
                bestMatchesSurvived = matchesSurvived;
                PlayerPrefs.SetInt(BestMatchesKey, bestMatchesSurvived);
            }

            if (humanChips > bestChipCount)
            {
                bestChipCount = humanChips;
                PlayerPrefs.SetInt(BestChipsKey, bestChipCount);
            }

            PlayerPrefs.Save();
        }

        private static string GetRankName(Card card)
        {
            switch (card.Rank)
            {
                case Rank.Ace:
                    return "Ace";
                case Rank.King:
                    return "King";
                case Rank.Queen:
                    return "Queen";
                case Rank.Jack:
                    return "Jack";
                default:
                    return ((int)card.Rank).ToString();
            }
        }
    }
}
