using System.ComponentModel.DataAnnotations;

namespace WebProjesi.Models
{
    public class TrainerAvailability
    {
        public int Id { get; set; }

        [Required]
        public int TrainerId { get; set; }

        [Required]
        [Range(0, 6)]
        public int DayOfWeek { get; set; } // 0=Pzt, 1=Salı, 2=Çar, 3=Per, 4=Cuma, 5=Cmt, 6=Pazar

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public Trainer? Trainer { get; set; }
    }
}
