using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApi.Swagger;

public class RequireAllPropertiesSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties == null)
        {
            return;
        }

        var notNullableProperties = schema.Properties
            .Where(x => !x.Value.Nullable && !schema.Required.Contains(x.Key))
            .ToList();

        foreach (var property in notNullableProperties)
        {
            _ = schema.Required.Add(property.Key);
        }

        // if (schema?.Properties == null || context.Type == null)
        // {
        //     return;
        // }

        // var requiredProperties = context.Type
        //     .GetProperties()
        //     .Where(prop =>
        //         (prop.PropertyType.IsValueType && Nullable.GetUnderlyingType(prop.PropertyType) == null) ||
        //         prop.PropertyType == typeof(string))
        //     .Select(prop => prop.Name)
        //     .ToList();

        // foreach (string? propertyName in requiredProperties)
        // {
        //     string camelCaseName = char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
        //     if (schema.Properties.ContainsKey(camelCaseName) && !schema.Required.Contains(camelCaseName))
        //     {
        //         schema.Required.Add(camelCaseName);
        //     }
        // }
    }
}
