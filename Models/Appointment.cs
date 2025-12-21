using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebProjesi.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public int TrainerId { get; set; }

        [Required]
        public int ServiceId { get; set; }
        [Required]
        public string UserId { get; set; } = "";


        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }

        [Required]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        // Randevu oluşturulduğu andaki ücret/süre (sonradan hizmet değişirse geçmiş fiyat bozulmasın diye)
        [Column(TypeName = "decimal(18,2)")]
        public decimal StoredPrice { get; set; }

        public int StoredDurationMinutes { get; set; }

        // Navigation
        public Trainer? Trainer { get; set; }
        public Service? Service { get; set; }
    }
}
