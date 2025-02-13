using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConwaysGameOfLifeApi.Data;
using ConwaysGameOfLifeApi.Models;
using System.Text;
using ConwaysGameOfLifeApi.Models.DTOs.Requests;
using ConwaysGameOfLifeApi.Models.DTOs.Responses;

namespace ConwaysGameOfLifeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoardsController : ControllerBase
    {
        private readonly GameOfLifeContext _context;

        public BoardsController(GameOfLifeContext context)
        {
            _context = context;
        }

        // GET: api/Boards
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BoardResponse>>> GetBoards()
        {
            if (_context.Boards == null)
            {
                return NotFound();
            }
            var boards = await _context.Boards.ToListAsync();
            return boards.Select(b => new BoardResponse
            {
                Id = b.Id,
                GameBoard = b.GameBoard,
                StateNumber = b.StateNumber,
                GameId = b.GameId
            }).ToList();
        }

        // GET: api/Boards/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BoardResponse>> GetBoard(int id)
        {
            if (_context.Boards == null)
            {
                return NotFound();
            }
            var board = await _context.Boards.FindAsync(id);

            if (board == null)
            {
                return NotFound();
            }

            return new BoardResponse
            {
                Id = board.Id,
                GameBoard = board.GameBoard,
                StateNumber = board.StateNumber,
                GameId = board.GameId
            };
        }

        // POST: api/Boards
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Board>> PostBoard(Board board)
        {
          if (_context.Boards == null)
          {
              return Problem("Entity set 'GameOfLifeContext.Boards'  is null.");
          }
            _context.Boards.Add(board);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBoard", new { id = board.Id }, board);
        }

        // POST: api/Boards/{id}/advance/{steps}
        [HttpPost("{id}/advance")]
        public async Task<ActionResult<BoardResponse>> AdvanceState(int id, [FromBody] AdvanceStateRequest request)
        {
            if (request.Steps < 1)
            {
                return BadRequest("Steps must be greater than 0");
            }

            if (_context.Boards == null)
            {
                return Problem("Entity set 'GameOfLifeContext.Boards' is null.");
            }

            var currentBoard = await _context.Boards
                .Include(b => b.Game)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (currentBoard == null)
            {
                return NotFound();
            }

            var nextStateNumber = currentBoard.StateNumber + request.Steps;

            try 
            {
                var existingBoard = await _context.GetBoardDataForGame(currentBoard.GameId, nextStateNumber);
                return new BoardResponse {
                    Id = existingBoard.Id,
                    GameBoard = existingBoard.GameBoard,
                    StateNumber = existingBoard.StateNumber,
                    GameId = existingBoard.GameId
                };
            }
            catch (Exception)
            {
                // Board not found, continue to calculate next state
            }

            var gameBoard = BoardSerializer.ConvertTo2D(currentBoard.GameBoard);

            // Calculate multiple steps
            for (int i = 0; i < request.Steps; i++)
            {
                gameBoard = GameBoardUtils.CalculateNextState(gameBoard);
            }

            var newBoard = new Board
            {
                GameBoardData = BoardSerializer.SerializeBoard(BoardSerializer.ConvertToJagged(gameBoard)),
                StateNumber = nextStateNumber,
                GameId = currentBoard.GameId
            };

            _context.Boards.Add(newBoard);
            await _context.SaveChangesAsync();

            return new BoardResponse
            {
                Id = newBoard.Id,
                GameBoard = newBoard.GameBoard,
                StateNumber = newBoard.StateNumber,
                GameId = newBoard.GameId
            };
        }

        // POST: api/Boards/{id}/next-state
        [HttpPost("{id}/next-state")]
        public async Task<ActionResult<BoardResponse>> GetNextState(int id)
        {
            if (_context.Boards == null)
            {
                return Problem("Entity set 'GameOfLifeContext.Boards' is null.");
            }

            var currentBoard = await _context.Boards
                .Include(b => b.Game)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (currentBoard == null)
            {
                return NotFound();
            }

            var nextStateNumber = currentBoard.StateNumber + 1;

            try 
            {
                var existingBoard = await _context.GetBoardDataForGame(currentBoard.GameId, nextStateNumber);
                return new BoardResponse {
                    Id = existingBoard.Id,
                    GameBoard = existingBoard.GameBoard,
                    StateNumber = existingBoard.StateNumber,
                    GameId = existingBoard.GameId
                };
            }
            catch (Exception)
            {
                // Board not found, continue to calculate next state
            }

            var currentGameBoard = BoardSerializer.ConvertTo2D(currentBoard.GameBoard);
            var nextGameBoard = GameBoardUtils.CalculateNextState(currentGameBoard);

            var newBoard = new Board
            {
                GameBoardData = BoardSerializer.SerializeBoard(BoardSerializer.ConvertToJagged(nextGameBoard)),
                StateNumber = nextStateNumber,
                GameId = currentBoard.GameId
            };

            _context.Boards.Add(newBoard);
            await _context.SaveChangesAsync();

            return new BoardResponse
            {
                Id = newBoard.Id,
                GameBoard = newBoard.GameBoard,
                StateNumber = newBoard.StateNumber,
                GameId = newBoard.GameId
            };
        }

        // POST: api/Boards/{id}/final-state
        [HttpPost("{id}/final-state")]
        public async Task<ActionResult<BoardResponse>> GetFinalBoardState(int id)
        {
            if (_context.Boards == null)
            {
                return Problem("Entity set 'GameOfLifeContext.Boards' is null.");
            }

            var currentBoard = await _context.Boards
                .Include(b => b.Game)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (currentBoard == null)
            {
                return NotFound();
            }

            const int MAX_STEPS = 300;
            const int STABLE_CHECK_COUNT = 3;
            const int REPEAT_CHECK_COUNT = 10;

            var gameBoard = BoardSerializer.ConvertTo2D(currentBoard.GameBoard);
            var history = new List<string>();
            var currentStep = 0;

            for (; currentStep < MAX_STEPS; currentStep++)
            {
                if (GameBoardUtils.IsAllDead(gameBoard))
                {
                    break;
                }

                var boardString = GameBoardUtils.BoardToString(gameBoard);
                history.Add(boardString);

                // Check if board hasn't changed in last 3 moves
                if (history.Count >= STABLE_CHECK_COUNT)
                {
                    var lastFew = history.TakeLast(STABLE_CHECK_COUNT).Distinct().Count();
                    if (lastFew == 1)
                    {
                        break;
                    }
                }

                // Check if pattern repeats within last 10 moves
                if (history.Count >= REPEAT_CHECK_COUNT)
                {
                    var patternRepeats = false;
                    var lastBoard = history.Last();
                    for (int j = history.Count - 2; j >= history.Count - REPEAT_CHECK_COUNT; j--)
                    {
                        if (history[j] == lastBoard)
                        {
                            patternRepeats = true;
                            break;
                        }
                    }
                    if (patternRepeats)
                    {
                        break;
                    }
                }

                gameBoard = GameBoardUtils.CalculateNextState(gameBoard);
            }
            var nextStateNumber = currentBoard.StateNumber + currentStep;
            var newBoard = new Board
            {
                GameBoardData = BoardSerializer.SerializeBoard(BoardSerializer.ConvertToJagged(gameBoard)),
                StateNumber = nextStateNumber,
                GameId = currentBoard.GameId
            };

            _context.Boards.Add(newBoard);
            await _context.SaveChangesAsync();

            return new BoardResponse
            {
                Id = newBoard.Id,
                GameBoard = newBoard.GameBoard,
                StateNumber = newBoard.StateNumber,
                GameId = newBoard.GameId
            };
        }

        // DELETE: api/Boards/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBoard(int id)
        {
            if (_context.Boards == null)
            {
                return NotFound();
            }
            var board = await _context.Boards.FindAsync(id);
            if (board == null)
            {
                return NotFound();
            }

            _context.Boards.Remove(board);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BoardExists(int id)
        {
            return (_context.Boards?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
