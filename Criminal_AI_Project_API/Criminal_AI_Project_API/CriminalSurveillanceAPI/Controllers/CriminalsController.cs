using Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.DTOs;
using Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.Entities;
using Criminal_AI_Project_API.CriminalSurveillanceAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Criminal_AI_Project_API.CriminalSurveillanceAPI.Services.Interfaces;
using System.IO;
using System.Linq;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class CriminalsController : ControllerBase
    {
    private readonly IConfiguration _config;
    private readonly IAdminService _adminService;
    private readonly Criminal_AI_Project_API.CriminalSurveillanceAPI.Services.Interfaces.ICriminalsService _criminalsService;
    private readonly Criminal_AI_Project_API.CriminalSurveillanceAPI.Services.Interfaces.ITrainingService _trainingService;
    private readonly Criminal_AI_Project_API.CriminalSurveillanceAPI.Services.Interfaces.IEventService _eventService;
    private readonly Criminal_AI_Project_API.CriminalSurveillanceAPI.Services.Interfaces.IDashboardService _dashboardService;

        public CriminalsController(IAdminService adminService, IConfiguration config, Criminal_AI_Project_API.CriminalSurveillanceAPI.Services.Interfaces.ICriminalsService criminalsService, Criminal_AI_Project_API.CriminalSurveillanceAPI.Services.Interfaces.ITrainingService trainingService, Criminal_AI_Project_API.CriminalSurveillanceAPI.Services.Interfaces.IEventService eventService, Criminal_AI_Project_API.CriminalSurveillanceAPI.Services.Interfaces.IDashboardService dashboardService)
        {
            _adminService = adminService;
            _config = config;
            _criminalsService = criminalsService;
            _trainingService = trainingService;
            _eventService = eventService;
            _dashboardService = dashboardService;
        }

    [HttpPost("AdminLogin")]
    [AllowAnonymous]
    public async Task<IActionResult> AdminLogin([FromBody] LoginDTO loginDto)
        {
            try
            {
                if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
                {
                    Log.Information("Invalid login request payload");
                    return BadRequest(new { message = "Email and Password are required." });
                }

                var admin = await _adminService.AuthenticateAsync(loginDto.Email, loginDto.Password).ConfigureAwait(false);
                if (admin == null)
                {
                    Log.Information($"Login failed: invalid credentials for email {loginDto.Email}");
                    return Unauthorized(new { message = "Invalid credentials." });
                }

                // Create JWT token
                var jwtKey = _config["JwtSettings:Key"] ?? "fallback_super_secret_key_please_change";
                var issuer = _config["JwtSettings:Issuer"] ?? "CriminalAI";
                var audience = _config["JwtSettings:Audience"] ?? "CriminalAIUsers";
                var expireMinutes = int.TryParse(_config["JwtSettings:ExpireMinutes"], out var em) ? em : 60;

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(jwtKey);
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, admin.admin_id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, admin.email ?? string.Empty),
                    new Claim("username", admin.username ?? string.Empty),
                    // Ensure the token contains a role claim so [Authorize(Roles = "Admin")] works
                    new Claim(System.Security.Claims.ClaimTypes.Role, "Admin")
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                Log.Information($"Admin {admin.email} logged in successfully");

                return Ok(new
                {
                    token = tokenString,
                    expires = tokenDescriptor.Expires,
                    admin = new { admin.admin_id, admin.username, admin.email }
                });
            }
            catch (Exception ex)
            {
                Log.Information($"Exception thrown while login {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while processing the request." });
            }
        }

        [HttpGet("GetCriminals")]
        public async Task<IActionResult> GetCriminals([FromQuery] Models.DTOs.SieveModel sieve)
        {
            Log.Logger.Information("Fetching list of criminals with sieve parameters");
            var result = await _criminalsService.GetAllAsync(sieve).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var dashboard = await _dashboardService.GetDashboardAsync().ConfigureAwait(false);
            return Ok(dashboard);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCriminal([FromBody] CriminalCreateDTO model)
        {
            if (model == null)
            {
                return BadRequest(new { message = "Criminal data is required" });
            }

            var created = await _criminalsService.CreateAsync(model).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetCriminalByGuid), new { guid = created.Guid }, created);
        }

        [HttpGet("{guid}")]
        public async Task<IActionResult> GetCriminalByGuid([FromRoute] Guid guid)
        {
            var crim = await _criminalsService.GetByGuidAsync(guid).ConfigureAwait(false);
            if (crim == null) return NotFound();
            return Ok(crim);
        }

        [HttpDelete("{guid}")]
        public async Task<IActionResult> DeleteCriminal([FromRoute] Guid guid)
        {
            var ok = await _criminalsService.DeleteByGuidAsync(guid).ConfigureAwait(false);
            if (!ok) return NotFound(new { message = "Criminal not found" });
            return NoContent();
        }

        [HttpPut("{guid}")]
        public async Task<IActionResult> UpdateCriminal([FromRoute] Guid guid, [FromBody] CriminalUpdateDTO dto)
        {
            if (dto == null) return BadRequest(new { message = "Update data is required" });
            if (dto.Guid != guid) return BadRequest(new { message = "GUID mismatch" });

            var updated = await _criminalsService.UpdateAsync(dto).ConfigureAwait(false);
            if (updated == null) return NotFound(new { message = "Criminal not found" });
            return Ok(updated);
        }

        // Training endpoints moved into CriminalsController per request
        [HttpPost("training")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateTraining([FromBody] AiTrainingCreateDTO dto)
        {
            if (dto == null) return BadRequest(new { message = "Training data is required" });
            var created = await _trainingService.CreateAsync(dto).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetTrainingByGuid), new { guid = created.Guid }, created);
        }
        [HttpGet("training/{guid}")]
        [AllowAnonymous]
        public IActionResult GetTrainingByGuid([FromRoute] Guid guid)
        {
            return NoContent();
        }

        [HttpPost("events")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateEvent([FromBody] CriminalEventCreateDTO dto)
        {
            if (dto == null) return BadRequest(new { message = "Event data is required" });
            var created = await _eventService.CreateAsync(dto).ConfigureAwait(false);
            return Created(string.Empty, created);
        }

        /// <summary>
        /// Returns detected images metadata stored under wwwroot/images/detected.
        /// If guid is provided, returns data only for that criminal GUID; otherwise returns all detected entries.
        /// </summary>
        [HttpGet("detected/{guid?}")]
        [AllowAnonymous]
        public IActionResult GetDetected([FromRoute] Guid? guid)
        {
            var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "detected");
            if (!Directory.Exists(webRoot))
            {
                return NotFound(new { message = "No detected images folder found." });
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}/images/detected";

            List<object> results = new List<object>();

            IEnumerable<string> guidDirs;
            if (guid.HasValue)
            {
                var specific = Path.Combine(webRoot, guid.Value.ToString());
                if (!Directory.Exists(specific)) return NotFound(new { message = "No detected data for the provided GUID." });
                guidDirs = new[] { specific };
            }
            else
            {
                guidDirs = Directory.GetDirectories(webRoot);
            }

            foreach (var gdir in guidDirs)
            {
                var guidFolderName = Path.GetFileName(gdir);
                var sessions = new List<object>();

                foreach (var sessionDir in Directory.GetDirectories(gdir))
                {
                    var sessionName = Path.GetFileName(sessionDir);
                    var files = Directory.GetFiles(sessionDir)
                        .Select(f => new FileInfo(f))
                        .Select(fi => new
                        {
                            fileName = fi.Name,
                            url = $"{baseUrl}/{Uri.EscapeDataString(guidFolderName)}/{Uri.EscapeDataString(sessionName)}/{Uri.EscapeDataString(fi.Name)}",
                            size = fi.Length,
                            lastModified = fi.LastWriteTimeUtc
                        }).ToList();

                    sessions.Add(new { session = sessionName, files });
                }

                // also include any files directly under the guid folder (if present)
                var rootFiles = Directory.GetFiles(gdir)
                    .Select(f => new FileInfo(f))
                    .Select(fi => new
                    {
                        fileName = fi.Name,
                        url = $"{baseUrl}/{Uri.EscapeDataString(guidFolderName)}/{Uri.EscapeDataString(fi.Name)}",
                        size = fi.Length,
                        lastModified = fi.LastWriteTimeUtc
                    }).ToList();

                results.Add(new
                {
                    guid = guidFolderName,
                    sessions,
                    files = rootFiles
                });
            }

            return Ok(results);
        }
    }
}
