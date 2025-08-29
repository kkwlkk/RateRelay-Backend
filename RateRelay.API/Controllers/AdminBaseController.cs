using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateRelay.API.Attributes.Auth;
using RateRelay.API.Filters;
using RateRelay.Domain.Interfaces;

namespace RateRelay.API.Controllers;

[ApiController]
[Route("api/admin")]
[Area("Admin")]
[Authorize]
[RequireAdmin]
[DisableDuringMaintenance]
public class AdminBaseController : BaseController;