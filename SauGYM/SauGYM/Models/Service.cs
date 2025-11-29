using System.ComponentModel.DataAnnotations;

namespace SauGYM.Models
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur.")]
        [Display(Name = "Hizmet Adı")]
        public string ServiceName { get; set; } // Örn: Pilates, Fitness

        [Display(Name = "Süre (Dakika)")]
        public int Duration { get; set; } // Örn: 45 dk

        [Display(Name = "Ücret")]
        public decimal Price { get; set; }

        // Bir hizmetin birden fazla randevusu olabilir
        public ICollection<Appointment> Appointments { get; set; }
    }
}
