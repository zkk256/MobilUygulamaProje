using System.ComponentModel.DataAnnotations;

namespace WebProjesi.Models
{
    public class TrainerCreateEditVm
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = "";

        [Display(Name = "Hakkında")]
        public string? Bio { get; set; }

        // Çoklu seçim için (checkboxlardan gelen Service Id'ler)
        public List<int> SelectedServiceIds { get; set; } = new();
    }
}
