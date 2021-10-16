using DevIO.API.DTO;
using DevIO.Business.Intefaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.API.Controllers
{
   [Route("api/conta")]
   public class AuthController : MainController
   {

      private readonly SignInManager<IdentityUser> _signInManager;
      private readonly UserManager<IdentityUser> _userManager;

      public AuthController(INotificador notificador, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager) : base(notificador)
      {
         _signInManager = signInManager;
         _userManager = userManager;
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

         IdentityResult result = await _userManager.CreateAsync(user, register.Password);

         if(result.Succeeded)
         {
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
            return CustomResponse(login);
         }

         if(result.IsLockedOut)
         {
            NotificarErro("Usuário temporariamente bloqueado por tentativas inválidas");
            return CustomResponse(login);
         }

         NotificarErro("Usuário ou senha incorretos");
         return CustomResponse(login);
      }

    }
}
