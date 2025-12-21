using System.ComponentModel.DataAnnotations;

namespace WebProjesi.Models
{
    public enum AppointmentStatus
    {
        [Display(Name = "Onay Bekliyor")]
        Pending = 0,

        [Display(Name = "Onaylandı")]
        Approved = 1,

        [Display(Name = "Reddedildi")]
        Rejected = 2,

        [Display(Name = "İptal Edildi")]
        Cancelled = 3
    }
}
