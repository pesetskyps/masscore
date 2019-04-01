
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MasstransitDemo.Models;
using MassTransit;

namespace MasstransitDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBus _bus;
        public HomeController(IBus bus)
        {
            _bus = bus;
        }
        
        public async Task<IActionResult> Index()
        {
            await _bus.Publish<Message>(
                new
                {
                    Value = "123"
                });

            return View();
        }
    }
}
