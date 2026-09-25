using FluentValidation.AspNetCore;
using HanwhaClient.Application;
using HanwhaClient.Helper.Validators;
using HanwhaClient.Model.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HanwhaClient.Helper.Extensions
{
    public static class FluentValidationExtension
    {
        public static IServiceCollection FluentValidationConfiguration(this IServiceCollection services)
        {

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var localizer = context.HttpContext.RequestServices
                        .GetRequiredService<IStringLocalizer<AppMessages>>();

                    var errors = context.ModelState
                        .Where(e => e.Value.Errors.Count > 0)
                        .Select(e => new
                        {
                            Field = e.Key,
                            Errors = e.Value.Errors.Select(x => x.ErrorMessage)
                        });

                    var errorList = errors.SelectMany(x => x.Errors).ToList();
                    var errorResponce = StandardAPIResponse<string>.ErrorResponse(null, localizer[MessageKeys.InvalidDataModel].Value, 400, errorList);
                    return new BadRequestObjectResult(errorResponce);
                };
            });

            services.AddControllers().AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<UsersRequestModelValidator>());
            return services;
        }
    }
}
