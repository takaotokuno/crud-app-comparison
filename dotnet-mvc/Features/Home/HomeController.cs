using System.Diagnostics;
using DotnetMvc.Features.Home.Models;
using DotnetMvc.Features.Home.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotnetMvc.Features.Home;

public sealed class HomeController(IHomeService homeService) : Controller
{
    public IActionResult Index()
    {
        return View(homeService.GetIndexViewModel());
    }

    public IActionResult Error()
    {
        return View(new ErrorViewModel(Activity.Current?.Id ?? HttpContext.TraceIdentifier));
    }
}
