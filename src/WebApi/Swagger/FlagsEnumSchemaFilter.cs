using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace WebApi.Swagger;

public class FlagsEnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;

        if (type.IsEnum && type.GetCustomAttributes(typeof(FlagsAttribute), false).Any())
        {
            var enumValues = Enum.GetValues(type).Cast<Enum>();

            schema.Type = "integer";
            schema.Format = "int64";
            schema.Enum.Clear(); // Убираем вложенные строки

            var names = new OpenApiArray();
            foreach (var value in enumValues)
            {
                long intValue = Convert.ToInt64(value);
                schema.Enum.Add(new OpenApiLong(intValue));
                names.Add(new OpenApiString(value.ToString()));
            }

            schema.Extensions["x-enumNames"] = names;
        }
    }
}
