namespace OthelloGame;

 class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Othello Game!");

            
            Console.Write("Enter Player 1 (Black) name: ");
            string player1Name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(player1Name)) player1Name = "Player 1";
            
            Console.Write("Enter Player 2 (White) name: ");
            string player2Name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(player2Name)) player2Name = "Player 2";
            
            var player1 = new Player { Name = player1Name, Color = DiskColor.Black };
            var player2 = new Player { Name = player2Name, Color = DiskColor.White };
            // new List<IPlayer> { player1, player2 };
            List<IPlayer> ListPlayer = new List<IPlayer>{player1, player2};
            var board = new OthelloBoard();
            var currentPlayer = player1;
            var status = StatusType.NotStart;
            var size = 8;
            var direction = new List<Position>
            {
                new Position(-1, -1), new Position(-1, 0), new Position(-1, 1),
                new Position(0, -1),                      new Position(0, 1),
                new Position(1, -1),  new Position(1, 0),  new Position(1, 1)
            };


            var game = new OthelloGame(ListPlayer, board, size, status, currentPlayer, direction);
            
            Console.WriteLine("\nStarting Othello Game!");
            Console.WriteLine("How to play: Enter your move as 'row column' (e.g., '3 4' for row 3, column 4)");
            Console.WriteLine();
            
            game.StartGame();
            
            // Print final board and scores
            Console.WriteLine("\nFinal Board:");
            game.PrintBoard();
        }
    }