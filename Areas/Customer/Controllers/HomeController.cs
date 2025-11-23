using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CinemaProject.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ApplicationDbContext _context = new();


        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(FilterVM filterVM, int page = 1)
        {
            const decimal discount = 50;
            var products = _context.Movies.AsQueryable();

            // Add Filters
            products = products.Include(e => e.Category);

            if (filterVM.MovieName is not null)
            {
                products = products.Where(e => e.Name.Contains(filterVM.MovieName));
                ViewBag.ProductName = filterVM.MovieName;
            }

            if (filterVM.MinPrice > 0)
            {
                products = products.Where(e => e.Price - e.Price * (e.Discount / 100) > filterVM.MinPrice);
                ViewBag.MinPrice = filterVM.MinPrice;
            }

            if (filterVM.MaxPrice > 0)
            {
                products = products.Where(e => e.Price - e.Price * (e.Discount / 100) < filterVM.MaxPrice);
                ViewBag.MaxPrice = filterVM.MaxPrice;
            }

            if (filterVM.CategoryId > 0)
            {
                products = products.Where(e => e.CategoryId == filterVM.CategoryId);
                ViewBag.CategoryId = filterVM.CategoryId;
            }

            if (filterVM.IsHot)
            {
                products = products.Where(e => e.Discount > discount);
                ViewBag.IsHot = filterVM.IsHot;
            }

            // List Of categories
            var categories = _context.Categories.AsQueryable();
            //ViewBag.Categories = categories.ToList();
            ViewData["Categories"] = categories.ToList();

            // Add Pagination
            var totalPages = Math.Ceiling(products.Count() / 8.0);
            products = products.Skip((page - 1) * 8).Take(8);
            ViewBag.totalPages = totalPages;
            ViewBag.currentPage = page;

            return View(products.ToList());
        }

        public async Task<IActionResult> Item(int id)
        {
            var movie = await _context.Movies.Include(e => e.Category).FirstOrDefaultAsync(e => e.Id == id);

            if (movie is null) return NotFound();

            var relatedMovie = _context.Movies.Include(e => e.Category).Where(e => e.Name.Contains(movie.Name) && e.Id != movie.Id).Skip(0).Take(4);

            ViewBag.relatedMovie = relatedMovie;

            return View(movie);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
