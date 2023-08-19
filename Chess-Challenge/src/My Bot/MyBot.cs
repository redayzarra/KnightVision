using System;
using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    // Piece values: pawn, knight, bishop, rook, queen, king
    private readonly int[] pieceValues = { 100, 300, 325, 500, 900, 10000 };
    private readonly List<Move>[] killerMoves = new List<Move>[10]; // 10 is the Max Depth

    public MyBot()
    {
        for (int i = 0; i < 10; i++)
            killerMoves[i] = new List<Move>();
    }

    public Move Think(Board board, Timer timer)
    {
        int bestScore = int.MinValue;
        Move bestMove = board.GetLegalMoves()[0];

        foreach (var move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            int score = Minimax(board, 5 - 1, false, int.MinValue, int.MaxValue); // 5 is the depth, I can adjust this based on the bot's performance
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
            board.UndoMove(move);
        }

        return bestMove;
    }

    private int Minimax(Board board, int depth, bool isMaximizing, int alpha, int beta)
    {
        if (depth == 0)
            return Evaluate(board);

        if (isMaximizing)
        {
            int maxEval = int.MinValue;
            foreach (var move in OrderMoves(board, board.GetLegalMoves().ToList(), depth))
            {
                board.MakeMove(move);
                int eval = Minimax(board, depth - 1, false, alpha, beta);
                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);
                if (beta <= alpha)
                {
                    board.UndoMove(move);
                    UpdateKillerMoves(depth, move);
                    break; // Beta cut-off
                }
                board.UndoMove(move);
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (var move in OrderMoves(board, board.GetLegalMoves().ToList(), depth))
            {
                board.MakeMove(move);
                int eval = Minimax(board, depth - 1, true, alpha, beta);
                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);
                if (beta <= alpha)
                {
                    board.UndoMove(move);
                    UpdateKillerMoves(depth, move);
                    break; // Alpha cut-off
                }
                board.UndoMove(move);
            }
            return minEval;
        }
    }

    private int Evaluate(Board board)
    {
        int score = 0;

        // Material Advantage
        PieceList[] pieceLists = board.GetAllPieceLists();
        for (int i = 0; i < 12; i++)
        {
            int pieceValue = pieceValues[(int)pieceLists[i].TypeOfPieceInList - 1];
            if (pieceLists[i].IsWhitePieceList)
                score -= pieceValue * pieceLists[i].Count;
            else
                score += pieceValue * pieceLists[i].Count;
        }

        // King Safety
        if (board.IsInCheck())
        {
            if (board.IsWhiteToMove)
                score += 50; // Black has an advantage
            else
                score -= 50; // White has an advantage
        }

        // Checkmate
        if (board.IsInCheckmate())
        {
            if (board.IsWhiteToMove)
                score += 10000; // Black wins
            else
                score -= 10000; // White wins
        }

        // Pawn Structure - Penalty for Doubled and Isolated Pawns
        ulong whitePawns = board.GetPieceBitboard(PieceType.Pawn, true);
        ulong blackPawns = board.GetPieceBitboard(PieceType.Pawn, false);
        for (int file = 0; file < 8; file++)
        {
            ulong fileMask = 0x0101010101010101UL << file;
            int whitePawnsInFile = BitboardHelper.GetNumberOfSetBits(whitePawns & fileMask);
            int blackPawnsInFile = BitboardHelper.GetNumberOfSetBits(blackPawns & fileMask);
                
            // Penalize Doubled pawns
            if (whitePawnsInFile > 1) score += 10 * (whitePawnsInFile - 1);
            if (blackPawnsInFile > 1) score -= 10 * (blackPawnsInFile - 1);
                
            // Penalize Isolated pawns
            ulong adjFilesMask = fileMask;
            if (file > 0) adjFilesMask |= fileMask >> 1;
            if (file < 7) adjFilesMask |= fileMask << 1;
            if ((whitePawns & adjFilesMask) == 0) score += 20;
            if ((blackPawns & adjFilesMask) == 0) score -= 20;
        }

        // Pawn Structure - Reward Passed pawns
        ulong passedWhitePawns = whitePawns & ~((blackPawns >> 8) | (blackPawns >> 7) | (blackPawns >> 9));
        ulong passedBlackPawns = blackPawns & ~((whitePawns << 8) | (whitePawns << 7) | (whitePawns << 9));
        score += 30 * BitboardHelper.GetNumberOfSetBits(passedWhitePawns);
        score -= 30 * BitboardHelper.GetNumberOfSetBits(passedBlackPawns);

        // Pawn Structure - Reward Supported pawns
        ulong supportedWhitePawns = whitePawns & ((whitePawns << 7) | (whitePawns << 9) | (whitePawns << 8));
        ulong supportedBlackPawns = blackPawns & ((blackPawns >> 7) | (blackPawns >> 9) | (blackPawns >> 8));
        score += 15 * BitboardHelper.GetNumberOfSetBits(supportedWhitePawns);
        score -= 15 * BitboardHelper.GetNumberOfSetBits(supportedBlackPawns);

        // Piece Mobility
        Move[] whiteMoves = board.GetLegalMoves(true);
        Move[] blackMoves = board.GetLegalMoves(false);
        score += (blackMoves.Length - whiteMoves.Length) * 10;

        // Reward for captures
        foreach (var move in whiteMoves)
        {
            if (move.IsCapture)
            {
                int captureScore = pieceValues[(int)move.CapturePieceType - 1] - pieceValues[(int)move.MovePieceType - 1];
                score -= captureScore;
            }
        }
        foreach (var move in blackMoves)
        {
            if (move.IsCapture)
            {
                int captureScore = pieceValues[(int)move.CapturePieceType - 1] - pieceValues[(int)move.MovePieceType - 1];
                score += captureScore;
            }
        }

        // Center Control
        ulong centerSquares = 0x0000001818000000UL;
        score += BitboardHelper.GetNumberOfSetBits(board.BlackPiecesBitboard & centerSquares) * 20;
        score -= BitboardHelper.GetNumberOfSetBits(board.WhitePiecesBitboard & centerSquares) * 20;

        return score;
    }

    private void UpdateKillerMoves(int depth, Move move)
    {
        if (!killerMoves[depth].Contains(move))
            killerMoves[depth].Add(move);
    }

    private List<Move> OrderMoves(Board board, List<Move> moves, int depth)
    {
        List<Move> orderedMoves = new List<Move>();

        // Add killer moves
        foreach (var killerMove in killerMoves[depth])
        {
            if (moves.Contains(killerMove))
                orderedMoves.Add(killerMove);
        }

        // Sort captures based on MVV-LVA
        orderedMoves.AddRange(moves.Where(m => m.IsCapture)
            .OrderByDescending(m => pieceValues[(int)m.CapturePieceType - 1] - pieceValues[(int)m.MovePieceType - 1]));

        // Add other moves
        orderedMoves.AddRange(moves.Except(orderedMoves));

        return orderedMoves;
    }
}
