using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProjesi.Data;
using WebProjesi.Models;

namespace WebProjesi.Controllers.Api
{
    [ApiController]
    [Route("api/trainers")]
    public class TrainersApiController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TrainersApiController(AppDbContext db)
        {
            _db = db;
        }

        private static int ToTrDayIndex(DateTime date)
        {
            return ((int)date.DayOfWeek + 6) % 7;
        }

        // GET: /api/trainers/available?date=2025-12-10&time=11:30&serviceId=1
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable([FromQuery] DateTime date, [FromQuery] string time, [FromQuery] int serviceId)
        {
            if (!TimeSpan.TryParse(time, out var timeOfDay))
                return BadRequest("time format should be HH:mm (example: 09:30)");

            var service = await _db.Services.FindAsync(serviceId);
            if (service == null) return NotFound("Service not found.");

            var start = date.Date.Add(timeOfDay);
            var end = start.AddMinutes(service.DurationMinutes);

            var trDay = ToTrDayIndex(start);

            // LINQ filtreleme:
            // 1) Bu hizmeti veriyor mu (TrainerServices)
            // 2) O gün/saat aralığında müsait mi (TrainerAvailabilities)
            // 3) Aynı saatte çakışan randevusu var mı (Appointments)
            var trainers = await _db.Trainers
                .Where(t => t.TrainerServices.Any(ts => ts.ServiceId == serviceId))
                .Where(t =>
                    _db.TrainerAvailabilities.Any(a =>
                        a.TrainerId == t.Id &&
                        a.DayOfWeek == trDay &&
                        a.StartTime <= start.TimeOfDay &&
                        a.EndTime >= end.TimeOfDay
                    )
                )
                .Where(t =>
                    !_db.Appointments.Any(ap =>
                        ap.TrainerId == t.Id &&
                        ap.Status != AppointmentStatus.Rejected &&
                        ap.Status != AppointmentStatus.Cancelled &&
                        ap.StartDateTime < end &&
                        start < ap.EndDateTime
                    )
                )
                .OrderBy(t => t.FullName)
                .Select(t => new { t.Id, t.FullName })
                .ToListAsync();

            return Ok(trainers);
        }


        // GET /api/trainers  -> tüm antrenörler
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.Trainers
                .OrderBy(t => t.FullName)
                .Select(t => new { t.Id, t.FullName })
                .ToListAsync();

            return Ok(list);
        }
    }
}
