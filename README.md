# KnightVision - Chess Bot

<div align="center">
 
  <img src="https://github.com/redayzarra/KnightVision/assets/113388793/679a66a3-73c2-41ac-9d45-383e246bc0c6" alt="KnightVision screenshot">

KnightVision forks me :(
</div>

## Overview

KnightVision is a chess bot created for the [Chess Coding Challenge](https://github.com/SebLague/Chess-Challenge). Built with C# using the .NET 6.0 framework, this bot focuses on adapting to opponent moves and ensuring efficient performance within the competition's 256MB memory limit. KnightVision employs sophisticated strategies to play strong chess while adhering to the competition's rules and constraints.

## What the Bot Does

KnightVision uses a combination of advanced techniques to make its moves:

1. **[Opening Book](https://www.chessprogramming.org/Opening_Book):**
   - The bot leverages a compact opening book to guide early-game play, utilizing a dictionary to store common openings and defenses. This ensures efficient opening moves without consuming excessive memory.

2. **[Iterative Deepening](https://www.chessprogramming.org/Iterative_Deepening):**
   - KnightVision employs iterative deepening to search for the best moves, gradually increasing the depth of its search until it finds the optimal move within the time constraints.

3. **[Negamax Algorithm](https://www.chessprogramming.org/Negamax) with [Alpha-Beta Pruning](https://www.chessprogramming.org/Alpha-Beta):**
   - The core of the bot's decision-making process is based on the Negamax algorithm enhanced with alpha-beta pruning. This technique efficiently searches the game tree to evaluate the best possible moves while pruning suboptimal branches to save time.

4. **[Transposition Table](https://www.chessprogramming.org/Transposition_Table):**
   - The bot uses a transposition table to cache board states and avoid recalculating positions it has already evaluated. This optimization significantly speeds up the search process.

5. **[Quiescence Search](https://www.chessprogramming.org/Quiescence_Search):**
   - To avoid the horizon effect, KnightVision employs quiescence search, which extends the search depth for capturing moves to ensure a more stable evaluation of positions.

6. **[Move Ordering](https://www.chessprogramming.org/Move_Ordering):**
   - Moves are ordered using a heuristic that prioritizes captures (most valuable victim and least valuable attacker) and historical data, improving the efficiency of the alpha-beta pruning.

7. **[Evaluation Function](https://www.chessprogramming.org/Evaluation):**
   - The bot uses a sophisticated evaluation function that takes into account various factors such as material balance, piece activity, king safety, and pawn structure. Precomputed evaluation weights are used to quickly assess the value of different positions.

8. **[Endgame Knowledge](https://www.chessprogramming.org/Endgame):**
   - KnightVision has specific strategies for endgame play, including recognizing basic endgame patterns, detecting stalemate scenarios, and adjusting its evaluation function to prioritize critical endgame principles like king centralization and pawn promotion.

## Usage

To run KnightVision, follow these steps:

1. Clone the repository.
2. Open the project in your preferred C# IDE.
3. Build the project using .NET 6.0.
4. Run the bot using the provided commands.

## License

This project is licensed under the MIT License. See the LICENSE file for details.

## Acknowledgments

- Thanks to the [Chess Coding Challenge](https://github.com/SebLague/Chess-Challenge) for inspiring this project.

For any questions or contributions, please contact the project maintainer or open an issue on the GitHub repository.
