   using BobaShop.Application.Recommendations;
   using BobaShop.Persistence.Services;
   using Microsoft.Extensions.DependencyInjection.Extensions;
   using BobaShop.Persistence.Context;
   using Microsoft.EntityFrameworkCore;
   using Microsoft.Extensions.DependencyInjection;

   namespace BobaShop.Persistence;

   public static class DependencyInjection
   {
       public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
       {
           services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
           services.TryAddSingleton<RecommendationOptions>();
           services.AddScoped<IRecommendationService, RecommendationService>();
           return services;
       }
   }
