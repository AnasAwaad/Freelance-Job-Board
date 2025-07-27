using FreelanceJobBoard.Application.Features.Auth.Commands.ClientRegister;
using FreelanceJobBoard.Application.Features.Auth.Commands.FreelancerRegister;
using FreelanceJobBoard.Application.Features.Auth.Commands.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IMediator mediator, ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("client-register")]
        public async Task<IActionResult> ClientRegister([FromBody] ClientRegisterCommand command)
        {
            try
            {
                await _mediator.Send(command);
                _logger.LogInformation("User registered successfully: {Email}", command.Email);
                return Ok(new { success = true, message = "Registration successful" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Registration failed for {Email}: {Error}", command.Email, ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error for {Email}", command.Email);
                return StatusCode(500, new { success = false, message = "An error occurred during registration" });
            }
        }
        [HttpPost("Register")]
        public async Task<IActionResult> FreelancerRegister([FromBody] FreelancerRegisterCommand command)
        {
            try
            {
                await _mediator.Send(command);
                _logger.LogInformation("User registered successfully: {Email}", command.Email);
                return Ok(new { success = true, message = "Registration successful" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Registration failed for {Email}: {Error}", command.Email, ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error for {Email}", command.Email);
                return StatusCode(500, new { success = false, message = "An error occurred during registration" });
            }
        }

        /// <summary>
        /// Login user
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var command = new LoginCommand { LoginDto = loginDto };
                var result = await _mediator.Send(command);

                _logger.LogInformation("User logged in successfully: {Email}", loginDto.Email);
                return Ok(new { success = true, data = result, message = "Login successful" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Login failed for {Email}: {Error}", loginDto.Email, ex.Message);
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for {Email}", loginDto.Email);
                return StatusCode(500, new { success = false, message = "An error occurred during login" });
            }
        }

        /// <summary>
        /// Logout user (JWT token invalidation is handled client-side)
        /// </summary>
        //[HttpPost("logout")]
        //public IActionResult Logout()
        //{
        //    // For JWT, logout is typically handled client-side by removing the token
        //    // You could implement token blacklisting here if needed
        //    _logger.LogInformation("User logout requested");
        //    return Ok(new { success = true, message = "Logged out successfully" });
        //}
    }
}