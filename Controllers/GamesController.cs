using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConwaysGameOfLifeApi.Data;
using ConwaysGameOfLifeApi.Models;
using ConwaysGameOfLifeApi.Models.DTOs.Requests;
using ConwaysGameOfLifeApi.Models.DTOs.Responses;

namespace ConwaysGameOfLifeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly GameOfLifeContext _context;
        private const int MAX_BOARD_DIMENSION = 100;

        public GamesController(GameOfLifeContext context)
        {
            _context = context;
        }

        // GET: api/Games
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GameResponse>>> GetGames()
        {
            if (_context.Games == null)
            {
                return NotFound();
            }

            var games = await _context.Games
                .Select(g => new GameResponse
                {
                    Id = g.Id,
                    BoardIds = g.Boards.Select(b => b.Id).ToList()
                })
                .ToListAsync();

            return games;
        }

        // GET: api/Games/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GameResponse>> GetGame(int id)
        {
            if (_context.Games == null)
            {
                return NotFound();
            }

            var game = await _context.Games
                .Where(g => g.Id == id)
                .Select(g => new
                {
                    g.Id,
                    BoardIds = g.Boards.Select(b => b.Id).ToList()
                })
                .FirstOrDefaultAsync();

            if (game == null)
            {
                return NotFound();
            }

            return new GameResponse 
            { 
                Id = game.Id,
                BoardIds = game.BoardIds
            };
        }

        // POST: api/Games
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Game>> PostGame([FromForm] CreateGameRequest request)
        {
            if (_context.Games == null)
            {
                return Problem("Entity set 'GameOfLifeContext.Games' is null.");
            }

            // Validate dimensions
            if (request.Width <= 0 || request.Height <= 0)
            {
                return BadRequest("Width and height must be greater than 0");
            }

            if (request.Width > MAX_BOARD_DIMENSION || request.Height > MAX_BOARD_DIMENSION)
            {
                return BadRequest($"Board dimensions cannot exceed {MAX_BOARD_DIMENSION}x{MAX_BOARD_DIMENSION}");
            }

            bool[,] gameBoard;

            try
            {
                if (request.CsvFile != null)
                {
                    using var reader = new StreamReader(request.CsvFile.OpenReadStream());
                    var csvData = await reader.ReadToEndAsync();
                    if (string.IsNullOrWhiteSpace(csvData))
                    {
                        return BadRequest("CSV file cannot be empty");
                    }
                    gameBoard = ParseCsvToBoard(csvData, request.Width, request.Height);
                }
                else if (request.BoardData != null)
                {
                    // Convert jagged array to 2D array
                    gameBoard = new bool[request.Height, request.Width];
                    
                    var sourceHeight = Math.Min(request.BoardData.Length, request.Height);
                    
                    for (int i = 0; i < sourceHeight; i++)
                    {
                        var sourceWidth = Math.Min(request.BoardData[i].Length, request.Width);
                        for (int j = 0; j < sourceWidth; j++)
                        {
                            gameBoard[i, j] = request.BoardData[i][j];
                        }
                    }
                }
                else
                {
                    return BadRequest("Either a CSV file or board data must be provided");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Invalid board data format: {ex.Message}");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try 
            {
                var game = new Game();
                _context.Games.Add(game);
                await _context.SaveChangesAsync();

                var board = new Board
                {
                    GameBoardData = BoardSerializer.SerializeBoard(BoardSerializer.ConvertToJagged(gameBoard)),
                    StateNumber = 1,
                    GameId = game.Id
                };

                _context.Boards.Add(board);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                await _context.Entry(game)
                    .Collection(g => g.Boards)
                    .Query()
                    .Where(b => b.GameId == game.Id)
                    .LoadAsync();

                return CreatedAtAction("GetGame", new { id = game.Id }, game);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // POST: api/Games/json
        [HttpPost("json")]
        public async Task<ActionResult<Game>> PostGameJson([FromBody] CreateGameRequest request)
        {
            return await PostGame(request);
        }

        private static bool[,] ParseCsvToBoard(string csvData, int width, int height)
        {
            // Initialize board with false values
            var board = new bool[height, width];
            
            var rows = csvData.Split('\n')
                             .Select(row => row.TrimEnd('\r'))
                             .ToArray();
            
            // Process only up to the specified height or available rows, whichever is smaller
            var rowCount = Math.Min(height, rows.Length);
            
            for (int i = 0; i < rowCount; i++)
            {
                var cells = rows[i].Split(',');
                // Process only up to the specified width or available cells, whichever is smaller
                var colCount = Math.Min(width, cells.Length);
                
                for (int j = 0; j < colCount; j++)
                {
                    if (!string.IsNullOrWhiteSpace(cells[j]))
                    {
                        var value = cells[j].Trim();
                        board[i, j] = value.Equals("1", StringComparison.OrdinalIgnoreCase) || 
                                     value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                     value.Equals("x", StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            
            return board;
        }

        // DELETE: api/Games/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGame(int id)
        {
            if (_context.Games == null)
            {
                return NotFound();
            }

            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
