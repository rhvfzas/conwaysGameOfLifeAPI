using System.Text;

namespace ConwaysGameOfLifeApi.Models
{
    public static class GameBoardUtils
    {
        public static bool[,] CalculateNextState(bool[,] currentBoard)
        {
            int height = currentBoard.GetLength(0);
            int width = currentBoard.GetLength(1);
            var nextBoard = new bool[height, width];

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    int liveNeighbors = CountLiveNeighbors(currentBoard, row, col);
                    bool currentCell = currentBoard[row, col];

                    nextBoard[row, col] = (currentCell && (liveNeighbors == 2 || liveNeighbors == 3)) ||
                                         (!currentCell && liveNeighbors == 3);
                }
            }

            return nextBoard;
        }

        public static int CountLiveNeighbors(bool[,] board, int row, int col)
        {
            int count = 0;
            int height = board.GetLength(0);
            int width = board.GetLength(1);

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;

                    int newRow = row + i;
                    int newCol = col + j;

                    if (newRow >= 0 && newRow < height && newCol >= 0 && newCol < width)
                    {
                        if (board[newRow, newCol]) count++;
                    }
                }
            }

            return count;
        }

        public static bool IsAllDead(bool[,] board)
        {
            int height = board.GetLength(0);
            int width = board.GetLength(1);

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    if (board[row, col])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static string BoardToString(bool[,] board)
        {
            int height = board.GetLength(0);
            int width = board.GetLength(1);
            var sb = new StringBuilder();

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    sb.Append(board[row, col] ? '1' : '0');
                }
            }
            return sb.ToString();
        }
    }
}