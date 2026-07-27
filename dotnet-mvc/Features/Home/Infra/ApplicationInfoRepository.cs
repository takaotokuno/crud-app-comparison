namespace DotnetMvc.Features.Home.Infra;

public sealed class ApplicationInfoRepository : IApplicationInfoRepository
{
    public ApplicationInfo Get()
    {
        return new ApplicationInfo(
            "商品在庫管理",
            "ASP.NET Core MVC の Feature Folder 構成で実装する CRUD アプリケーションです。");
    }
}
