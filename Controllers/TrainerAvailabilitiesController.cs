using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebProjesi.Data;
using WebProjesi.Models;

namespace WebProjesi.Controllers
{
    public class TrainerAvailabilitiesController : Controller
    {
        private readonly AppDbContext _db;

        public TrainerAvailabilitiesController(AppDbContext db)
        {
            _db = db;
        }

        private static readonly string[] TR_DAYS = new[]
        {
            "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi", "Pazar"
        };

        private void FillDropdowns(int? trainerId = null, int? dayOfWeek = null)
        {
            ViewBag.Trainers = new SelectList(
                _db.Trainers.OrderBy(t => t.FullName),
                "Id",
                "FullName",
                trainerId
            );

            ViewBag.Days = new SelectList(
                TR_DAYS.Select((name, idx) => new { Id = idx, Name = name }),
                "Id",
                "Name",
                dayOfWeek
            );
        }

        // GET: /TrainerAvailabilities
        public async Task<IActionResult> Index()
        {
            var list = await _db.TrainerAvailabilities
                .Include(x => x.Trainer)
                .OrderBy(x => x.Trainer!.FullName)
                .ThenBy(x => x.DayOfWeek)
                .ThenBy(x => x.StartTime)
                .ToListAsync();

            return View(list);
        }

        // GET: /TrainerAvailabilities/Create
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            FillDropdowns();
            return View(new TrainerAvailability());
        }

        // POST: /TrainerAvailabilities/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainerAvailability model)
        {
            if (model.EndTime <= model.StartTime)
                ModelState.AddModelError("", "Bitiş saati başlangıç saatinden büyük olmalıdır.");

            if (!ModelState.IsValid)
            {
                FillDropdowns(model.TrainerId, model.DayOfWeek);
                return View(model);
            }

            _db.TrainerAvailabilities.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /TrainerAvailabilities/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.TrainerAvailabilities.FindAsync(id);
            if (item == null) return NotFound();

            FillDropdowns(item.TrainerId, item.DayOfWeek);
            return View(item);
        }

        // POST: /TrainerAvailabilities/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TrainerAvailability model)
        {
            if (id != model.Id) return BadRequest();

            if (model.EndTime <= model.StartTime)
                ModelState.AddModelError("", "Bitiş saati başlangıç saatinden büyük olmalıdır.");

            if (!ModelState.IsValid)
            {
                FillDropdowns(model.TrainerId, model.DayOfWeek);
                return View(model);
            }

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /TrainerAvailabilities/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.TrainerAvailabilities
                .Include(x => x.Trainer)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null) return NotFound();

            return View(item);
        }

        // POST: /TrainerAvailabilities/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.TrainerAvailabilities.FindAsync(id);
            if (item == null) return NotFound();

            _db.TrainerAvailabilities.Remove(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
