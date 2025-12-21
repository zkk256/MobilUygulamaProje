using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProjesi.Data;
using WebProjesi.Models;

namespace WebProjesi.Controllers
{
    public class ServicesController : Controller
    {
        private readonly AppDbContext _db;

        public ServicesController(AppDbContext db)
        {
            _db = db;
        }

        //Herkes görebilir
        public async Task<IActionResult> Index()
        {
            return View(await _db.Services.ToListAsync());
        }

        //Herkes görebilir
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var service = await _db.Services.FirstOrDefaultAsync(m => m.Id == id);
            if (service == null) return NotFound();

            return View(service);
        }

        //Sadece Admin
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        //Sadece Admin
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            if (!ModelState.IsValid) return View(service);

            _db.Services.Add(service);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //Sadece Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var service = await _db.Services.FindAsync(id);
            if (service == null) return NotFound();

            return View(service);
        }

        //Sadece Admin
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Service service)
        {
            if (id != service.Id) return NotFound();
            if (!ModelState.IsValid) return View(service);

            try
            {
                _db.Update(service);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceExists(service.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        //Sadece Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var service = await _db.Services.FirstOrDefaultAsync(m => m.Id == id);
            if (service == null) return NotFound();

            return View(service);
        }

        //Sadece Admin
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _db.Services.FindAsync(id);
            if (service != null)
            {
                _db.Services.Remove(service);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ServiceExists(int id)
        {
            return _db.Services.Any(e => e.Id == id);
        }
    }
}
