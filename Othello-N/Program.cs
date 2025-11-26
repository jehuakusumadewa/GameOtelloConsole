namespace OthelloGame;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to Othello Game!");

        // Setup players
        Console.Write("Enter Player 1 (Black) name: ");
        string player1Name = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(player1Name)) player1Name = "Player 1";
        
        Console.Write("Enter Player 2 (White) name: ");
        string player2Name = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(player2Name)) player2Name = "Player 2";
        
        var player1 = new Player { Name = player1Name, Color = DiskColor.Black };
        var player2 = new Player { Name = player2Name, Color = DiskColor.White };
        
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
        Console.WriteLine("Cara main: Masukkan langkah sebagai 'baris kolom' (contoh: '3 4')");
        Console.WriteLine("Ketik 'quit' untuk keluar dari game");
        Console.WriteLine();
        
        // Start game
        game.StartGame();
        
        // Main game loop - SANGAT SEDERHANA!
        while (game.IsGameActive())
        {
            game.PrintCurrentTurnInfo();
            
            if (game.CurrentPlayerHasValidMoves())
            {
                bool validMove = false;
                while (!validMove && game.IsGameActive())
                {
                    Console.Write("Masukkan langkah (baris kolom): ");
                    string input = Console.ReadLine() ?? "";
                    
                    // Cek jika user mau quit
                    if (input.Trim().ToLower() == "quit")
                    {
                        Console.WriteLine("Game dihentikan oleh user.");
                        return;
                    }
                    
                    
                    if (game.ProcessMove(input))
                    {
                        validMove = true;
                        Console.WriteLine("Langkah berhasil!");
                    }
                    // Semua error message sudah dihandle oleh ProcessMove
                }
            }
            else
            {
                Console.WriteLine("Tekan tombol apa saja untuk skip turn...");
                Console.ReadKey();
                game.SkipTurn();
            }
        }
        
        // Game finished - tampilkan hasil akhir
        Console.WriteLine("\n=== HASIL AKHIR ===");
        game.PrintBoard();
        
        var winner = game.GetWinner();
        if (winner != null)
        {
            Console.WriteLine($"Pemenang: {winner.Name} ({winner.Color})");
        }
        else
        {
            Console.WriteLine("Permainan seri!");
        }
        
        Console.WriteLine("\nTerima kasih telah bermain Othello!");
    }
}