using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SauGYM.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        [Display(Name = "Randevu Tarihi")]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "Durum")]
        public string Status { get; set; } = "Onay Bekliyor"; // Onaylandı, İptal, Bekliyor

        // İlişkiler (Foreign Keys)

        // Hangi Hizmet?
        public int ServiceId { get; set; }
        public Service Service { get; set; }

        // Hangi Antrenör?
        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; }

        // Hangi Üye?
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
