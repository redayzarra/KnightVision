using ChessChallenge.API;
using System;
using System.Linq; 
using System.Collections.Generic;

public class MyBot : IChessBot
{   
    // Opening book for getting strong positions
    private readonly Dictionary<string, string> openingBook = new()
    {
        {"e2e4", "e7e5"},
        {"e2e4e7e5", "g1f3"},
        {"e2e4e7e5g1f3", "b8c6"},
        {"e2e4e7e5g1f3b8c6", "f1b5"}, // Ruy-Lopez
        {"e2e4e7e5g1f3b8c6f1b5", "a7a6"}, // Continuation of Ruy-Lopez
        {"e2e4e7e5g1f3b8c6f1c4", "c7c5"}, // Italian Game
        {"e2e4e7e5g1f3b8c6d2d4", "e5d4"}, // Scotch Game

        {"d2d4d7d5", "c2c4"}, // Queen's Gambit
        {"d2d4d7d5c2c4", "d5c4"}, // Continuation of Queen's Gambit

        {"d2d4", "g8f6"},
        {"d2d4g8f6", "c2c4"}, 
        {"d2d4g8f6c2c4", "g7g6"}, // King's Indian Defence
        {"d2d4g8f6c2c4g7g6", "b1c3"},
        {"d2d4g8f6c2c4g7g6b1c3", "f8g7"},
        {"d2d4g8f6c2c4g7g6b1c3f8g7", "e2e4"}, // Continuation of King's Indian

        {"e2e4c7c5", "g1f3"}, // Sicilian Defense
        {"e2e4e7e6", "d2d4"}, // French Defense
        {"e2e4c7c6", "d2d4"}, // Caro-Kann
        {"e2e4d7d6", "d2d4"},
        {"e2e4d7d6d2d4", "g8f6"}  // Pirc Defense
    };

    // Piece values: null, pawn, knight, bishop, rook, queen, king
    readonly int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

    public Move Think(Board board, Timer timer)
    {
        // OPENING BOOK PHASE //
        Move[] gameHistory = board.GameMoveHistory;
        string moveHistoryString = string.Join("", gameHistory.Select(m => m.StartSquare.Name + m.TargetSquare.Name));
        
        // Are the current moves in the opening book?
        if (openingBook.TryGetValue(moveHistoryString, out string? nextMove))
        {
            return new Move(nextMove!, board);
        }
        

        // IF NOT OPENING BOOK, THIS LOGIC WILL TAKE PLACE:
        Move[] allMoves = board.GetLegalMoves();

        // Pick a random move to play if nothing better is found
        Random rng = new();
        Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
        int highestValueCapture = 0;

        foreach (Move move in allMoves)
        {
            // Always play checkmate in one
            if (MoveIsCheckmate(board, move))
            {
                moveToPlay = move;
                break;
            }

            // Find highest value capture
            Piece capturedPiece = board.GetPiece(move.TargetSquare);
            int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];

            if (capturedPieceValue > highestValueCapture)
            {
                moveToPlay = move;
                highestValueCapture = capturedPieceValue;
            }
        }

        return moveToPlay;
    }

    // Test if this move gives checkmate
    bool MoveIsCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }
}