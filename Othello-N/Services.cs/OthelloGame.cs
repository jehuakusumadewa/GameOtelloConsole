namespace OthelloGame;

public class OthelloGame
{
    private IBoard _board;
    private List<IPlayer> _players;
    private IPlayer _currentPlayer;
    private StatusType _status;
    private int _size;

    private readonly List<Position> _directions;

    // Delegates untuk event messages
    public Action<IPlayer> OnTurnChanged { get; set; }
    public Action<string> OnMessage { get; set; }

    public OthelloGame(List<IPlayer> players, IBoard board, int size, StatusType status, IPlayer currentPlayer, List<Position> directions)
    {
        _board = board;
        _size = size;
        _status = status;
        _currentPlayer = currentPlayer;
        _players = players;
        _directions = directions;

        // Default actions
        OnTurnChanged = (player) => Console.WriteLine($"🎮 Giliran {player.Name} ({player.Color})");
        OnMessage = (msg) => Console.WriteLine($"💡 {msg}");
    }

    private void InitializeBoard()
    {
        _board.Squares = new ICell[_size, _size];
        
        for (int i = 0; i < _size; i++)
        {
            for (int j = 0; j < _size; j++)
            {
                _board.Squares[i, j] = new Cell
                {
                    Position = new Position(i, j)
                };
            }
        }
    }

    public void StartGame()
    {
        _status = StatusType.Play;
        InitializeBoard();
        InitializeBoardDisks();
        
        Console.WriteLine("🎯 Othello Game Started!");
        
        // Panggil delegate untuk turn pertama
        OnTurnChanged?.Invoke(_currentPlayer);
    }

    public bool IsGameActive()
    {
        return _status == StatusType.Play;
    }

    public bool CurrentPlayerHasValidMoves()
    {
        return HasValidMove(_currentPlayer);
    }

    // SATU method untuk handle semua: parsing + validation + execution
    public bool ProcessMove(string input)
    {
        // Step 1: Parse dan validasi format input
        if (string.IsNullOrWhiteSpace(input)) 
        {
            OnMessage?.Invoke("Input tidak boleh kosong");
            return false;
        }
        
        string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) 
        {
            OnMessage?.Invoke("Format input salah. Gunakan: 'baris kolom' (contoh: '3 4')");
            return false;
        }
        
        if (!int.TryParse(parts[0], out int row) || !int.TryParse(parts[1], out int col))
        {
            OnMessage?.Invoke("Input harus berupa angka");
            return false;
        }

        // Convert to zero-based indices
        row--;
        col--;

        // Step 2: Validasi range
        if (row < 0 || row >= _size || col < 0 || col >= _size)
        {
            OnMessage?.Invoke($"Input diluar range. Gunakan angka 1 sampai {_size}");
            return false;
        }

        // Step 3: Validasi game logic
        Position move = new Position(row, col);
        if (!IsValidMove(move, _currentPlayer))
        {
            OnMessage?.Invoke("Langkah tidak valid. Pilih dari langkah yang tersedia.");
            return false;
        }

        // Step 4: Execute move
        MakeMove(move);
        return true;
    }

    public void SkipTurn()
    {
        Console.WriteLine($"{_currentPlayer.Name} tidak ada langkah valid. Skip turn.");
        
        // Switch player - akan memanggil OnTurnChanged
        SwitchPlayer();
        
        // Cek jika game harus berakhir
        if (!HasValidMove(_currentPlayer))
        {
            Console.WriteLine("Kedua pemain tidak ada langkah valid. Game berakhir!");
            FinishGame();
        }
    }

    // Method MakeMove (private)
    private void MakeMove(Position position)
    {
        Console.WriteLine($"{_currentPlayer.Name} meletakkan disk di ({position.X + 1}, {position.Y + 1})");

        // Place the Disk
        var newDisk = new OthelloDisk { Color = _currentPlayer.Color };
        PlaceDisk(position, newDisk);

        // Flip opponent's Disks
        FlipDisks(position, _currentPlayer.Color);

        // Switch player - akan otomatis panggil OnTurnChanged
        SwitchPlayer();
    }

    // Method SwitchPlayer dengan pesan
    private void SwitchPlayer()
    {
        _currentPlayer = (_currentPlayer == _players[0]) ? _players[1] : _players[0];
        
        // Panggil delegate setiap kali ganti player
        OnTurnChanged?.Invoke(_currentPlayer);
    }

    public void PrintCurrentTurnInfo()
    {
        PrintBoard();
        PrintScores();
        
        var validMoves = GetValidMoves();
        if (validMoves.Count > 0)
        {
            Console.WriteLine("Langkah yang tersedia: ");
            foreach (var move in validMoves)
            {
                Console.WriteLine($"({move.X + 1}, {move.Y + 1})");
            }
        }
        else
        {
            Console.WriteLine("Tidak ada langkah valid - harus skip turn");
        }
        Console.WriteLine();
    }

    // ========== METHOD-METHOD GAME LOGIC ==========

    private void InitializeBoardDisks()
    {
        PlaceDisk(new Position(3, 3), new OthelloDisk { Color = DiskColor.White });
        PlaceDisk(new Position(3, 4), new OthelloDisk { Color = DiskColor.Black });
        PlaceDisk(new Position(4, 3), new OthelloDisk { Color = DiskColor.Black });
        PlaceDisk(new Position(4, 4), new OthelloDisk { Color = DiskColor.White });
    }

    private void PlaceDisk(Position position, IDisk Disk)
    {
        Disk.Position = position;
        _board.Squares[position.X, position.Y].Disk = Disk;
    }

    private IPlayer GetPlayerByColor(DiskColor color)
    {
        return _players.Find(p => p.Color == color);
    }

    public IPlayer GetCurrentPlayer()
    {
        return _currentPlayer;
    }

    private List<Position> GetPossibleMovesForPlayer(IPlayer player)
    {
        var validMoves = new List<Position>();
        
        for (int i = 0; i < _size; i++)
        {
            for (int j = 0; j < _size; j++)
            {
                var position = new Position(i, j);
                if (IsValidMove(position, player))
                {
                    validMoves.Add(position);
                }
            }
        }
        
        return validMoves;
    }

    private bool IsValidMove(Position position, IPlayer player)
    {
        if (position.X < 0 || position.X >= _size || position.Y < 0 || position.Y >= _size || 
            _board.Squares[position.X, position.Y].Disk != null)
            return false;

        DiskColor opponentColor = (player.Color == DiskColor.Black) ? DiskColor.White : DiskColor.Black;

        foreach (var direction in _directions)
        {
            if (CheckDirection(position, direction, player.Color, opponentColor))
                return true;
        }
        
        return false;
    }

    private bool CheckDirection(Position position, Position direction, DiskColor playerColor, DiskColor opponentColor)
    {
        int x = position.X + direction.X;
        int y = position.Y + direction.Y;
        bool foundOpponent = false;

        while (x >= 0 && x < _size && y >= 0 && y < _size && 
               _board.Squares[x, y].Disk != null && 
               _board.Squares[x, y].Disk.Color == opponentColor)
        {
            foundOpponent = true;
            x += direction.X;
            y += direction.Y;
        }

        if (foundOpponent && x >= 0 && x < _size && y >= 0 && y < _size && 
            _board.Squares[x, y].Disk != null && 
            _board.Squares[x, y].Disk.Color == playerColor)
        {
            return true;
        }
        
        return false;
    }

    private bool HasValidMove(IPlayer player)
    {
        return GetPossibleMovesForPlayer(player).Count > 0;
    }

    private void FlipDisks(Position position, DiskColor playerColor)
    {
        DiskColor opponentColor = (playerColor == DiskColor.Black) ? DiskColor.White : DiskColor.Black;

        foreach (var direction in _directions)
        {
            FlipDirection(position, direction, playerColor, opponentColor);
        }
    }

    private void FlipDirection(Position position, Position direction, DiskColor playerColor, DiskColor opponentColor)
    {
        int x = position.X + direction.X;
        int y = position.Y + direction.Y;
        var DisksToFlip = new List<Position>();

        while (x >= 0 && x < _size && y >= 0 && y < _size && 
               _board.Squares[x, y].Disk != null && 
               _board.Squares[x, y].Disk.Color == opponentColor)
        {
            DisksToFlip.Add(new Position(x, y));
            x += direction.X;
            y += direction.Y;
        }

        if (x >= 0 && x < _size && y >= 0 && y < _size && 
            _board.Squares[x, y].Disk != null && 
            _board.Squares[x, y].Disk.Color == playerColor)
        {
            foreach (var flipPos in DisksToFlip)
            {
                _board.Squares[flipPos.X, flipPos.Y].Disk.Color = playerColor;
                Console.WriteLine($"Membalik disk di ({flipPos.X + 1}, {flipPos.Y + 1}) menjadi {playerColor}");
            }
        }
    }

    public bool IsGameOver()
    {
        return _status == StatusType.Win || _status == StatusType.Draw;
    }

    public IPlayer GetWinner()
    {
        if (_status != StatusType.Win) return null;

        int blackScore = GetPlayerScore(GetPlayerByColor(DiskColor.Black));
        int whiteScore = GetPlayerScore(GetPlayerByColor(DiskColor.White));

        if (blackScore > whiteScore)
            return GetPlayerByColor(DiskColor.Black);
        else if (whiteScore > blackScore)
            return GetPlayerByColor(DiskColor.White);
        else
            return null;
    }

    public int GetPlayerScore(IPlayer player)
    {
        int count = 0;
        for (int i = 0; i < _size; i++)
        {
            for (int j = 0; j < _size; j++)
            {
                var Disk = _board.Squares[i, j].Disk;
                if (Disk != null && Disk.Color == player.Color)
                {
                    count++;
                }
            }
        }
        return count;
    }

    public void FinishGame()
    {
        int blackScore = GetPlayerScore(GetPlayerByColor(DiskColor.Black));
        int whiteScore = GetPlayerScore(GetPlayerByColor(DiskColor.White));

        Console.WriteLine($"\n=== GAME OVER ===");
        Console.WriteLine($"Skor Akhir - Hitam: {blackScore}, Putih: {whiteScore}");

        if (blackScore > whiteScore)
        {
            _status = StatusType.Win;
            Console.WriteLine($"{GetPlayerByColor(DiskColor.Black).Name} (Hitam) menang!");
        }
        else if (whiteScore > blackScore)
        {
            _status = StatusType.Win;
            Console.WriteLine($"{GetPlayerByColor(DiskColor.White).Name} (Putih) menang!");
        }
        else
        {
            _status = StatusType.Draw;
            Console.WriteLine("Permainan seri!");
        }
    }

    public void PrintBoard()
    {
        Console.WriteLine("  1 2 3 4 5 6 7 8");
        for (int i = 0; i < _size; i++)
        {
            Console.Write($"{i + 1} ");
            for (int j = 0; j < _size; j++)
            {
                var Disk = _board.Squares[i, j].Disk;
                char displayChar = Disk == null ? '.' : 
                                 Disk.Color == DiskColor.Black ? 'B' : 'W';
                Console.Write($"{displayChar} ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }

    public List<Position> GetValidMoves()
    {
        return GetPossibleMovesForPlayer(_currentPlayer);
    }

    public void PrintScores()
    {
        int blackScore = GetPlayerScore(GetPlayerByColor(DiskColor.Black));
        int whiteScore = GetPlayerScore(GetPlayerByColor(DiskColor.White));
        Console.WriteLine($"Skor - Hitam: {blackScore}, Putih: {whiteScore}");
    }
}