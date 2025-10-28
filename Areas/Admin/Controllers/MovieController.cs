using CinemaProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MovieProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MovieController : Controller
    {
        //private ApplicationDbContext _context = new();
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Cinema> _cinemaRepository;
        private readonly IRepository<Movie> _movieRepository;

        public MovieController(
            IRepository<Category> categoryRepository,
            IRepository<Cinema> cinemaRepository,
            IRepository<Movie> movieRepository
            )

        {
            _categoryRepository = categoryRepository;
            _cinemaRepository = cinemaRepository;
            _movieRepository = movieRepository;
            
        }

        public async Task<ViewResult> Index(FilterVM filterVM, CancellationToken cancellationToken, int page = 1)
        {
            //const decimal discount = 50;
            var Movies = await _movieRepository.GetAsync(includes: [e => e.Category, e => e.Cinema], tracked: false, cancellationToken: cancellationToken);

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
            var categories = await _categoryRepository.GetAsync(cancellationToken: cancellationToken);
            //ViewBag.Categories = categories.ToList();
            ViewData["Categories"] = categories.ToList();

            // List Of cinemas
            var cinemas = await _cinemaRepository.GetAsync(cancellationToken: cancellationToken);
            //ViewBag.Categories = categories.ToList();
            ViewData["Cinemas"] = cinemas.ToList();

            // Add Pagination
            var totalPages = Math.Ceiling(Movies.Count() / 8.0);
            Movies = Movies.Skip((page - 1) * 8).Take(8);
            ViewBag.totalPages = totalPages;
            ViewBag.currentPage = page;

            return View(Movies.AsEnumerable());
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            CreateMovieVM createMovieVM = new()
            {
                Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name"),
                Cinemas = new SelectList(await _cinemaRepository.GetAllAsync(), "Id", "Name"),
            };
            return View(createMovieVM);
        }

        //[HttpPost]
        //public IActionResult Create(Movie Movie, IFormFile file)
        //{
        //    if (file is not null && file.Length > 0)
        //    {
        //        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        //        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//images", fileName);

        //        //if(!System.IO.File.Exists(filePath))
        //        //{
        //        //    System.IO.File.Create(filePath);
        //        //}

        //        using (var stream = System.IO.File.Create(filePath))
        //        {
        //            file.CopyTo(stream);
        //        }

        //        Movie.MainImg = fileName;
        //    }

        //    _context.Movies.Add(Movie);
        //    _context.SaveChanges();

        //    return RedirectToAction(nameof(Index));
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMovieVM vm, CancellationToken cancellationToken)
        {
            ModelState.Remove(nameof(vm.Categories));
            ModelState.Remove(nameof(vm.Cinemas));

            if (!ModelState.IsValid)
            {
                // لازم نرجّع القوائم المنسدلة تاني لو فيه error علشان يعرض المدخلات الخطأ
                vm.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name");
                vm.Cinemas = new SelectList(await _cinemaRepository.GetAllAsync(), "Id", "Name");
                return View(vm);
            }

            // الصورة الاساسية
            string? mainFileName = null;
            if (vm.MainImage is not null && vm.MainImage.Length > 0)
            {
                // توليد guid + extension
                mainFileName = Guid.NewGuid().ToString() + Path.GetExtension(vm.MainImage.FileName);
                // وضعها في ال path الخاص بيها
                var mainPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//Images//Movies", mainFileName);
                // حفظ الصورة في المسار
                using var stream = System.IO.File.Create(mainPath);
                vm.MainImage.CopyTo(stream);
                //vm.MainImage = mainFileName;
            }

            // إنشاء كائن Movie جديد
            var movie = new Movie
            {
                Name = vm.Name,
                Description = vm.Descrption,
                Price = vm.Price,
                Status = vm.Status,
                ShowTime = vm.Date,
   
                CategoryId = vm.CategoryId,
                CinemaId = vm.CinemaId,
                MainImg = mainFileName,
                
            };
            await _movieRepository.CreateAsync(movie);
            await _movieRepository.CommitAsync();


            // Response.Cookies.Append("Cookies-succuss", "Done You Add new Movie 👌");  دي مشكلتها انها مش بتختفي غير لما تعملها اعدادات معينه في البارامتر التاليت
            TempData["Cookies-succuss"] = "Done You Added new Movie";

            // رفع الصور الفرعية (لو فيه)
            //if (vm.SubImages != null && vm.SubImages.Count > 0)
            //{
            //    foreach (var img in vm.SubImages)
            //    {
            //        string subFile = Guid.NewGuid() + Path.GetExtension(img.FileName);
            //        string subPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//Images//Movies", subFile);

            //        using var stream = System.IO.File.Create(subPath);
            //        img.CopyTo(stream);

            //        await _MovieImageRepo.CreateAsync(new MovieImage
            //        {
            //            MovieId = movie.Id,
            //            ImageUrl = subFile,
            //            Order = 1
            //        });
            //    }

            //    await _MovieImageRepo.CommitAsync();
            //}

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var Movie = await _movieRepository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);

            if (Movie is null)
                return RedirectToAction("NotFoundPage", "Home");

            return View(Movie);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Movie Movie, IFormFile file, CancellationToken cancellationToken)
        {
            var MovieInDB = await _movieRepository.GetOneAsync(e => e.Id == Movie.Id, tracked: false, cancellationToken: cancellationToken);

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

            _movieRepository.Update(Movie);
            await _movieRepository.CommitAsync(cancellationToken);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var Movie = await _movieRepository.GetOneAsync(e => e.Id == id);

            if (Movie is null)
                return RedirectToAction("NotFoundPage", "Home");

            _movieRepository.Delete(Movie);
            await _movieRepository.CommitAsync(cancellationToken);

            return RedirectToAction(nameof(Index));
        }
    }
}