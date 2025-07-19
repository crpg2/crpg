using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApi.Swagger;

public class MakeAllParametersRequiredOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var methodParameters = context.MethodInfo.GetParameters();

        foreach (var parameter in operation.Parameters)
        {
            var methodParam = methodParameters.FirstOrDefault(p =>
                p.Name?.Equals(parameter.Name, StringComparison.OrdinalIgnoreCase) == true);

            if (methodParam != null)
            {
                bool isNullable = methodParam.ParameterType.IsClass || Nullable.GetUnderlyingType(methodParam.ParameterType) != null;
                parameter.Required = !isNullable;
            }
        }
    }
}
