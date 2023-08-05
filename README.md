# RookNoFurther - Chess Bot

## Overview

RookNoFurther is a chess bot created for this [Chess Coding Challenge](https://github.com/SebLague/Chess-Challenge). It's built with C# using the .NET 6.0 framework. The bot focuses on adapting to opponent moves and ensuring it works efficiently within the competition's 256mb memory limit. While it uses some complex strategies behind the scenes, the main goal is to play strong chess while staying within the competition's rules and constraints.

## My Plan 

### 1. Opening Book

I can't have a full-fledged opening book since that would take up way too much space, however, I can give my bot a bunch of common openings and some defenses. I am planning on handpicking lines like King's Pawn (e4), Queen's Pawn (d4), and some defenses like the Sicilian, French, and King's Indian Defense. I can use a dictionary where the keys are the moves played, and the value is the next best move. 

#### Solution

Use an array or dictionary to store the moves. The key can be the concatenation of moves played so far, and the value can be the best move or a list of good moves for that position.

#### Execution

- At the start of each game, before diving into the deeper search, the bot should check the opening book for the current board state.
- If the position exists in the opening book, play the corresponding move. If there's a list of moves, you can either pick one randomly or choose the most optimal based on further criteria.
- Once you're out of the opening phase (or if a position is not found in the book), the bot will revert to its search algorithm.

### 2. Endgame Knowledge
### 3. Bitboard Use
### 4. Dynamic Search
### 5. Positional Awareness
### 6. Adaptability
### 7. Memory Efficiency
### 8. Horizon Effect Mitigation
### 9. Self-Play Tuning
### 10. Time Wisdom
### 11. API Mastery
