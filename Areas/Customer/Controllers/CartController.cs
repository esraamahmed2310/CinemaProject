using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace CinemaProject.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<Movie> _MovieRepository;
        private readonly IRepository<Promotion> _promotionRepository;

        public CartController(UserManager<ApplicationUser> userManager, IRepository<Cart> cartRepository, IRepository<Movie> MovieRepository, IRepository<Promotion> promotionRepository)
        {
            _userManager = userManager;
            _cartRepository = cartRepository;
            _MovieRepository = MovieRepository;
            _promotionRepository = promotionRepository;
        }

        public async Task<IActionResult> Index(string? code = null)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null) return NotFound();

            var MoviesInCart = await _cartRepository.GetAsync(e => e.ApplicationUserId == user.Id, includes: [e => e.Movie]);

            if (code is not null)
            {
                var promotion = await _promotionRepository.GetOneAsync(e => e.Code == code);

                if (promotion is not null)
                {
                    var MovieInCart = MoviesInCart.FirstOrDefault(e => e.MovieId == promotion.MovieId);

                    if (MovieInCart is not null)
                    {
                        if (promotion.IsValid && promotion.ValidTo > DateTime.UtcNow && promotion.MaxUsage > 0)
                        {
                            MovieInCart.Price -= MovieInCart.Price * (promotion.Discount / 100);
                            promotion.MaxUsage -= 1;
                            await _cartRepository.CommitAsync();
                            TempData["success-notification"] = "Applying Code Successfully";
                        }
                    }
                    else
                    {
                        TempData["error-notification"] = "Invalid Or Expired promotion";
                    }
                }
                else
                {
                    TempData["error-notification"] = "Invalid Or Expired promotion";
                }
            }

            return View(MoviesInCart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int MovieId, int count, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null) return NotFound();

            var MovieInDb = await _MovieRepository.GetOneAsync(e => e.Id == MovieId);

            if (MovieInDb is null) return NotFound();

            var cart = await _cartRepository.GetOneAsync(e => e.MovieId == MovieId && e.ApplicationUserId == user.Id);

            if (cart is not null)
            {
                cart.Count += count;
            }
            else
            {
                await _cartRepository.CreateAsync(new()
                {
                    ApplicationUserId = user.Id,
                    MovieId = MovieId,
                    Count = count,
                    Price = MovieInDb.Price - MovieInDb.Price * (MovieInDb.Discount / 100)
                }, cancellationToken: cancellationToken);
            }

            await _cartRepository.CommitAsync(cancellationToken);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> IncremntCount(int MovieId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null) return NotFound();

            var cart = await _cartRepository.GetOneAsync(e => e.MovieId == MovieId && e.ApplicationUserId == user.Id);

            if (cart is null) return NotFound();

            cart.Count += 1;
            await _cartRepository.CommitAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DecremntCount(int MovieId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null) return NotFound();

            var cart = await _cartRepository.GetOneAsync(e => e.MovieId == MovieId && e.ApplicationUserId == user.Id);

            if (cart is null) return NotFound();

            if (cart.Count > 1)
                cart.Count -= 1;
            await _cartRepository.CommitAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteMovie(int MovieId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null) return NotFound();

            var cart = await _cartRepository.GetOneAsync(e => e.MovieId == MovieId && e.ApplicationUserId == user.Id);

            if (cart is null) return NotFound();

            _cartRepository.Delete(cart);
            await _cartRepository.CommitAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Pay()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null) return NotFound();

            var cart = await _cartRepository.GetAsync(e => e.ApplicationUserId == user.Id, includes: [e => e.Movie]);

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/identity/checkout/success",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/identity/checkout/cancel",
            };

            foreach (var item in cart)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Movie.Name,
                            Description = item.Movie.Description,
                        },
                        UnitAmount = (long)item.Price * 100,
                    },
                    Quantity = item.Count,
                });
            }

            var service = new SessionService();
            var session = service.Create(options);
            return Redirect(session.Url);
        }
    }
}
