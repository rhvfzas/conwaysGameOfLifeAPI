namespace ConwaysGameOfLifeApi.Models.DTOs.Responses
{
    public class GameResponse
    {
        public int Id { get; set; }
        public List<int> BoardIds { get; set; } = new List<int>();
    }
}