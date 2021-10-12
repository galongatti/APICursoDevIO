using AutoMapper;
using DevIO.API.DTO;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.API.Controllers
{
   [Route("api/produtos")]
   public class ProdutoController : MainController
   {

      private readonly IProdutoRepository _produtoRepository;
      private readonly IProdutoService _produtoService;
      private readonly IMapper _mapper;


      public ProdutoController(INotificador notificador, IProdutoRepository produtoRepository, IProdutoService produtoService, IMapper mapper) : base(notificador)
      {
         _produtoRepository = produtoRepository;
         _produtoService = produtoService;
         _mapper = mapper;
      }

      [HttpGet]
      public async Task<IEnumerable<ProdutoDTO>> ObterTodos()
      {
         return _mapper.Map<IEnumerable<ProdutoDTO>>(await _produtoRepository.ObterProdutosFornecedores());
      }


      [HttpGet("{id:guid}")]
      public async Task<ActionResult<ProdutoDTO>> ObterPorId(Guid id)
      {
         ProdutoDTO produtoDTO = await ObterProduto(id);
         if (produtoDTO == null)
         {
            NotificarErro("Produto não encontrado");
            return CustomResponse();
         }

         return CustomResponse(produtoDTO);

      }

      [HttpPost]
      public async Task<ActionResult<ProdutoDTO>> CadastrarProduto(ProdutoDTO produtoDTO)
      {
         if (!ModelState.IsValid)
         {
            return CustomResponse(ModelState);
         }

         var imagemNome = Guid.NewGuid() + "_" + produtoDTO.Imagem;

         if (!UploadArquivo(produtoDTO.ImagemUpload, imagemNome)) return CustomResponse(produtoDTO);

         produtoDTO.Imagem = imagemNome;

         Produto produto = _mapper.Map<Produto>(produtoDTO);
         await _produtoService.Adicionar(produto);
         return CustomResponse(produtoDTO);
      }


      [HttpPut("{id:guid}")]
      public async Task<IActionResult> AtualizarProduto(Guid id, ProdutoDTO produtoDTO)
      {
         if(!id.Equals(produtoDTO.Id))
         {
            NotificarErro("ID Inválido");
            return CustomResponse();
         }

         ProdutoDTO produtoAtualizacao = await ObterProduto(id);
         produtoDTO.Imagem = produtoAtualizacao.Imagem;
         if (!ModelState.IsValid)
         {
            return CustomResponse(ModelState);
         }

         if(!string.IsNullOrEmpty(produtoDTO.ImagemUpload))
         {
            string imagemNome = string.Concat(Guid.NewGuid(), "_", produtoDTO.Imagem);

            if(!UploadArquivo(produtoDTO.ImagemUpload, imagemNome))
            {
               return CustomResponse(ModelState);
            }

            produtoAtualizacao.Imagem = imagemNome;
         }

         produtoAtualizacao.Nome = produtoDTO.Nome;
         produtoAtualizacao.Descricao = produtoDTO.Descricao;
         produtoAtualizacao.Valor = produtoDTO.Valor;
         produtoAtualizacao.Ativo = produtoDTO.Ativo;

         await _produtoService.Atualizar(_mapper.Map<Produto>(produtoAtualizacao));

         return CustomResponse(produtoDTO);
      }

      [HttpPost("CadastrarProdutoImgGrande")]
      public async Task<ActionResult<ProdutoDTO>> CadastrarProdutoImgGrande(ProdutoImgDTO produtoDTO)
      {
         if (!ModelState.IsValid)
         {
            return CustomResponse(ModelState);
         }

         var imgPrefixo = Guid.NewGuid() + "_";

         if (!await UploadArquivoGrande(produtoDTO.ImagemUpload, imgPrefixo)) return CustomResponse(produtoDTO);

         produtoDTO.Imagem = imgPrefixo + produtoDTO.ImagemUpload.FileName;

         Produto produto = _mapper.Map<Produto>(produtoDTO);
         await _produtoService.Adicionar(produto);
         return CustomResponse(produtoDTO);
      }


      [RequestSizeLimit(400000000)]
      [HttpPost("imagem")]
      public async Task<ActionResult> AdicionarImagem(IFormFile file)
      {
         return Ok(file);
      }



      [HttpDelete("{id:guid}")]
      public async Task<ActionResult<ProdutoDTO>> Excluir(Guid id)
      {
         ProdutoDTO produtoDTO = await ObterProduto(id);

         if (produtoDTO == null)
         {
            NotificarErro("Produto não encontrado");
            return CustomResponse();
         }

         await _produtoService.Remover(id);
         return CustomResponse();
      }

      private bool UploadArquivo(string arquivo, string imgNome)
      {
         byte[] imageDataByteArray = Convert.FromBase64String(arquivo);
         if (string.IsNullOrEmpty(arquivo))
         {
            NotificarErro("Forneça uma imagem para este produto");
            return false;
         }

         string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagens", imgNome);

         if (System.IO.File.Exists(filePath))
         {
            NotificarErro("Já existe um arquivo com este nome!");
            return false;
         }

         System.IO.File.WriteAllBytes(filePath, imageDataByteArray);
         return true;
      }

      private async Task<bool> UploadArquivoGrande(IFormFile arquivo, string imgPrefixo)
      {
         
         if (arquivo.Length == 0 || arquivo == null)
         {
            NotificarErro("Forneça uma imagem para este produto");
            return false;
         }

         string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagens", imgPrefixo + arquivo.FileName);

         if (System.IO.File.Exists(filePath))
         {
            NotificarErro("Já existe um arquivo com este nome!");
            return false;
         }

         using (var stream = new FileStream(filePath, FileMode.Create))
         {
            await arquivo.CopyToAsync(stream);
         }

         return true;
      }


      private async Task<ProdutoDTO> ObterProduto(Guid id)
      {
         return _mapper.Map<ProdutoDTO>(await _produtoRepository.ObterProdutoFornecedor(id));
      }





   }
}
