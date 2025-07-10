using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace WebApi.Swagger;

// enum: [0, 1, ...] => ["String0", "String1", ...]
public class FlagsEnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;

        if (type.IsEnum && type.GetCustomAttributes(typeof(FlagsAttribute), false).Any())
        {
            var enumValues = Enum.GetValues(type).Cast<Enum>();

            schema.Type = "string";
            schema.Format = string.Empty;
            schema.Enum.Clear();

            var names = new OpenApiArray();
            foreach (var value in enumValues)
            {
                long intValue = Convert.ToInt64(value);
                schema.Enum.Add(new OpenApiLong(intValue));
                names.Add(new OpenApiString(value.ToString()));
            }

            schema.Enum = names;
        }
    }
}
