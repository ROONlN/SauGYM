using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SauGYM.Data;
using SauGYM.Models;
using Microsoft.AspNetCore.Authorization;

namespace SauGYM.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var allAppointments = await _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Include(a => a.AppUser)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(allAppointments);
        }

        public async Task<IActionResult> MyAppointments()
        {
            var userEmail = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userEmail);

            if (user == null) return NotFound();

            var myAppointments = await _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Where(a => a.AppUserId == user.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(myAppointments);
        }

        public IActionResult Create()
        {
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "ServiceName");
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "TrainerId", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AppointmentId,AppointmentDate,ServiceId,TrainerId")] Appointment appointment)
        {
            var userEmail = User.Identity.Name;
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userEmail);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            appointment.AppUserId = user.Id;
            appointment.Status = "Onay Bekliyor";

            var selectedTrainer = await _context.Trainers.FindAsync(appointment.TrainerId);
            var selectedService = await _context.Services.FindAsync(appointment.ServiceId);

            if (selectedTrainer != null && selectedService != null)
            {
                string hocaUzmanlik = selectedTrainer.Specialization.ToLower();
                string hizmetAdi = selectedService.ServiceName.ToLower();
                bool uyumluMu = hizmetAdi.Contains(hocaUzmanlik) || hocaUzmanlik.Contains(hizmetAdi);

                if (!uyumluMu && !hocaUzmanlik.Contains("fitness") && !hocaUzmanlik.Contains("trainer"))
                {
                    ModelState.AddModelError("", $"Warning: {selectedTrainer.FullName} specializes in '{selectedTrainer.Specialization}'. Not suitable for '{selectedService.ServiceName}'.");
                }

                DateTime newStart = appointment.AppointmentDate;
                DateTime newEnd = newStart.AddMinutes(selectedService.Duration);

                var existingAppointments = await _context.Appointments
                    .Include(a => a.Service)
                    .Where(a => a.TrainerId == appointment.TrainerId
                                && a.AppointmentDate.Date == newStart.Date
                                && a.Status != "İptal"
                                && a.Status != "Cancelled")
                    .ToListAsync();

                foreach (var existing in existingAppointments)
                {
                    DateTime existingStart = existing.AppointmentDate;
                    DateTime existingEnd = existingStart.AddMinutes(existing.Service.Duration);

                    if (newStart < existingEnd && newEnd > existingStart)
                    {
                        ModelState.AddModelError("", $"Time conflict! The trainer is busy between {existingStart:HH:mm} - {existingEnd:HH:mm}.");
                        break;
                    }
                }
            }

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

            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "ServiceName", appointment.ServiceId);
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "TrainerId", "FullName", appointment.TrainerId);
            return View(appointment);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            var userEmail = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userEmail);

            if (user == null || appointment.AppUserId != user.Id)
            {
                return Unauthorized();
            }

            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "ServiceName", appointment.ServiceId);
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "TrainerId", "FullName", appointment.TrainerId);
            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AppointmentId,AppointmentDate,ServiceId,TrainerId")] Appointment appointment)
        {
            if (id != appointment.AppointmentId) return NotFound();

            var userEmail = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userEmail);
            if (user == null) return RedirectToPage("/Account/Login");

            appointment.AppUserId = user.Id;
            appointment.Status = "Onay Bekliyor";

            var selectedService = await _context.Services.FindAsync(appointment.ServiceId);
            var selectedTrainer = await _context.Trainers.FindAsync(appointment.TrainerId);

            if (selectedService != null && selectedTrainer != null)
            {
                string hocaUzmanlik = selectedTrainer.Specialization.ToLower();
                string hizmetAdi = selectedService.ServiceName.ToLower();
                bool uyumluMu = hizmetAdi.Contains(hocaUzmanlik) || hocaUzmanlik.Contains(hizmetAdi);

                if (!uyumluMu && !hocaUzmanlik.Contains("fitness") && !hocaUzmanlik.Contains("trainer"))
                {
                    ModelState.AddModelError("", $"Warning: {selectedTrainer.FullName} specializes in '{selectedTrainer.Specialization}'. Not suitable for '{selectedService.ServiceName}'.");
                }

                DateTime newStart = appointment.AppointmentDate;
                DateTime newEnd = newStart.AddMinutes(selectedService.Duration);

                var conflicts = await _context.Appointments
                    .Include(a => a.Service)
                    .Where(a => a.TrainerId == appointment.TrainerId
                                && a.AppointmentDate.Date == newStart.Date
                                && a.AppointmentId != id
                                && a.Status != "İptal"
                                && a.Status != "Cancelled")
                    .ToListAsync();

                foreach (var existing in conflicts)
                {
                    DateTime existingStart = existing.AppointmentDate;
                    DateTime existingEnd = existingStart.AddMinutes(existing.Service.Duration);

                    if (newStart < existingEnd && newEnd > existingStart)
                    {
                        ModelState.AddModelError("", $"Time conflict! The trainer is busy between {existingStart:HH:mm} - {existingEnd:HH:mm}.");
                        break;
                    }
                }
            }

            ModelState.Remove("AppUserId");
            ModelState.Remove("AppUser");
            ModelState.Remove("Trainer");
            ModelState.Remove("Service");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(MyAppointments));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Appointments.Any(e => e.AppointmentId == id)) return NotFound();
                    else throw;
                }
            }

            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "ServiceName", appointment.ServiceId);
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "TrainerId", "FullName", appointment.TrainerId);
            return View(appointment);
        }

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