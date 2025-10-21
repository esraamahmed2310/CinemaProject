using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CinemaController : Controller
    {
        private ApplicationDbContext _context = new();

        public ViewResult Index()
        {
            var Cinemas = _context.Cinemas.AsNoTracking().AsQueryable();

            return View(Cinemas.Select(e => new
            {
                e.Id,
                e.Name,
                e.Description,
                e.IsActive
            }).AsEnumerable());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Cinema Cinema, IFormFile file)
        {
            if (file is not null && file.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//images", fileName);

                //if(!System.IO.File.Exists(filePath))
                //{
                //    System.IO.File.Create(filePath);
                //}

                using (var stream = System.IO.File.Create(filePath))
                {
                    file.CopyTo(stream);
                }

                Cinema.Image = fileName;
            }

            _context.Cinemas.Add(Cinema);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var Cinema = _context.Cinemas.Find(id);

            if (Cinema is null)
                return RedirectToAction("NotFoundPage", "Home");

            return View(Cinema);
        }

        [HttpPost]
        public IActionResult Edit(Cinema Cinema, IFormFile file)
        {
            var CinemaInDB = _context.Cinemas.AsNoTracking().FirstOrDefault(e => e.Id == Cinema.Id);

            if (CinemaInDB is null)
                return RedirectToAction("NotFoundPage", "Home");

            if (file is not null)
            {
                if (file.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//images", fileName);

                    //if(!System.IO.File.Exists(filePath))
                    //{
                    //    System.IO.File.Create(filePath);
                    //}

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        file.CopyTo(stream);
                    }

                    Cinema.Image = fileName;
                }
            }
            else
            {
                Cinema.Image = CinemaInDB.Image;
            }

            _context.Cinemas.Update(Cinema);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var Cinema = _context.Cinemas.Find(id);

            if (Cinema is null)
                return RedirectToAction("NotFoundPage", "Home");

            _context.Cinemas.Remove(Cinema);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}