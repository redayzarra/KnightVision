using System;
using ChessChallenge.API;
using static ChessChallenge.API.BitboardHelper;
using static System.Math;

public class MyBot : IChessBot
{
    // Variables to control search parameters and track the last score
    public int maxSearchTime, searchingDepth, lastScore;

    Timer timer;
    Board board;

    // Stores the best move found during the search
    Move searchBestMove, rootBestMove;

    // Transposition table: caching board states to avoid recalculations
    readonly (
        ulong hash,
        ushort moveRaw,
        int score,
        int depth,
        int bound
    )[] transpositionTable = new (ulong, ushort, int, int, int)[0x800000];

    // Move history for improving move ordering
    readonly int[,,] history = new int[2, 7, 64];

    // Precomputed evaluation weights for pieces
    readonly ulong[] packedData = {
        0x0000000000000000, 0x2328170f2d2a1401, 0x1f1f221929211507, 0x18202a1c2d261507,
        0x252e3022373a230f, 0x585b47456d65321c, 0x8d986f66a5a85f50, 0x0002000300070005,
        0xfffdfffd00060001, 0x2b1f011d20162306, 0x221c0b171f15220d, 0x1b1b131b271c1507,
        0x232d212439321f0b, 0x5b623342826c2812, 0x8db65b45c8c01014, 0x0000000000000000,
        0x615a413e423a382e, 0x6f684f506059413c, 0x82776159705a5543, 0x8b8968657a6a6150,
        0x948c7479826c6361, 0x7e81988f73648160, 0x766f7a7e70585c4e, 0x6c7956116e100000,
        0x3a3d2d2840362f31, 0x3c372a343b3a3838, 0x403e2e343c433934, 0x373e3b2e423b2f37,
        0x383b433c45433634, 0x353d4b4943494b41, 0x46432e354640342b, 0x55560000504f0511,
        0x878f635c8f915856, 0x8a8b5959898e5345, 0x8f9054518f8e514c, 0x96985a539a974a4c,
        0x9a9c67659e9d5f59, 0x989c807a9b9c7a6a, 0xa09f898ba59c6f73, 0xa1a18386a09b7e84,
        0xbcac7774b8c9736a, 0xbab17b7caebd7976, 0xc9ce7376cac57878, 0xe4de6f70dcd87577,
        0xf4ef7175eedc7582, 0xf9fa8383dfe3908e, 0xfffe7a81f4ec707f, 0xdfe79b94e1ee836c,
        0x2027252418003d38, 0x4c42091d31193035, 0x5e560001422c180a, 0x6e6200004d320200,
        0x756c000e5f3c1001, 0x6f6c333f663e3f1d, 0x535b55395c293c1b, 0x2f1e3d5e22005300,
        0x004c0037004b001f, 0x00e000ca00be00ad, 0x02e30266018800eb, 0xffdcffeeffddfff3,
        0xfff9000700010007, 0xffe90003ffeefff4, 0x00000000fff5000d,
    };

    // Returns the evaluation weight for a given item
    int EvalWeight(int item) => (int)(packedData[item >> 1] >> item * 32);

    // Gets the bot's next move using iterative deepening
    public Move Think(Board boardOrig, Timer timerOrig)
    {
        board = boardOrig;
        timer = timerOrig;

        // Set the maximum search time we can allocate per move
        maxSearchTime = timer.MillisecondsRemaining / 4;

        // Initialize search depth
        searchingDepth = 1;

        // Iterative deepening to find the best move
        do
        {
            try
            {
                // Aspiration window search
                if (Abs(lastScore - Negamax(lastScore - 20, lastScore + 20, searchingDepth)) >= 20)
                {
                    Negamax(-32000, 32000, searchingDepth);
                }
                rootBestMove = searchBestMove;
            }
            catch
            {
                break;
            }
        } while (++searchingDepth <= 200 && timer.MillisecondsElapsedThisTurn < maxSearchTime / 10);

        return rootBestMove;
    }

    // Negamax algorithm with alpha-beta pruning
    public int Negamax(int alpha, int beta, int depth)
    {
        // Abort search if time exceeds limit
        if (timer.MillisecondsElapsedThisTurn >= maxSearchTime && searchingDepth > 1)
            throw new Exception();

        // Lookup the transposition table entry for the current board position
        ref var transpoTable = ref transpositionTable[board.ZobristKey & 0x7FFFFF];
        var (ttHash, ttMoveRaw, score, ttDepth, ttBound) = transpoTable;

        bool ttHit = ttHash == board.ZobristKey; // Cache hit check
        bool nonPv = alpha + 1 == beta; // Non-principal variation check
        bool inQSearch = depth <= 0; // Quiescence search check

        // Initial evaluation setup
        int eval = 0x000b000a; 
        int bestScore = board.PlyCount - 30000;
        int oldAlpha = alpha;

        // Track move counts and quiet moves
        int moveCount = 0;
        int quietsToCheck = 0b_010111_001010_000101_000100_000000 >> depth * 6 & 0b111111;
        
        int tmp = 0;
        if (ttHit) // If we have a cache hit...
        {  
            // Use cached score if conditions are met
            if (ttDepth >= depth && ttBound switch
            {
                2147483647 => score >= beta,
                0 => score <= alpha,
                _ => nonPv || inQSearch,
            })
            {
                return score;
            }
        }
        else if (depth > 3)
            depth--; // Decrement depth if it is too large

        // Evaluates the current board position
        int EvaluatePosition(ulong pieces)
        {
            // Sum contributions of each piece to the overall score
            while (pieces != 0)
            {
                int sqIndex = ClearAndGetIndexOfLSB(ref pieces);
                Piece piece = board.GetPiece(new(sqIndex));
                int pieceType = (int)piece.PieceType;
                bool pieceIsWhite = piece.IsWhite;

                pieceType -= (sqIndex & 0b111 ^ board.GetKingSquare(pieceIsWhite).File) >> 1 >> pieceType;
                sqIndex = EvalWeight(112 + pieceType)
                    + (int)(packedData[pieceType * 64 + sqIndex >> 3 ^ (pieceIsWhite ? 0 : 0b111)]
                        >> (0x01455410 >> sqIndex * 4) * 8
                        & 0xFF00FF)
                    + EvalWeight(11 + pieceType) * GetNumberOfSetBits(
                        GetSliderAttacks((PieceType)Min(5, pieceType), new(sqIndex), board)
                    )
                    + EvalWeight(118 + pieceType) * GetNumberOfSetBits(
                        (pieceIsWhite ? 0x0101010101010100UL << sqIndex : 0x0080808080808080UL >> 63 - sqIndex)
                            & board.GetPieceBitboard(PieceType.Pawn, pieceIsWhite)
                    );

                // Adjust evaluation based on player's turn
                eval += pieceIsWhite == board.IsWhiteToMove ? sqIndex : -sqIndex;
                tmp += 0x0421100 >> pieceType * 4 & 0xF;
            }

            return (short)eval * tmp + eval / 0x10000 * (24 - tmp);
        }

        // Use cached score if available and not in quiescence
        eval = ttHit && !inQSearch ? score : EvaluatePosition(board.AllPiecesBitboard) / 24;

        // Alpha-beta pruning in quiescence search
        if (inQSearch)
        {
            alpha = Max(alpha, bestScore = eval);
        }
        else if (nonPv && eval >= beta && board.TrySkipTurn())
        {
            // Null move pruning: Skip a turn to prune moves
            bestScore = depth <= 4
                ? eval - 58 * depth
                : -Negamax(-beta, -alpha, (depth * 100 + beta - eval) / 186 - 1);
            board.UndoSkipTurn();
        }

        // Beta cutoff
        if (bestScore >= beta)
        {
            return bestScore;
        }

        // Stalemate detection
        if (board.IsInStalemate())
        {
            return 0;
        }

        // Generate all legal moves
        var moves = board.GetLegalMoves(inQSearch);
        var scores = new int[moves.Length];

        // Score moves for better ordering
        for (int i = 0; i < moves.Length; i++)
        {
            scores[i] -= ttHit && moves[i].RawValue == ttMoveRaw ? 1000000
                : Max(
                    (int)moves[i].CapturePieceType * 32768 - (int)moves[i].MovePieceType - 16384,
                    HistoryValue(moves[i])
                );
        }

        // Sort moves by score
        Array.Sort(scores, moves);
        Move bestMove = default;

        foreach (Move move in moves)
        {
            // Delta pruning in quiescence search
            if (inQSearch && eval + (0b1_0100110100_1011001110_0110111010_0110000110_0010110100_0000000000 >> (int)move.CapturePieceType * 10 & 0b1_11111_11111) <= alpha)
            {
                break;
            }

            board.MakeMove(move);
            int nextDepth = board.IsInCheck() ? depth : depth - 1;
            int reduction = (depth - nextDepth) * Max((moveCount * 93 + depth * 144) / 1000 + scores[moveCount] / 172, 0);

            if (board.IsRepeatedPosition())
            {
                score = 0;
            }
            else
            {
                // Null window search for PV
                while (moveCount != 0 && (score = -Negamax(~alpha, -alpha, nextDepth - reduction)) > alpha && reduction != 0)
                {
                    reduction = 0;
                }
                if (moveCount == 0 || score > alpha)
                {
                    score = -Negamax(-beta, -alpha, nextDepth);
                }
            }

            board.UndoMove(move);

            if (score > bestScore)
            {
                alpha = Max(alpha, bestScore = score);
                bestMove = move;
            }
            if (score >= beta)
            {
                if (!move.IsCapture)
                {
                    // Update history table for quiet moves
                    tmp = eval - alpha >> 31 ^ depth;
                    tmp *= tmp;
                    foreach (Move malusMove in moves.AsSpan(0, moveCount))
                    {
                        if (!malusMove.IsCapture)
                        {
                            HistoryValue(malusMove) -= tmp + tmp * HistoryValue(malusMove) / 512;
                        }
                    }
                    HistoryValue(move) += tmp - tmp * HistoryValue(move) / 512;
                }
                break;
            }

            // Prune moves based on various conditions
            if (nonPv && depth <= 4 && !move.IsCapture && (quietsToCheck-- == 1 || eval + 127 * depth < alpha))
            {
                break;
            }

            moveCount++;
        }

        // Update the transposition table with the new best move and score
        transpoTable = (
            board.ZobristKey,
            alpha > oldAlpha ? bestMove.RawValue : ttMoveRaw,
            Clamp(bestScore, -20000, 20000),
            Max(depth, 0),
            bestScore >= beta ? 2147483647 : alpha - oldAlpha
        );

        searchBestMove = bestMove;
        return lastScore = bestScore;
    }

    // Accesses and updates move history values
    ref int HistoryValue(Move move) => ref history[
        board.PlyCount & 1,
        (int)move.MovePieceType,
        move.TargetSquare.Index
    ];
}

