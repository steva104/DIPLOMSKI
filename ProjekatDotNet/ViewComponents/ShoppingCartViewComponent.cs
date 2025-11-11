using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VinylVibe.DataAccess.Repository.IRepository;

namespace VinylVibeWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        
        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var userId = identity.FindFirst(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                if (HttpContext.Session.GetInt32("SessionCart") == null)
                {
                    HttpContext.Session.SetInt32("SessionCart", _unitOfWork.ShoppingCart.GetAll(x => x.UserId == userId.Value).Count());
                }             
                return View(HttpContext.Session.GetInt32("SessionCart"));
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }

        }
    }
}
