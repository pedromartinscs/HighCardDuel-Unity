# High Card Duel

High Card Duel is a small Unity 2D UI card game demo.

Open `Assets/Scenes/Main.unity`, press Play, then press Start. The demo shuffles a standard 52-card deck, splits it into two 26-card decks, and automatically plays every round with a short coroutine delay.

## Rules

- One card is revealed for the player and one for the CPU each round.
- The higher rank gains 1 point.
- If both ranks match, both players gain 1 point.
- After 26 rounds, the UI shows the winner.

## Architecture

- `Card` models a single playing card.
- `Deck` creates, shuffles, splits, and draws from the standard deck.
- `GameController` owns the game loop and coroutine timing.
- `CardDisplay` renders a card label in Unity UI.
- `ScoreDisplay` renders scores and round progress.
- `GameUI` handles button/status UI state.
- `HighCardDuelBootstrap` builds the Canvas-based demo UI at runtime.
