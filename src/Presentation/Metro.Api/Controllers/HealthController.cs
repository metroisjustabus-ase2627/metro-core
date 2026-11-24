using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Metro.Api.Health;

namespace Metro.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("health")]
[ApiExplorerSettings(GroupName = "ApiV1")]
[Tags("Health")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public HealthController(
        HealthCheckService healthCheckService,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        _healthCheckService = healthCheckService;
        _configuration = configuration;
        _environment = environment;
    }

    [HttpGet]
    [ProducesResponseType(typeof(HealthStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthStatusResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(cancellationToken: cancellationToken);
        var version = _configuration["APP_VERSION"]
            ?? _configuration["App:Version"]
            ?? Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? "dev";

        var response = new HealthStatusResponse
        {
            Status = report.Status.ToString(),
            Version = version,
            Environment = _environment.EnvironmentName,
            TotalDuration = report.TotalDuration.ToString(),
            Checks = report.Entries.Select(entry => new HealthCheckItemResponse
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                Description = entry.Value.Description,
                Duration = entry.Value.Duration.ToString()
            }).ToList()
        };

        var statusCode = report.Status == HealthStatus.Healthy
            ? StatusCodes.Status200OK
            : StatusCodes.Status503ServiceUnavailable;

        return new ObjectResult(response) { StatusCode = statusCode };
    }
}
