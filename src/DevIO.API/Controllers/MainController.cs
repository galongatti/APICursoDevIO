using AutoMapper;
using DevIO.API.DTO;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Mvc;
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
     
   }

   [Route("api/[controller]")]
   public class FornecedoresController: MainController
   {

      private readonly IFornecedorRepository _fornecedorRepository;
      private readonly IMapper _mapper;

      public FornecedoresController(IFornecedorRepository fornecedor, IMapper mapper)
      {
         _fornecedorRepository = fornecedor;
         _mapper = mapper;
      }

      [HttpGet]
      public async Task<ActionResult<List<FornecedorDTO>>> GetFornecedores()
      {
         try
         {
            List<FornecedorDTO> fornecedores = _mapper.Map<List<FornecedorDTO>>(await _fornecedorRepository.ObterTodos());

            return Ok(fornecedores);
         }
         catch (Exception)
         {
            return BadRequest("Erro ao buscar por fornecedores");            
         }
      }
   }
}
