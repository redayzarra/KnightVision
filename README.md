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

I can allow my bot to detect if it's in the endgame stage, where I could then use heuristics to gain an advantage. If the position is in the endgame, then I can modify the bot's evaluation function to prioritize central king placement, reward rook placement on open files, especially behind passed pawns, and I can adjust pawn values to prioritize their advancement in the endgame, etc. I can also check for specific endgame patterns and then steer the game based on that. 

#### Solution

Check if the position is in endgame (decide how to check for endgame) and switch the bot's evaluation function, play different strategies, endgame patterns, and more.

##### 1. Basic King and Pawn Endgames:
- King Opposition: Using the Square struct, determine the relative positions of both kings. If they are on the same rank, file, or diagonal with an odd number of squares between them, the side not to move has the opposition.
- Pawn Promotion: If a pawn is nearing the promotion square (rank 7 for white, rank 2 for black), prioritize its advancement using the Move class. Look for moves that push the pawn forward.
- Square of the Pawn: Calculate if the opposing king can catch the pawn before it promotes. If the king is outside the pawn's "square," prioritize pushing the pawn. If inside, focus on other strategies.

##### 2. Recognizing Simple Drawn Endgames:

- Bishop of Opposite Colors: If both sides only have a single bishop and they move on different colored squares (using the PieceList class to fetch the bishops), it's often a draw. You can steer the game towards solidifying that draw.
- Insufficient Material: Using the Board.IsInsufficientMaterial() function, detect positions where checkmate is impossible (like king vs. king, king and bishop vs. king, etc.).

##### 3. Piece-Square Tables for Endgames:

- Modify the bot's evaluation function to prioritize central king placement in the endgame.
- Reward rook placement on open files, especially behind passed pawns.
- Adjust pawn values to prioritize their advancement in the endgame.

##### 4. Complex Endgame Patterns:

- Philidor and Lucena Positions: Recognize these crucial rook endgame patterns. If the PieceList detects rooks and kings as the primary pieces, check for these structures and execute the winning techniques.
- Bishop and Wrong Rook Pawn: If there's a lone pawn, king, and bishop against a king, and the pawn is a rook pawn (a or h file), determine if it's the wrong colored promotion square for the bishop. If so, aim for a drawing technique.

##### 5. Using Bitboards for Endgame Analysis:

- Pawn Structures: Use the BitboardHelper class to quickly analyze pawn structures. For example, detect passed pawns, isolated pawns, or doubled pawns.
- King Activity: Use BitboardHelper.GetKingAttacks(Square square) to evaluate the activity and safety of the kings.

### 3. Bitboard Use
### 4. Dynamic Search
### 5. Positional Awareness
### 6. Adaptability
### 7. Memory Efficiency
### 8. Horizon Effect Mitigation
### 9. Self-Play Tuning
### 10. Time Wisdom
### 11. API Mastery
