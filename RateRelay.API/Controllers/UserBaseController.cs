using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateRelay.Domain.Interfaces;

namespace RateRelay.API.Controllers;

[ApiController]
[Route("api/user/[controller]")]
[Area("User")]
[Authorize]
public class UserBaseController : BaseController;