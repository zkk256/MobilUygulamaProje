using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProjesi.Data;
using WebProjesi.Models;

namespace WebProjesi.Controllers
{
    public class TrainersController : Controller
    {
        private readonly AppDbContext _db;

        public TrainersController(AppDbContext db)
        {
            _db = db;
        }

        private async Task FillServicesAsync()
        {
            ViewBag.AllServices = await _db.Services
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        // Herkes görebilir
        public async Task<IActionResult> Index()
        {
            var list = await _db.Trainers
                .Include(t => t.TrainerServices)
                .ThenInclude(ts => ts.Service)
                .OrderBy(t => t.FullName)
                .ToListAsync();

            return View(list);
        }

        // Herkes görebilir
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var trainer = await _db.Trainers
                .Include(t => t.TrainerServices)
                .ThenInclude(ts => ts.Service)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trainer == null) return NotFound();

            return View(trainer);
        }

        // Sadece Admin
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await FillServicesAsync();
            return View(new TrainerCreateEditVm());
        }

        // Sadece Admin
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainerCreateEditVm vm)
        {
            await FillServicesAsync();

            if (!ModelState.IsValid)
                return View(vm);

            var trainer = new Trainer
            {
                FullName = vm.FullName,
                Bio = vm.Bio
            };

            foreach (var sid in vm.SelectedServiceIds.Distinct())
            {
                trainer.TrainerServices.Add(new TrainerService
                {
                    ServiceId = sid
                });
            }

            _db.Trainers.Add(trainer);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Sadece Admin
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var trainer = await _db.Trainers
                .Include(t => t.TrainerServices)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trainer == null) return NotFound();

            await FillServicesAsync();

            var vm = new TrainerCreateEditVm
            {
                Id = trainer.Id,
                FullName = trainer.FullName,
                Bio = trainer.Bio,
                SelectedServiceIds = trainer.TrainerServices.Select(x => x.ServiceId).ToList()
            };

            return View(vm);
        }

        // Sadece Admin
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TrainerCreateEditVm vm)
        {
            if (id != vm.Id) return NotFound();

            await FillServicesAsync();

            if (!ModelState.IsValid)
                return View(vm);

            var trainer = await _db.Trainers
                .Include(t => t.TrainerServices)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trainer == null) return NotFound();

            trainer.FullName = vm.FullName;
            trainer.Bio = vm.Bio;

            // Seçimleri güncelle: eskileri sil, yenileri ekle
            trainer.TrainerServices.Clear();

            foreach (var sid in vm.SelectedServiceIds.Distinct())
            {
                trainer.TrainerServices.Add(new TrainerService
                {
                    TrainerId = trainer.Id,
                    ServiceId = sid
                });
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Sadece Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var trainer = await _db.Trainers
                .Include(t => t.TrainerServices)
                .ThenInclude(ts => ts.Service)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trainer == null) return NotFound();

            return View(trainer);
        }

        // Sadece Admin
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainer = await _db.Trainers
                .Include(t => t.TrainerServices)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trainer != null)
            {
                trainer.TrainerServices.Clear(); // join kayıtlarını da sil
                _db.Trainers.Remove(trainer);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TrainerExists(int id)
        {
            return _db.Trainers.Any(e => e.Id == id);
        }
    }
}
