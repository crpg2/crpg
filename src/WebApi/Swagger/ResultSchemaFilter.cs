using Crpg.Application.Common.Results;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApi.Swagger;

public class ResultSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsGenericType)
        {
            return;
        }

        var typeDef = context.Type.GetGenericTypeDefinition();

        if (typeDef == typeof(Result<>))
        {
            if (!schema.Required.Contains("errors"))
            {
                schema.Required.Add("errors");
            }

            if (!schema.Required.Contains("data"))
            {
                schema.Required.Add("data");
            }
        }
    }
}
