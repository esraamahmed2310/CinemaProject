using CinemaProject.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace CinemaProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CinemaController : Controller
    {
        private readonly IRepository<Cinema> _cinemaRepository;

        public CinemaController(IRepository<Cinema> cinemaRepository)
        {
            _cinemaRepository = cinemaRepository;
        }

        public async Task<ViewResult> Index(CancellationToken cancellationToken)
        {
            var cinemas = await _cinemaRepository.GetAsync(cancellationToken: cancellationToken, tracked: false);

            // بنرجع الـ list نفسها مش anonymous object (علشان الـ View تتعامل مع Model Cinema)
            return View(cinemas);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Cinema cinema, IFormFile file, CancellationToken cancellationToken)
        {
            if (file is not null && file.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream, cancellationToken);
                }

                cinema.Image = fileName;
            }

            await _cinemaRepository.CreateAsync(cinema);
            await _cinemaRepository.CommitAsync(cancellationToken);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var cinema = await _cinemaRepository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);

            if (cinema is null)
                return RedirectToAction("NotFoundPage", "Home");

            return View(cinema);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Cinema cinema, IFormFile file, CancellationToken cancellationToken)
        {
            var cinemaInDB = await _cinemaRepository.GetOneAsync(e => e.Id == cinema.Id, cancellationToken: cancellationToken);

            if (cinemaInDB is null)
                return RedirectToAction("NotFoundPage", "Home");

            if (file is not null && file.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream, cancellationToken);
                }

                cinema.Image = fileName;
            }
            else
            {
                cinema.Image = cinemaInDB.Image;
            }

            _cinemaRepository.Update(cinema);
            await _cinemaRepository.CommitAsync(cancellationToken);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var cinema = await _cinemaRepository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);

            if (cinema is null)
                return RedirectToAction("NotFoundPage", "Home");

            _cinemaRepository.Delete(cinema);
            await _cinemaRepository.CommitAsync(cancellationToken);

            return RedirectToAction(nameof(Index));
        }
    }
}
