using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConwaysGameOfLifeApi.Models
{
    public class Board
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(TypeName = "jsonb")]
        [Required]
        [JsonIgnore]
        public required string GameBoardData { get; set; }

        [NotMapped]
        public bool[][] GameBoard
        {
            get => JsonSerializer.Deserialize<bool[][]>(GameBoardData) ?? Array.Empty<bool[]>();
            set => GameBoardData = JsonSerializer.Serialize(value ?? Array.Empty<bool[]>());
        }

        [Required]
        public int StateNumber { get; set; }
        
        [Required]
        public int GameId { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(GameId))]
        public virtual Game Game { get; set; } = null!;
    }
}
