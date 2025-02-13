namespace ConwaysGameOfLifeApi.Models.DTOs.Responses
{
    public class BoardResponse
    {
        public int Id { get; set; }
        public bool[][] GameBoard { get; set; } = Array.Empty<bool[]>();
        public int StateNumber { get; set; }
        public int GameId { get; set; }
    }
}