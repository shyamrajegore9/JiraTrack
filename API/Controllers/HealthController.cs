using JiraTrack.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JiraTrack.Controllers;

[ApiController]
[Route("health")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() =>
        Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });

    [HttpGet("ready")]
    public async Task<IActionResult> Ready([FromServices] ApplicationDbContext db, CancellationToken cancellationToken)
    {
        try
        {
            var canConnect = await db.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
                return StatusCode(503, new { status = "NotReady", database = "Unavailable" });

            return Ok(new { status = "Ready", database = "Connected", timestamp = DateTime.UtcNow });
        }
        catch
        {
            return StatusCode(503, new { status = "NotReady", database = "Error" });
        }
    }
}
