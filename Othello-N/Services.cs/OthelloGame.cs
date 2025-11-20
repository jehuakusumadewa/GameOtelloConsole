

namespace OthelloGame;

public class OthelloGame
    {
        private IBoard _board;
        private List<IPlayer> _players;
        private IPlayer _currentPlayer;
        private StatusType _status;
        private int _size;

        private readonly List<Position> _directions;

        public OthelloGame(List<IPlayer> players, IBoard board, int size, StatusType status, IPlayer currentPlayer, List<Position> directions)
        {
            _board = board;
            _size = size;
            _status = status;
            _currentPlayer = currentPlayer;
            _players = players;
            _status = status;
            _directions = directions;
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
            
            while (_status == StatusType.Play)
            {
                if (HasValidMove(_currentPlayer))
                {
                    Console.WriteLine($"Current Player: {_currentPlayer.Name} ({_currentPlayer.Color})");
                    PrintBoard();
                    PrintScores();
                    
                    var validMoves = GetValidMoves();
                    Console.WriteLine("Available moves: ");
                    foreach (var move in validMoves)
                    {
                        Console.WriteLine($"({move.X + 1}, {move.Y + 1})");
                    }
                    
                    bool validInput = false;
                    while (!validInput)
                    {
                        Console.Write("Enter your move (row column, e.g., '3 4'): ");
                        string input = Console.ReadLine();
                        
                        if (string.IsNullOrWhiteSpace(input))
                        {
                            Console.WriteLine("Invalid input. Please try again.");
                            continue;
                        }
                        
                        string[] parts = input.Split(' ');
                        if (parts.Length != 2)
                        {
                            Console.WriteLine("Please enter two numbers separated by space.");
                            continue;
                        }
                        
                        if (int.TryParse(parts[0], out int row) && int.TryParse(parts[1], out int col))
                        {
                            // Convert to zero-based indices
                            row--;
                            col--;
                            
                            Position move = new Position(row, col);
                            
                            if (IsValidMove(move, _currentPlayer))
                            {
                                MakeMove(move);
                                validInput = true;
                            }
                            else
                            {
                                Console.WriteLine("Invalid move. Please choose from the available moves.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Please enter valid numbers.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"{_currentPlayer.Name} has no valid moves. Switching player.");
                    SwitchPlayer();
                    if (!HasValidMove(_currentPlayer))
                    {
                        Console.WriteLine("No valid moves for both players. Game over!");
                        FinishGame();
                        return;
                    }
                }
            }
        }

        private void InitializeBoardDisks()
        {
            // Set initial Disks for Othello
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
            // Check if position is within bounds and empty
            if (position.X < 0 || position.X >= _size || position.Y < 0 || position.Y >= _size || 
                _board.Squares[position.X, position.Y].Disk != null)
                return false;

            DiskColor opponentColor = (player.Color == DiskColor.Black) ? DiskColor.White : DiskColor.Black;

            // Check all directions
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

        public void MakeMove(Position position)
        {
            if (!IsValidMove(position, _currentPlayer))
                throw new InvalidOperationException("Invalid move");

            Console.WriteLine($"{_currentPlayer.Name} places Disk at ({position.X + 1}, {position.Y + 1})");

            // Place the Disk
            var newDisk = new OthelloDisk { Color = _currentPlayer.Color };
            PlaceDisk(position, newDisk);

            // Flip opponent's Disks
            FlipDisks(position, _currentPlayer.Color);

            SwitchPlayer();
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
                    Console.WriteLine($"Flipped Disk at ({flipPos.X + 1}, {flipPos.Y + 1}) to {playerColor}");
                }
            }
        }

        private void SwitchPlayer()
        {
            _currentPlayer = (_currentPlayer == _players[0]) ? _players[1] : _players[0];
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

            Console.WriteLine($"Final Score - Black: {blackScore}, White: {whiteScore}");

            if (blackScore > whiteScore)
            {
                _status = StatusType.Win;
                Console.WriteLine($"{GetPlayerByColor(DiskColor.Black).Name} (Black) wins!");
            }
            else if (whiteScore > blackScore)
            {
                _status = StatusType.Win;
                Console.WriteLine($"{GetPlayerByColor(DiskColor.White).Name} (White) wins!");
            }
            else
            {
                _status = StatusType.Draw;
                Console.WriteLine("The game is a draw!");
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
            Console.WriteLine($"Current Scores - Black: {blackScore}, White: {whiteScore}");
        }
    }