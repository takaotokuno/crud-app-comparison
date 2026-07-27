using DotnetMvc.Features.Home.Models;

namespace DotnetMvc.Features.Home.Services;

public interface IHomeService
{
    HomeIndexViewModel GetIndexViewModel();
}
