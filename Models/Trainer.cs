using System.ComponentModel.DataAnnotations;

namespace WebProjesi.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Ad Soyad")]
        [StringLength(80)]
        public string FullName { get; set; } = "";

        [Display(Name = "Hakkında")]
        [StringLength(600)]
        public string? Bio { get; set; }

        public List<TrainerService> TrainerServices { get; set; } = new();


    }

}
