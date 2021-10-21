using AutoMapper;
using DevIO.API.DTO;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using DevIO.Business.Notificacoes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.API.Controllers
{
   [ApiController]
   [Route("[controller]")]
   public abstract class MainController : ControllerBase
   {


      private readonly INotificador _notificador;
      public readonly IUser AppUser;
      private INotificador notificador;

      protected Guid UsuarioId { get; set; }
      protected bool UsuarioAutenticado { get; set; }

      protected MainController(INotificador notificador, IUser appUser)
      {
         _notificador = notificador;
         AppUser = appUser;

         if(appUser.IsAuthenticated())
         {
            UsuarioId = appUser.GetUserId();
            UsuarioAutenticado = true;
         }

      }

      protected bool OperacaoValida()
      {
         return !_notificador.TemNotificacao();
      }

      protected ActionResult CustomResponse(object result = null)
      {
         if(OperacaoValida())
         {
            return Ok(new 
            { 
               success = true,
               data = result,
            });
         } 
         else
         {
            return BadRequest(new
            {
               success = false,
               errors = _notificador.ObterNotificacoes().Select(x => x.Mensagem)
            });
         }
      }

      protected ActionResult CustomResponse(ModelStateDictionary modelState)
      {
         if(!modelState.IsValid)
            NotificarErroModelInvalida(modelState);
         return CustomResponse();
      }

      protected void NotificarErroModelInvalida(ModelStateDictionary model)
      {
         List<ModelError> erros = model.Values.SelectMany(e => e.Errors).ToList();
         erros.ForEach(e =>
         {
            string errorMsg = e.Exception == null ? e.ErrorMessage : e.Exception.Message;
            NotificarErro(errorMsg);
         });
      }

      protected void NotificarErro(string mensagem)
      {
         _notificador.Handle(new Notificacao(mensagem));
      }


   }
}
