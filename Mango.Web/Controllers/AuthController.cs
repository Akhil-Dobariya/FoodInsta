using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Mango.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;

        public AuthController(IAuthService authService,ITokenProvider tokenProvider)
        {
            _authService = authService;    
            _tokenProvider = tokenProvider;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginRequestDTO());
        }

        [HttpGet]
        public IActionResult Register()
        {
            var roleList = new List<SelectListItem>()
            {
                new SelectListItem{Text=StaticDetails.RoleAdmin, Value=StaticDetails.RoleAdmin},
                new SelectListItem{Text=StaticDetails.RoleCustomer, Value=StaticDetails.RoleCustomer}
            };

            ViewBag.RoleList = roleList;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDTO loginRequest)
        {
            ResponseDto responseDto = await _authService.LoginAsync(loginRequest);

            if (responseDto != null && responseDto.IsSuccess)
            {
                LoginResponseDTO loginResponseDTO = JsonConvert.DeserializeObject<LoginResponseDTO>(Convert.ToString(responseDto.Result));

                await SignInUser(loginResponseDTO);

                _tokenProvider.SetToken(loginResponseDTO.Token);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["error"] = responseDto?.Message;
                return View(loginRequest);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegistrationRequestDTO registrationRequest)
        {
            ResponseDto responseDto = await _authService.RegisterAsync(registrationRequest);
            ResponseDto assignRole;

            if (responseDto != null && responseDto.IsSuccess) 
            {
                if (string.IsNullOrEmpty(registrationRequest.RoleName))
                {
                    registrationRequest.RoleName = StaticDetails.RoleCustomer;
                }

                assignRole = await _authService.AssignRoleAsync(registrationRequest);

                if (assignRole != null && assignRole.IsSuccess)
                {
                    TempData["success"] = "Registration Successful";
                    return RedirectToAction(nameof(Login));
                }
            }
            else
            {
                TempData["error"] = responseDto?.Message;
            }

            var roleList = new List<SelectListItem>()
            {
                new SelectListItem{Text=StaticDetails.RoleAdmin, Value=StaticDetails.RoleAdmin},
                new SelectListItem{Text=StaticDetails.RoleCustomer, Value=StaticDetails.RoleCustomer}
            };

            ViewBag.RoleList = roleList;
            return View(registrationRequest);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            _tokenProvider.ClearToken();
            return RedirectToAction("Index","Home");
        }

        private async Task SignInUser(LoginResponseDTO model)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(model.Token);

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Email, jwt.Claims.FirstOrDefault(t => t.Type == JwtRegisteredClaimNames.Email).Value));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub, jwt.Claims.FirstOrDefault(t => t.Type == JwtRegisteredClaimNames.Sub).Value));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Name, jwt.Claims.FirstOrDefault(t => t.Type == JwtRegisteredClaimNames.Name).Value));

            identity.AddClaim(new Claim(ClaimTypes.Name, jwt.Claims.FirstOrDefault(t => t.Type == JwtRegisteredClaimNames.Email).Value));
            identity.AddClaim(new Claim(ClaimTypes.Role, jwt.Claims.FirstOrDefault(t => t.Type == "role").Value));

            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,principal);
        }
    }
}
