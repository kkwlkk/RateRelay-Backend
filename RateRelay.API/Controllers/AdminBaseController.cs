using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateRelay.API.Attributes.Auth;
using RateRelay.Domain.Interfaces;

namespace RateRelay.API.Controllers;

[ApiController]
[Route("api/admin")]
[Area("Admin")]
[Authorize]
[RequireAdmin]
public class AdminBaseController : BaseController;