using AutoMapper;
using DevIO.API.DTO;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DevIO.API.Extensions.CustomAuthorize;

namespace DevIO.API.Controllers
{
   [Authorize]
   [ApiVersion("2.0")]
   [ApiVersion("1.0", Deprecated = true)]
   [Route("api/v{version:apiVersion}/[controller]")]
   public class FornecedoresController : MainController
   {

      private readonly IFornecedorRepository _fornecedorRepository;
      private readonly IFornecedorService _fornecedorService;
      private readonly IMapper _mapper;
      private readonly IEnderecoRepository _enderecoRepository;

      public FornecedoresController(IFornecedorRepository fornecedor, IFornecedorService fornecedorService, IMapper mapper, INotificador notificador, IEnderecoRepository enderecoRepository, IUser user) : base(notificador, user)
      {
         _fornecedorRepository = fornecedor;
         _mapper = mapper;
         _fornecedorService = fornecedorService;
         _enderecoRepository = enderecoRepository;
      }

      [ClaimsAuthorize("Fornecedor", "Consultar")]
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

      [ClaimsAuthorize("Fornecedor", "Consultar")]
      [HttpGet("{id:guid}")]
      public async Task<ActionResult<FornecedorDTO>> ObterPorID(Guid id)
      {

         if (id.Equals(string.Empty))
            return BadRequest();

         try
         {
            FornecedorDTO fornecedor = await GetFornecedor(id);

            if (fornecedor == null)
            {
               return NotFound();
            }
            else
            {
               return Ok(fornecedor);
            }
         }
         catch (Exception)
         {
            return BadRequest();            
         }
      }

      [ClaimsAuthorize("Fornecedor","Cadastrar")]
      [HttpPost]
      public async Task<ActionResult<FornecedorDTO>> CadastrarUsuario(FornecedorDTO fornecedorDTO)
      {
         if (!ModelState.IsValid)
            return CustomResponse(ModelState);
         
            Fornecedor fornecedor = _mapper.Map<Fornecedor>(fornecedorDTO);
            await _fornecedorService.Adicionar(fornecedor);
            return CustomResponse(fornecedorDTO);
      }

  
      [ClaimsAuthorize("Fornecedor", "Atualizar")]
      [HttpPut]
      public async Task<ActionResult<FornecedorDTO>> AtualizarUsuario(Guid id, FornecedorDTO fornecedorDTO)
      {
         if (!ModelState.IsValid)
            return CustomResponse(ModelState);

         if (id != fornecedorDTO.Id) 
         {
            NotificarErro("O id informado não é o mesmo que foi passado na query");
            return CustomResponse(fornecedorDTO);
         }
         
       
         Fornecedor fornecedor = _mapper.Map<Fornecedor>(fornecedorDTO);
         await _fornecedorService.Atualizar(fornecedor);            
         return CustomResponse(fornecedorDTO);
      }

      [ClaimsAuthorize("Fornecedor", "Excluir")]
      [HttpDelete]
      public async Task<ActionResult<FornecedorDTO>> ExcluirUsuario(Guid id)
      {
         if (id.Equals(string.Empty))
         {
            NotificarErro("ID do fornecedor deve ser preenchido");
            return CustomResponse();
         }

         await _fornecedorService.Remover(id);
         return CustomResponse();
         
      }

      [ClaimsAuthorize("Fornecedor", "Consultar")]
      [HttpGet("obter-endereco/{id:guid}")]
      public async Task<ActionResult<EnderecoDTO>> ObterEnderecoPorId(Guid id)
      {
         if (id.Equals(string.Empty))
         {
            NotificarErro("ID do endereço do fornecedor deve ser preenchido");
            return CustomResponse();
         }

         return _mapper.Map<EnderecoDTO>(await _enderecoRepository.ObterEnderecoPorFornecedor(id));
      }

      [ClaimsAuthorize("Fornecedor", "Atualizar")]
      [HttpPut("atualizar-endereco/{id:guid}")]
      public async Task<ActionResult<EnderecoDTO>> AtualizarEndereco(Guid id, EnderecoDTO enderecoDTO)
      {

         if(id != enderecoDTO.Id)
         {
            NotificarErro("O id do endereço é diferente do que foi passado na query");
            CustomResponse();
         }

         if (!ModelState.IsValid)
         {
            return CustomResponse(ModelState);
         }

         Endereco endereco = _mapper.Map<Endereco>(enderecoDTO);
         await _fornecedorService.AtualizarEndereco(endereco);

         return CustomResponse(enderecoDTO);
      }

      private async Task<FornecedorDTO> GetFornecedor(Guid id)
      {
         return _mapper.Map<FornecedorDTO>(await _fornecedorRepository.ObterFornecedorProdutosEndereco(id));
      }
   }
}
