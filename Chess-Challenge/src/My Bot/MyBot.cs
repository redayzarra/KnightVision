using ChessChallenge.API;
using System.Linq; 
using System.Collections.Generic;

public class MyBot : IChessBot
{
    // Opening book using a dictionary - decide on better lines to use
    private readonly Dictionary<string, string> openingBook = new()
    {
        {"e2e4", "e7e5"},
        {"e2e4e7e5", "g1f3"},
        {"e2e4e7e5g1f3", "g8f6"},
    };

    public Move Think(Board board, Timer timer)
    {   
        // Game's move history => string format
        Move[] gameHistory = board.GameMoveHistory;
        string moveHistoryString = string.Join("", gameHistory.Select(m => m.StartSquare.Name + m.TargetSquare.Name));
        
        // Is the current move in the opening book?
        if (openingBook.TryGetValue(moveHistoryString, out string? nextMove))
        {
            return new Move(nextMove!, board); // String => Move
        }

        // Are we in endgame? I need a better check for this.
        if (board.PlyCount > 40)
        {
            // 1. Prioritize pawn promotion - we can add more strategies later
            var promotionMoves = board.GetLegalMoves().Where(move => move.IsPromotion);
            if (promotionMoves.Any())
                return promotionMoves.First();
        }

        Move[] moves = board.GetLegalMoves();
        return moves[0];
    }
}
