using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApi.Swagger;

public class RemoveUnusedSchemasFilter : ISchemaFilter
{
    private static readonly HashSet<string> _unusedSchemas = new()
    {
        "Coordinate",
        "CoordinateEqualityComparer",
        "CoordinateSequence",
        "CoordinateSequenceFactory",
        "Dimension",
        "ElevationModel",
        "Envelope",
        "Geometry",
        "GeometryFactory",
        "GeometryOverlay",
        "GeometryRelate",
        "LineString",
        "LinearRing",
        "NtsGeometryServices",
        "OgcGeometryType",
        "Ordinates",
        "Point",
        "Polygon",
        "PrecisionModel",
        "PrecisionModels",
    };

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (_unusedSchemas.Contains(context.Type.Name))
        {
            string schemaName = context.Type.Name;
            if (context.SchemaRepository.Schemas.ContainsKey(schemaName))
            {
                context.SchemaRepository.Schemas.Remove(schemaName);
            }
        }
    }
}
