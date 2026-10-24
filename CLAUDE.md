# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

ASP.NET Core Web API (.NET 7, `net7.0`) backend for a travel app. The companion Angular frontend lives in the sibling directory `../Traveller_frontend`. The backend is currently the stock `dotnet new webapi` template — the only endpoint is the sample `WeatherForecastController` (`GET /WeatherForecast`).

## Commands

Run from this directory (contains `Traveller_backend.sln` / `Traveller_backend.csproj`):

- Build: `dotnet build`
- Run (HTTP, http://localhost:5091): `dotnet run --launch-profile http`
- Run (HTTPS, https://localhost:7170 + http://localhost:5091): `dotnet run --launch-profile https`
- Hot reload: `dotnet watch run`
- Swagger UI (Development only): `/swagger`

There is no test project yet. If one is added (e.g. `dotnet new xunit`), run tests with `dotnet test` and a single test with `dotnet test --filter "FullyQualifiedName~<TestName>"`.

## Architecture

- `Program.cs` — minimal-hosting entry point: registers controllers, `AddEndpointsApiExplorer`, and Swashbuckle (`AddSwaggerGen`); Swagger middleware is enabled only when `ASPNETCORE_ENVIRONMENT=Development`. Pipeline: HTTPS redirection → authorization (no auth scheme configured yet) → `MapControllers`.
- `Controllers/` — attribute-routed `[ApiController]` classes using `[Route("[controller]")]`; dependencies come in via constructor injection.
- Models currently sit in the project root (e.g. `WeatherForecast.cs`), namespace `Traveller_backend`.
- `Nullable` and `ImplicitUsings` are enabled in the csproj.
- Config: `appsettings.json` / `appsettings.Development.json` (logging only). Launch profiles in `Properties/launchSettings.json`.
- No CORS policy is configured yet — the Angular frontend (dev server typically on http://localhost:4200) will need one added in `Program.cs` before it can call this API from the browser.
