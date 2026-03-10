using Microsoft.OpenApi.Models;
using NetTopologySuite.Geometries;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApi.Swagger;

public class GeometryReferenceSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!typeof(Geometry).IsAssignableFrom(context.Type))
        {
            return;
        }

        bool isGeneratingComponentDefinition = context.MemberInfo == null && context.ParameterInfo == null;
        if (isGeneratingComponentDefinition)
        {
            return;
        }

        schema.Properties?.Clear();
        schema.Required?.Clear();
        schema.AllOf?.Clear();
        schema.Type = null;

        schema.Reference = new OpenApiReference
        {
            Type = ReferenceType.Schema,
            Id = context.Type.Name,
        };
    }
}
