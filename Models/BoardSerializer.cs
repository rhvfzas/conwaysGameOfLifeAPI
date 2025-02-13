using System.Text.Json;

namespace ConwaysGameOfLifeApi.Models
{
    public static class BoardSerializer
    {
        public static string SerializeBoard(bool[][] board)
        {
            return JsonSerializer.Serialize(board);
        }

        public static bool[][] DeserializeBoard(string boardData)
        {
            return JsonSerializer.Deserialize<bool[][]>(boardData) ?? Array.Empty<bool[]>();
        }

        // Helper methods to convert between 2D and jagged arrays if needed
        public static bool[][] ConvertToJagged(bool[,] board2D)
        {
            int rows = board2D.GetLength(0);
            int cols = board2D.GetLength(1);
            var jagged = new bool[rows][];
            
            for (int i = 0; i < rows; i++)
            {
                jagged[i] = new bool[cols];
                for (int j = 0; j < cols; j++)
                {
                    jagged[i][j] = board2D[i, j];
                }
            }
            
            return jagged;
        }

        public static bool[,] ConvertTo2D(bool[][] jaggedArray)
        {
            if (jaggedArray == null || jaggedArray.Length == 0)
                return new bool[0, 0];

            int rows = jaggedArray.Length;
            int cols = jaggedArray[0].Length;
            var board2D = new bool[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    board2D[i, j] = jaggedArray[i][j];
                }
            }

            return board2D;
        }
    }
}
