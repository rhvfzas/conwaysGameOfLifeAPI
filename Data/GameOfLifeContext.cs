using Microsoft.EntityFrameworkCore;
using ConwaysGameOfLifeApi.Models;

namespace ConwaysGameOfLifeApi.Data
{
    public class GameOfLifeContext : DbContext
    {
        public GameOfLifeContext(DbContextOptions<GameOfLifeContext> options)
            : base(options)
        {
        }

        public DbSet<Board> Boards { get; set; }
        public DbSet<Game> Games { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Board>()
                .HasOne(b => b.Game)
                .WithMany(g => g.Boards)
                .HasForeignKey(b => b.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Board>()
                .HasIndex(b => new { b.GameId, b.StateNumber });
        }

        public async Task<Board> GetBoardDataForGame(int gameId, int stateNumber)
        {
            if (!await Games.AnyAsync(g => g.Id == gameId))
            {
                throw new ArgumentException("Game not found", nameof(gameId));
            }

            var board = await Boards
                .Where(b => b.GameId == gameId && b.StateNumber == stateNumber)
                .FirstOrDefaultAsync();

            if (board == null)
            {
                throw new ArgumentException("Board not found for the given state number", nameof(stateNumber));
            }

            return board;
        }
    }
}
