FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY AspNetNextApp.sln ./
COPY AspNetNextApp.Api/AspNetNextApp.Api.csproj ./AspNetNextApp.Api/

RUN dotnet restore ./AspNetNextApp.sln

COPY . .

RUN dotnet publish ./AspNetNextApp.Api/AspNetNextApp.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "AspNetNextApp.Api.dll"]