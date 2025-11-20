namespace OthelloGame;
    public interface ICell
    {
        Position Position { get; set; }
        IDisk Disk { get; set; }
    }