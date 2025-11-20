namespace OthelloGame;
    public class OthelloBoard : IBoard
    {
        public ICell[,] Squares { get; set; }
    }