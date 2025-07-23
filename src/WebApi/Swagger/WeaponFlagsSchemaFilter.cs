using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class WeaponFlagsSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.Name == "ItemWeaponComponentViewModel" && schema.Properties.ContainsKey("flags"))
        {
            schema.Properties["flags"] = new OpenApiSchema
            {
                Type = "array",
                Items = new OpenApiSchema
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = "WeaponFlags",
                    },
                },
            };
        }
    }
}
