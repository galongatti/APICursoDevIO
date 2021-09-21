using AutoMapper;
using DevIO.API.DTO;
using DevIO.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.API.Extensions
{

   public class AutoMapperConfig : Profile
   {
      public AutoMapperConfig()
      {
         CreateMap<Fornecedor, FornecedorDTO>().ReverseMap();
         CreateMap<Endereco, EnderecoDTO>().ReverseMap();
         CreateMap<Produto, ProdutoDTO>().ReverseMap();
      }
   }
}
