using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApi.Swagger;

public class GeoJsonDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Components.Schemas["GeoJsonPoint"] = GeoJsonSchemas.PointSchema;
        swaggerDoc.Components.Schemas["GeoJsonMultiPoint"] = GeoJsonSchemas.MultiPointSchema;
        swaggerDoc.Components.Schemas["GeoJsonPolygon"] = GeoJsonSchemas.PolygonSchema;
    }
}
