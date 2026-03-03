using Domain.Model;
using FluentValidation;
using Infrastructure.Repository;
using Infrastructure.Repository.InterfacesRepository;
using Infrastructure.Repository.InterfacesServices;
using Infrastructure.Services;
using Infrastructure.Services.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<RolRepository>();
            services.AddScoped<UsuarioRepository>();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<MenuService>();
            services.AddScoped<RolService>();
            services.AddScoped<UsuarioService>();
            return services;
        }

        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services.AddScoped<IValidator<Usuario>, UsuarioValidator>();
            return services;
        }
    }
}
