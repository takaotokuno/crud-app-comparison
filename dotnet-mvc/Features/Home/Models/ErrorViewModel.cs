namespace DotnetMvc.Features.Home.Models;

public sealed record ErrorViewModel(string? RequestId)
{
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
