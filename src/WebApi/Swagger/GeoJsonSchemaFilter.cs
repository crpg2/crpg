using Microsoft.OpenApi.Models;
using NetTopologySuite.Geometries;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApi.Swagger;

public class GeoJsonSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!typeof(Geometry).IsAssignableFrom(context.Type))
        {
            return;
        }

        string geoJsonType = GetGeoJsonType(context.Type);

        schema.AllOf = null;
        schema.Properties.Clear();
        schema.Required.Clear();
        schema.Type = null;
        schema.Reference = new OpenApiReference
        {
            Type = ReferenceType.Schema,
            Id = $"GeoJson{geoJsonType}",
        };
    }

    private static string GetGeoJsonType(Type type) => type.Name switch
    {
        nameof(Point) => "Point",
        nameof(MultiPoint) => "MultiPoint",
        nameof(Polygon) => "Polygon",
        nameof(MultiPolygon) => "MultiPolygon",
        _ => "Geometry",
    };
}
