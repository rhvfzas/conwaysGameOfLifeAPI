namespace ConwaysGameOfLifeApi.Models.DTOs.Requests
{
    public class CreateGameRequest
    {
        public IFormFile? CsvFile { get; set; }
        public bool[][]? BoardData { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}