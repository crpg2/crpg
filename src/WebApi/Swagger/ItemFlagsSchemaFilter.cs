using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class ItemFlagsSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.Name == "ItemViewModel" && schema.Properties.ContainsKey("flags"))
        {
            schema.Properties["flags"] = new OpenApiSchema
            {
                Type = "array",
                Items = new OpenApiSchema
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = "ItemFlags",
                    },
                },
            };
        }
    }
}
