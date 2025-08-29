using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateRelay.API.Filters;

namespace RateRelay.API.Controllers;

[ApiController]
[Route("api/user/[controller]")]
[Area("User")]
[Authorize]
[DisableDuringMaintenance]
public class UserBaseController : BaseController;