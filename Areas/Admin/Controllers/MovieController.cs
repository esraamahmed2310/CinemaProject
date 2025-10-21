using CinemaProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MovieProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MovieController : Controller
    {
        private ApplicationDbContext _context = new();

        public ViewResult Index(FilterVM filterVM, int page = 1)
        {
            const decimal discount = 50;
            var Movies = _context.Movies.AsNoTracking().AsQueryable();

            // Add Filters
            if (filterVM.MovieName is not null)
            {
                Movies = Movies.Where(e => e.Name.Contains(filterVM.MovieName));
                ViewBag.MovieName = filterVM.MovieName;
            }

            //if (filterVM.MinPrice > 0)
            //{
            //    Movies = Movies.Where(e => (e.Price - e.Price * (e.Discount / 100)) > filterVM.MinPrice);
            //    ViewBag.MinPrice = filterVM.MinPrice;
            //}

            //if (filterVM.MaxPrice > 0)
            //{
            //    Movies = Movies.Where(e => (e.Price - e.Price * (e.Discount / 100)) < filterVM.MaxPrice);
            //    ViewBag.MaxPrice = filterVM.MaxPrice;
            //}

            if (filterVM.CategoryId > 0)
            {
                Movies = Movies.Where(e => e.CategoryId == filterVM.CategoryId);
                ViewBag.CategoryId = filterVM.CategoryId;
            }

            //if (filterVM.IsHot)
            //{
            //    Movies = Movies.Where(e => e.Discount > discount);
            //    ViewBag.IsHot = filterVM.IsHot;
            //}

            // List Of categories
            var categories = _context.Categories.AsQueryable();
            //ViewBag.Categories = categories.ToList();
            ViewData["Categories"] = categories.ToList();

            // Add Pagination
            var totalPages = Math.Ceiling(Movies.Count() / 8.0);
            Movies = Movies.Skip((page - 1) * 8).Take(8);
            ViewBag.totalPages = totalPages;
            ViewBag.currentPage = page;

            return View(Movies.Select(e => new
            {
                e.Id,
                e.Name,
                e.Description,
                e.Status,
                e.Price,
                e.ShowTime,
                CategoryName = e.Category.Name,
                CinemaName = e.Cinema.Name

            }).AsEnumerable());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Movie Movie, IFormFile file)
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

                Movie.MainImg = fileName;
            }

            _context.Movies.Add(Movie);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var Movie = _context.Movies.Find(id);

            if (Movie is null)
                return RedirectToAction("NotFoundPage", "Home");

            return View(Movie);
        }

        [HttpPost]
        public IActionResult Edit(Movie Movie, IFormFile file)
        {
            var MovieInDB = _context.Movies.AsNoTracking().FirstOrDefault(e => e.Id == Movie.Id);

            if (MovieInDB is null)
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

                    Movie.MainImg = fileName;
                }
            }
            else
            {
                Movie.MainImg = MovieInDB.MainImg;
            }

            _context.Movies.Update(Movie);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var Movie = _context.Movies.Find(id);

            if (Movie is null)
                return RedirectToAction("NotFoundPage", "Home");

            _context.Movies.Remove(Movie);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}