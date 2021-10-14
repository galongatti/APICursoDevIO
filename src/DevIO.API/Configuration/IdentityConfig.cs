﻿using DevIO.API.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.API.Configuration
{
   public static class IdentityConfig
   {
      public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services, IConfiguration configuration)
      {

         services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

         services.AddIdentity<IdentityUser, IdentityRole>()
         .AddRoles<IdentityRole>()
         .AddEntityFrameworkStores<ApplicationDbContext>()
         .AddDefaultTokenProviders();



         return services;
      }
   }
}