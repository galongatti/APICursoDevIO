using DevIO.Business.Intefaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DevIO.API.Extensions
{
   public class AspNetUser : IUser
   {
      private readonly IHttpContextAccessor _accessor;

      public AspNetUser(IHttpContextAccessor accessor) => _accessor = accessor;

      public string Name => _accessor.HttpContext.User.Identity.Name;

      public IEnumerable<Claim> GetClaimsIdentity()
      {
         throw new NotImplementedException();
      }

      public string GetUserEmail()
      {
         throw new NotImplementedException();
      }

      public Guid GetUserId()
      {
         return IsAuthenticated() ? Guid.Parse(_accessor.HttpContext.User.GetUserId()) : Guid.NewGuid();
      }

      public bool IsAuthenticated()
      {
         throw new NotImplementedException();
      }

      public bool IsInRole(string role)
      {
         throw new NotImplementedException();
      }
   }

   public static class ClaimPrincipalExtension
   {
      public static string GetUserId(this ClaimsPrincipal principal)
      {
         if(principal == null)
         {
            throw new ArgumentException(nameof(principal));
         }

         var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
         return claim?.Value;
      }

      public static string GetUserEmail(this ClaimsPrincipal principal)
      {
         if (principal == null)
         {
            throw new ArgumentException(nameof(principal));
         }

         var claim = principal.FindFirst(ClaimTypes.Email);
         return claim?.Value;
      }




   }
}
