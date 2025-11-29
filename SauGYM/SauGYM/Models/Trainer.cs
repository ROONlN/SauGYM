using System.ComponentModel.DataAnnotations;

namespace SauGYM.Models
{
    public class Trainer
    {
        [Key]
        public int TrainerId { get; set; }

        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [Display(Name = "Antrenör Adı Soyadı")]
        public string FullName { get; set; }

        [Display(Name = "Uzmanlık Alanı")]
        public string Specialization { get; set; } // Örn: Kilo Verme, Vücut Geliştirme

        // Antrenörün fotoğrafı için (Arayüzde güzel durması için ekledik)
        public string? ImageUrl { get; set; }

        // Antrenörün çalışma saatleri (Basit bir metin olarak tutabiliriz: "09:00 - 17:00")
        [Display(Name = "Çalışma Saatleri")]
        public string WorkingHours { get; set; }

        public ICollection<Appointment> Appointments { get; set; }
    }
}
