using System.ComponentModel.DataAnnotations;

namespace WebProjesi.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        [Range(10, 300)]
        public int DurationMinutes { get; set; }

        [Range(0, 100000)]
        public decimal Price { get; set; }

        public List<TrainerService> TrainerServices { get; set; } = new();
    }
}
