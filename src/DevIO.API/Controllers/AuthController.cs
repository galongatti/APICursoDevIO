using DevIO.API.DTO;
using DevIO.API.Extensions;
using DevIO.Business.Intefaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DevIO.API.Controllers
{
   [Route("api/conta")]
   public class AuthController : MainController
   {

      private readonly SignInManager<IdentityUser> _signInManager;
      private readonly UserManager<IdentityUser> _userManager;
      private readonly AppSettings _appSettings;
      private readonly ILogger _logger;

      public AuthController(INotificador notificador, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IOptions<AppSettings> appSettings, IUser user, ILogger<AuthController> logger) : base(notificador, user)
      {
         _signInManager = signInManager;
         _userManager = userManager;
         _appSettings = appSettings.Value;
         _logger = logger;
      }

      [HttpPost("CadastrarUsuario")]
      public async Task<ActionResult> CadastrarUsuario(RegisterUserDTO register)
      {

         if(!ModelState.IsValid)
         {
            CustomResponse(ModelState);
         }

         IdentityUser user = new IdentityUser()
         {
            UserName = register.Email,
            Email = register.Email,
            EmailConfirmed = true,
         };

         // DEFINE CLAIMS
         List<Claim> claims = new List<Claim>
         {
            new Claim("Fornecedor", "Cadastrar"),
            new Claim("Fornecedor", "Atualizar"),
            new Claim("Fornecedor", "Excluir"),
            new Claim("Fornecedor", "Consultar"),
         };


         IdentityResult result = await _userManager.CreateAsync(user, register.Password);

         if(result.Succeeded)
         {
            await _userManager.AddClaimsAsync(user, claims);
            await _signInManager.SignInAsync(user, false);
            return CustomResponse(register);
         }

         foreach(IdentityError error in result.Errors)
         {
            NotificarErro(error.Description);
         }



         return CustomResponse(register);
      }

      [HttpPost("Login")]
      public async Task<ActionResult> Login(LoginUserDTO login)
      {
         if (!ModelState.IsValid)
         {
            CustomResponse(ModelState);
         }

         Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(login.Email,login.Password, false, true);

         if(result.Succeeded)
         {
            return CustomResponse(await GerarJWT(login.Email));
         }

         if(result.IsLockedOut)
         {
            NotificarErro("Usuário temporariamente bloqueado por tentativas inválidas");
            return CustomResponse(login);
         }

         NotificarErro("Usuário ou senha incorretos");
         return CustomResponse(login);
      }

      private async Task<LoginResponseDTO> GerarJWT(string email)
      {

         IdentityUser user = await _userManager.FindByEmailAsync(email);
         IList<Claim> claims = await _userManager.GetClaimsAsync(user);
         IList<string> userRoles = await _userManager.GetRolesAsync(user);

         claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
         claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
         claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
         claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString()));
         claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(),ClaimValueTypes.Integer64));

         foreach(string userRole in userRoles)
         {
            claims.Add(new Claim("role", userRole));
         }

         ClaimsIdentity identityClaims = new ClaimsIdentity();
         identityClaims.AddClaims(claims);


         JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
         byte[] key = Encoding.ASCII.GetBytes(_appSettings.Secret);
         SecurityToken token = tokenHandler.CreateToken(new SecurityTokenDescriptor
         { 
            Issuer = _appSettings.Emissor,
            Audience = _appSettings.ValidoEm,
            Expires = DateTime.UtcNow.AddHours(_appSettings.ExpiracaoHoras),
            SigningCredentials = new SigningCredentials( new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
            Subject = identityClaims
         });

         string encondedToken = tokenHandler.WriteToken(token);
         LoginResponseDTO response = new LoginResponseDTO()
         { 
            AccessToken = encondedToken,
            ExpiresIn = TimeSpan.FromHours(_appSettings.ExpiracaoHoras).TotalSeconds,
            UserToken = new TokenUserDTO
            { 
               Id = user.Id,
               Email = user.Email,
               Claims = claims.Select(c => new ClaimDTO { Type = c.Type, Value = c.Value})
            }
         };

         return response;

      }

      private static long ToUnixEpochDate(DateTime date)
      {
         return (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
      }
   }
}
