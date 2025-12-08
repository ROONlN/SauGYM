using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SauGYM.Data;
using SauGYM.Models;
using Microsoft.AspNetCore.Authorization; // Yetkilendirme için şart

namespace SauGYM.Controllers
{
    [Authorize] // Giriş yapmayan kimse bu Controller'a erişemez
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. ADMİN PANELİ: TÜM RANDEVULAR (INDEX)
        // ==========================================
        // Eski Index metodunu sildik, yerine bu gelişmiş olanı koyduk.
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var allAppointments = await _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Include(a => a.AppUser) // Kim almış görelim
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(allAppointments);
        }

        // ==========================================
        // 2. ÜYE PANELİ: RANDEVULARIM
        // ==========================================
        public async Task<IActionResult> MyAppointments()
        {
            var userEmail = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userEmail);

            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            var myAppointments = await _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Where(a => a.AppUserId == user.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(myAppointments);
        }

        // ==========================================
        // 3. RANDEVU OLUŞTURMA (GET)
        // ==========================================
        public IActionResult Create()
        {
            // Dropdownları dolduruyoruz
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "ServiceName");
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "TrainerId", "FullName");
            return View();
        }

        // ==========================================
        // 4. RANDEVU OLUŞTURMA (POST - KAYIT)
        // ==========================================
        // ==========================================
        // 4. RANDEVU OLUŞTURMA (POST - KAYIT)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AppointmentId,AppointmentDate,ServiceId,TrainerId")] Appointment appointment)
        {
            var userEmail = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userEmail);

            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            var selectedService = await _context.Services.FindAsync(appointment.ServiceId);
            if (selectedService == null)
            {
                ModelState.AddModelError("", "Hizmet bulunamadı.");
                // Geri dönüş kodları...
            }
            else
            {
                // 2. Yeni Randevunun Başlangıç ve Bitiş saatlerini hesapla
                DateTime newStart = appointment.AppointmentDate;
                DateTime newEnd = newStart.AddMinutes(selectedService.Duration);

                // 3. O hocanın, O GÜNKÜ tüm randevularını veritabanından çek
                // (Sadece saati değil, o günkü tüm kayıtları alıp tek tek bakacağız)
                var existingAppointments = await _context.Appointments
                    .Include(a => a.Service) // Hizmet süresini bilmek için Include şart
                    .Where(a => a.TrainerId == appointment.TrainerId
                                && a.AppointmentDate.Date == newStart.Date // Sadece aynı gün
                                && a.Status != "İptal")
                    .ToListAsync();

                // 4. Tek tek çakışma var mı diye kontrol et
                bool isConflict = false;

                foreach (var existing in existingAppointments)
                {
                    // Mevcut randevunun aralığını bul
                    DateTime existingStart = existing.AppointmentDate;
                    DateTime existingEnd = existingStart.AddMinutes(existing.Service.Duration);

                    // ÇAKIŞMA FORMÜLÜ:
                    // (Yeni Başlangıç < Eski Bitiş) VE (Yeni Bitiş > Eski Başlangıç)
                    if (newStart < existingEnd && newEnd > existingStart)
                    {
                        isConflict = true;
                        break; // İlk çakışmada döngüyü kır
                    }
                }

                if (isConflict)
                {
                    // Hata mesajını biraz daha detaylandıralım
                    ModelState.AddModelError("", $"Seçtiğiniz antrenör bu saat aralığında dolu. (Ders bitiş saati çakışıyor).");
                }
            }
            // =================================================================

            // Validasyon temizliği (Aynen kalıyor)
            ModelState.Remove("AppUserId");
            ModelState.Remove("AppUser");
            ModelState.Remove("Trainer");
            ModelState.Remove("Service");

            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyAppointments));
            }

            // Hata varsa sayfayı tekrar doldur
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "ServiceName", appointment.ServiceId);
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "TrainerId", "FullName", appointment.TrainerId);
            return View(appointment);
        }

        // ==========================================
        // 5. DURUM DEĞİŞTİRME (ONAYLA/İPTAL)
        // ==========================================
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, string status)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = status;
            _context.Update(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}