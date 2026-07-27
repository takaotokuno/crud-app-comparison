using DotnetMvc.Features.Home.Infra;
using DotnetMvc.Features.Home.Models;

namespace DotnetMvc.Features.Home.Services;

public sealed class HomeService(IApplicationInfoRepository repository) : IHomeService
{
    public HomeIndexViewModel GetIndexViewModel()
    {
        var applicationInfo = repository.Get();
        return new HomeIndexViewModel(applicationInfo.Name, applicationInfo.Description);
    }
}
