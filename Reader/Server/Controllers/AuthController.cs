using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Reader.DataAccess;
using Reader.Domain;
using Reader.Services;
using Reader.Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Reader.Server.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        private readonly UserManager<AppUser> _userManager;
        private readonly UserRepo _userRepo;

        public AuthController(
                IConfiguration config,
                IEmailService emailService,
                UserManager<AppUser> userManager,
                UserRepo userRepo)
        {
            _config = config;
            _emailService = emailService;
            _userManager = userManager;
            _userRepo = userRepo;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register(AuthDto userToRegister)
        {
            try
            {
                if (string.IsNullOrEmpty(userToRegister.Email))
                    return BadRequest(new AuthResponseDto
                    {
                        IsAuthSuccessful = false,
                        ErrorMessage = "Email is required.",
                        Token = null
                    });

                var user = await _userManager.FindByEmailAsync(userToRegister.Email);

                if (user != null)
                    return BadRequest(new AuthResponseDto
                    {
                        IsAuthSuccessful = false,
                        ErrorMessage = "Email is taken, please try with a different email address!",
                        Token = null
                    });

                if (!userToRegister.TermsAgreedTo)
                    return BadRequest(new AuthResponseDto
                    {
                        IsAuthSuccessful = false,
                        ErrorMessage = "Terms and Conditions are required.",
                        Token = null
                    });

                user = new AppUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = userToRegister.Email.Trim(),
                    UserName = userToRegister.Email.Trim(),
                    TermsAndConditionsAgreedTo = userToRegister.TermsAgreedTo,
                    TermsAndConditionsAgreedToOn = DateTime.Now,
                    PasswordLastChanged = DateTime.Now,
                    JoinedOn = DateTime.Now
                };

                var userCreateResult = await _userManager.CreateAsync(user, userToRegister.Password);
                if (userCreateResult.Succeeded)
                {
                    var signingCredentials = GetSigningCredentials();
                    var claims = await GetClaims(user);
                    var tokenOptions = GenerateTokenOptions(signingCredentials, claims);
                    var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    StringBuilder urlBuilder = new StringBuilder();

                    urlBuilder.Append("https://rentme.com/account/confirmemail?");
                    urlBuilder.Append($"userId={user.Id}&code={code}");
                    var callbackUrl = urlBuilder.ToString();

                    await _emailService.SendEmailAsync(
                        fromEmail: "noreply@rentme.com",
                        toEmail: user.Email,
                        subject: "Please confirm your e-mail address",
                        message: $"Hi. To get up and running, you’ll just need to click <a class='btn btn-primary btn-lg' href='{callbackUrl}'>here</a> to confirm your email address.");

                    return Ok(new AuthResponseDto
                    {
                        IsAuthSuccessful = true,
                        Token = token
                    });
                }

                return BadRequest(new AuthResponseDto
                {
                    IsAuthSuccessful = false,
                    Token = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] AuthDto userLoginResource)
        {
            var user = _userManager.Users.SingleOrDefault(u => u.UserName == userLoginResource.Email.Trim());
            if (user is null)
                return NotFound(new AuthResponseDto
                {
                    IsAuthSuccessful = false,
                    ErrorMessage = "User not found"
                });

            var userSigninResult = await _userManager.CheckPasswordAsync(user, userLoginResource.Password);
            if (userSigninResult)
            {
                var signingCredentials = GetSigningCredentials();
                var claims = await GetClaims(user);
                var tokenOptions = GenerateTokenOptions(signingCredentials, claims);
                var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

                return Ok(new AuthResponseDto
                {
                    IsAuthSuccessful = true,
                    Token = token
                });
            }

            return BadRequest(new AuthResponseDto
            {
                IsAuthSuccessful = false,
                ErrorMessage = "Email or password is incorrect."
            });
        }

        [HttpPost("ForgotPassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest("Email is required");

            var user = await _userManager.FindByEmailAsync(email.Trim());

            if (user is null) return NotFound("User not found");

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            StringBuilder urlBuilder = new StringBuilder();

            urlBuilder.Append($"https://rentme.com/account/ResetPassword?code={code}");
            var callbackUrl = urlBuilder.ToString();

            try
            {
                await _emailService.SendEmailAsync(
                           fromEmail: "noreply@rentme.com",
                           toEmail: user.Email,
                           subject: "Forgot your password? We can help.",
                           message: $"Hi, <br /> Did you forget your password? No worries. We've got you covered. Click the link to reset your password <a href='{callbackUrl}'> here</a>. <br /> Happy writing!");

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("ResendEmailConfirmation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResendEmailConfirmation(string email)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest("Email is required");

            var user = await _userManager.FindByEmailAsync(email.Trim());

            if (user is null) return NotFound("User not found");

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            StringBuilder urlBuilder = new StringBuilder();

            urlBuilder.Append($"https://rentme.com/account/confirmemail?userId={user.Id}&code={code}");
            var callbackUrl = urlBuilder.ToString();

            await _emailService.SendEmailAsync(
                fromEmail: "noreply@rentme.com",
                toEmail: user.Email,
                subject: "Confirm your email",
                message: $"Please confirm your account by clicking <a href='{callbackUrl}'>here</a>.");

            return Ok();
        }

        [HttpPost("ResetPassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordModel)
        {
            if (string.IsNullOrEmpty(resetPasswordModel.Email)) return BadRequest("Email is required");
            if (string.IsNullOrEmpty(resetPasswordModel.Password)) return BadRequest("Password is missing.");
            if (string.IsNullOrEmpty(resetPasswordModel.Code)) return BadRequest("Code is missing");
            if (!string.Equals(resetPasswordModel.Password, resetPasswordModel.ConfirmPassword)) return BadRequest("Password's do not match.");

            var user = await _userManager.FindByEmailAsync(resetPasswordModel.Email.Trim());
            if (user is null) return NotFound("User not found");

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetPasswordModel.Code));
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, resetPasswordModel.Password);

            if (result.Succeeded)
            {
                await _emailService.SendEmailAsync(
                     fromEmail: "noreply@rentme.com",
                     toEmail: user.Email,
                     subject: "Password Change",
                     message: $"Hey. We're just confirming that you recently changed your password. If you updated your password, you can ignore this message. If your password was changed without your permission, please contact us immediately.");

                user.PasswordLastChanged = DateTime.Now;
                await _userRepo.UpdateOneAsync(user);
            }

            return result.Succeeded ? Ok() : BadRequest();
        }

        [HttpPost("ConfirmEmail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (string.IsNullOrEmpty(code)) return BadRequest("Code is required");
            if (string.IsNullOrEmpty(userId)) return BadRequest("UserId is required");

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user is null) return NotFound();

                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                var result = await _userManager.ConfirmEmailAsync(user, code);

                return result.Succeeded ? Ok() : BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        private SigningCredentials GetSigningCredentials()
        {
            var jwtSettings = _config.GetSection("JWTSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["securityKey"]);
            var secret = new SymmetricSecurityKey(key);

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        private async Task<List<Claim>> GetClaims(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            //UserImage image = await _userImageRepo.GetOneAsync(filter: x => x.UserId == user.Id);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }

        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var jwtSettings = _config.GetSection("JWTSettings");
            var tokenOptions = new JwtSecurityToken(
                issuer: jwtSettings["validIssuer"],
                audience: jwtSettings["validAudience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["expiryInMinutes"])),
                signingCredentials: signingCredentials);

            return tokenOptions;
        }
    }
}
