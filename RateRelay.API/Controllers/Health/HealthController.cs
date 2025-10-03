using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RateRelay.API.Controllers.Health;

[Microsoft.AspNetCore.Components.Route("api/[controller]")]
public class HealthController : BaseController
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetHealth()
    {
        return Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });
    }
}