using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RateRelay.API.Controllers;

[ApiController]
[Route("api/user/[controller]")]
[Area("User")]
[Authorize]
public class UserBaseController : BaseController;