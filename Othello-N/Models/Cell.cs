namespace OthelloGame;
    public class Cell : ICell
    {
        public Position Position { get; set; }
        public IDisk Disk { get; set; }
    }