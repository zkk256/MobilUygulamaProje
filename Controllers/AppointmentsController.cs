using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebProjesi.Data;
using WebProjesi.Models;

namespace WebProjesi.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly AppDbContext _db;

        public AppointmentsController(AppDbContext db)
        {
            _db = db;
        }

        private static readonly string[] TR_DAYS =
        {
            "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi", "Pazar"
        };

        private static int ToTrDayIndex(DateTime date)
        {
            return ((int)date.DayOfWeek + 6) % 7;
        }

        private void FillDropdowns(int? trainerId = null, int? serviceId = null)
        {
            ViewBag.Trainers = new SelectList(_db.Trainers.OrderBy(t => t.FullName), "Id", "FullName", trainerId);
            ViewBag.Services = new SelectList(_db.Services.OrderBy(s => s.Name), "Id", "Name", serviceId);
        }

        //Admin: tüm randevular
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var list = await _db.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .OrderByDescending(a => a.StartDateTime)
                .Select(a => new AppointmentAdminVm
                {
                    Id = a.Id,
                    UserEmail = _db.Users
                        .Where(u => u.Id == a.UserId)
                        .Select(u => u.Email!)
                        .FirstOrDefault() ?? a.UserId,

                    TrainerName = a.Trainer!.FullName,
                    ServiceName = a.Service!.Name,
                    StartDateTime = a.StartDateTime,
                    EndDateTime = a.EndDateTime,
                    Status = a.Status,
                    StoredPrice = a.StoredPrice
                })
                .ToListAsync();

            return View(list);
        }

        //Üye: kendi randevuları
        [Authorize]
        public async Task<IActionResult> My()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var list = await _db.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.StartDateTime)
                .ToListAsync();

            return View(list);
        }

        //Üye: randevu oluşturma ekranı
        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            FillDropdowns();
            var vm = new AppointmentCreateVm
            {
                Date = DateTime.Today,
                Time = new TimeSpan(9, 0, 0)
            };
            return View(vm);
        }

        //Üye: randevu oluştur
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentCreateVm vm)
        {
            FillDropdowns(vm.TrainerId, vm.ServiceId);

            if (!ModelState.IsValid)
                return View(vm);

            var service = await _db.Services.FindAsync(vm.ServiceId);
            if (service == null)
            {
                ModelState.AddModelError("", "Seçilen hizmet bulunamadı.");
                return View(vm);
            }

            var start = vm.Date.Date.Add(vm.Time);
            var end = start.AddMinutes(service.DurationMinutes);

            if (end <= start)
            {
                ModelState.AddModelError("", "Bitiş saati başlangıçtan büyük olmalıdır.");
                return View(vm);
            }

            //1) Uygunluk kontrolü
            var trDayIndex = ToTrDayIndex(start);

            bool hasAvailability = await _db.TrainerAvailabilities.AnyAsync(a =>
                a.TrainerId == vm.TrainerId &&
                a.DayOfWeek == trDayIndex &&
                a.StartTime <= start.TimeOfDay &&
                a.EndTime >= end.TimeOfDay
            );

            if (!hasAvailability)
            {
                ModelState.AddModelError("", $"Antrenör bu gün/saat aralığında müsait değil. (Gün: {TR_DAYS[trDayIndex]})");
                return View(vm);
            }

            //2) Çakışma kontrolü (aynı trainer, zaman kesişmesi)
            bool conflict = await _db.Appointments.AnyAsync(ap =>
                ap.TrainerId == vm.TrainerId &&
                ap.Status != AppointmentStatus.Rejected &&
                ap.Status != AppointmentStatus.Cancelled &&
                ap.StartDateTime < end &&
                start < ap.EndDateTime
            );

            if (conflict)
            {
                ModelState.AddModelError("", "Bu antrenör için seçilen saatte randevu var. Lütfen başka saat seç.");
                return View(vm);
            }

            //3) Kaydet (UserId ile)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var appointment = new Appointment
            {
                TrainerId = vm.TrainerId,
                ServiceId = vm.ServiceId,
                UserId = userId!, //randevu sahibi
                StartDateTime = start,
                EndDateTime = end,
                Status = AppointmentStatus.Pending,
                StoredPrice = service.Price,
                StoredDurationMinutes = service.DurationMinutes
            };

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();

            //Üye oluşturunca kendi listesine dönsün
            return RedirectToAction(nameof(My));
        }

        //Admin: onayla / reddet
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Approve(int id)
        {
            var ap = await _db.Appointments.FindAsync(id);
            if (ap == null) return NotFound();

            ap.Status = AppointmentStatus.Approved;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Reject(int id)
        {
            var ap = await _db.Appointments.FindAsync(id);
            if (ap == null) return NotFound();

            ap.Status = AppointmentStatus.Rejected;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }

    //Form ViewModel (Create ekranı)
    public class AppointmentCreateVm
    {
        [Required]
        public int TrainerId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan Time { get; set; }
    }


    public class AppointmentAdminVm
    {
        public int Id { get; set; }
        public string UserEmail { get; set; } = "";
        public string TrainerName { get; set; } = "";
        public string ServiceName { get; set; } = "";
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public decimal StoredPrice { get; set; }
    }

}
