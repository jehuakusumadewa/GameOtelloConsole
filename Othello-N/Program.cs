namespace OthelloGame;

class Program
{
    // ========== UI METHODS ==========
    
    private static void PrintBoard(OthelloGame game)
    {
        var board = game.GetBoard();
        int size = game.GetBoardSize();
        
        Console.Write("  ");
        for (int j = 0; j < size; j++)
        {
            Console.Write($"{j + 1} ");
        }
        Console.WriteLine();
        
        for (int i = 0; i < size; i++)
        {
            Console.Write($"{i + 1} ");
            for (int j = 0; j < size; j++)
            {
                var disk = board.Squares[i, j].Disk;
                char displayChar = disk == null ? '.' : 
                                 disk.Color == DiskColor.Black ? 'B' : 'W';
                Console.Write($"{displayChar} ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }

    private static void PrintScores(OthelloGame game)
    {
        var players = game.GetPlayers();
        foreach (var player in players)
        {
            int score = game.GetPlayerScore(player);
            Console.WriteLine($"{player.Name} ({player.Color}): {score} disk");
        }
    }

    private static void PrintValidMoves(OthelloGame game)
    {
        var validMoves = game.GetValidMoves();
        if (validMoves.Count > 0)
        {
            Console.WriteLine("Langkah yang tersedia: ");
            foreach (var move in validMoves)
            {
                Console.WriteLine($"  ({move.X + 1}, {move.Y + 1})");
            }
        }
        else
        {
            Console.WriteLine("⚠️  Tidak ada langkah valid - harus skip turn");
        }
    }

    private static void PrintCurrentTurnInfo(OthelloGame game)
    {
        var currentPlayer = game.GetCurrentPlayer();
        
        Console.WriteLine($"\n=== GILIRAN {currentPlayer.Name} ({currentPlayer.Color}) ===");
        
        PrintBoard(game);
        PrintScores(game);
        PrintValidMoves(game);
        
        Console.WriteLine();
    }

    private static void PrintGameResult(OthelloGame game)
    {
        Console.WriteLine("\n🎉 === HASIL AKHIR === 🎉");
        
        PrintBoard(game);
        
        var winner = game.GetWinner();
        if (winner != null)
        {
            Console.WriteLine($"🏆 PEMENANG: {winner.Name} ({winner.Color})");
        }
        else
        {
            Console.WriteLine("🤝 PERMAINAN SERI!");
        }
        
        PrintScores(game);
    }

    // ========== MAIN PROGRAM ==========
    
    static void Main(string[] args)
    {
        Console.WriteLine("🎯 Welcome to Othello Game!");
        Console.WriteLine("===========================");

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
        
        Console.WriteLine("\n🎮 Cara Main:");
        Console.WriteLine("  - Masukkan langkah sebagai 'baris kolom' (contoh: '3 4')");
        Console.WriteLine("  - Ketik 'skip' untuk skip turn (jika tidak ada langkah valid)");
        Console.WriteLine("  - Ketik 'quit' untuk keluar dari game");
        Console.WriteLine("  - Ketik 'help' untuk menampilkan info ini lagi");
        Console.WriteLine();
        
        // Start game
        game.StartGame();
        
        // Main game loop
        while (game.IsGameActive())
        {
            PrintCurrentTurnInfo(game);
            
            if (game.CurrentPlayerHasValidMoves())
            {
                bool validMove = false;
                while (!validMove && game.IsGameActive())
                {
                    Console.Write($"Masukkan langkah {game.GetCurrentPlayer().Name}: ");
                    string input = Console.ReadLine()?.Trim() ?? "";
                    
                    // Handle special commands
                    switch (input.ToLower())
                    {
                        case "quit":
                            Console.WriteLine("Game dihentikan oleh user.");
                            return;
                            
                        case "help":
                            Console.WriteLine("\n🎮 Cara Main:");
                            Console.WriteLine("  - Masukkan langkah sebagai 'baris kolom' (contoh: '3 4')");
                            Console.WriteLine("  - Ketik 'skip' untuk skip turn");
                            Console.WriteLine("  - Ketik 'quit' untuk keluar dari game");
                            Console.WriteLine("  - Ketik 'help' untuk menampilkan info ini");
                            Console.WriteLine();
                            continue;
                            
                        case "skip":
                            Console.WriteLine("Tidak bisa skip saat ada langkah valid!");
                            continue;
                    }
                    
                    if (game.ProcessMove(input))
                    {
                        validMove = true;
                        Console.WriteLine("✅ Langkah berhasil!");
                    }
                    // Error message sudah dihandle oleh ProcessMove
                }
            }
            else
            {
                Console.WriteLine("⏭️  Tidak ada langkah valid. Tekan Enter untuk skip turn...");
                Console.ReadLine();
                game.SkipTurn();
            }
        }
        
        // Game finished - tampilkan hasil akhir
        PrintGameResult(game);
        
        Console.WriteLine("\n🙏 Terima kasih telah bermain Othello!");
        Console.WriteLine("Tekan Enter untuk keluar...");
        Console.ReadLine();
    }
}