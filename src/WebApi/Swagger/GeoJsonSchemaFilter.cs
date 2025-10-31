using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using NetTopologySuite.Geometries;
using Swashbuckle.AspNetCore.SwaggerGen;

public class GeoJsonSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!typeof(Geometry).IsAssignableFrom(context.Type))
        {
            return;
        }

        schema.Properties.Clear();
        schema.Type = "object";
        schema.Required = new HashSet<string> { "type", "coordinates" };

        schema.Properties["type"] = new OpenApiSchema
        {
            Type = GetGeoJsonTypeName(context.Type),
        };

        schema.Properties["coordinates"] = new OpenApiSchema
        {
            Type = "array",
            Items = new OpenApiSchema { Type = "number" },
            Example = GetCoordinatesExample(context.Type),
        };
    }

    private static string GetGeoJsonTypeName(Type type)
    {
        return type.Name switch
        {
            nameof(Point) => "Point",
            nameof(LineString) => "LineString",
            nameof(Polygon) => "Polygon",
            nameof(MultiPoint) => "MultiPoint",
            nameof(MultiLineString) => "MultiLineString",
            nameof(MultiPolygon) => "MultiPolygon",
            nameof(GeometryCollection) => "GeometryCollection",
            _ => "Geometry",
        };
    }

    private static IOpenApiAny GetCoordinatesExample(Type type)
    {
        return type.Name switch
        {
            nameof(Point) => new OpenApiArray
            {
                new OpenApiDouble(124.6953125),
                new OpenApiDouble(-86.2265625),
            },
            nameof(LineString) => new OpenApiArray
            {
                new OpenApiArray { new OpenApiDouble(0), new OpenApiDouble(0) },
                new OpenApiArray { new OpenApiDouble(1), new OpenApiDouble(1) },
            },
            nameof(Polygon) => new OpenApiArray
            {
                new OpenApiArray
                {
                    new OpenApiArray { new OpenApiDouble(0), new OpenApiDouble(0) },
                    new OpenApiArray { new OpenApiDouble(1), new OpenApiDouble(0) },
                    new OpenApiArray { new OpenApiDouble(1), new OpenApiDouble(1) },
                    new OpenApiArray { new OpenApiDouble(0), new OpenApiDouble(0) },
                },
            },
            nameof(MultiPoint) => new OpenApiArray
            {
                new OpenApiArray { new OpenApiDouble(0), new OpenApiDouble(0) },
                new OpenApiArray { new OpenApiDouble(1), new OpenApiDouble(1) },
            },
            nameof(MultiLineString) => new OpenApiArray
            {
                new OpenApiArray
                {
                    new OpenApiArray { new OpenApiDouble(0), new OpenApiDouble(0) },
                    new OpenApiArray { new OpenApiDouble(1), new OpenApiDouble(1) },
                },
                new OpenApiArray
                {
                    new OpenApiArray { new OpenApiDouble(2), new OpenApiDouble(2) },
                    new OpenApiArray { new OpenApiDouble(3), new OpenApiDouble(3) },
                },
            },
            nameof(MultiPolygon) => new OpenApiArray
            {
                new OpenApiArray
                {
                    new OpenApiArray
                    {
                        new OpenApiArray { new OpenApiDouble(0), new OpenApiDouble(0) },
                        new OpenApiArray { new OpenApiDouble(1), new OpenApiDouble(0) },
                        new OpenApiArray { new OpenApiDouble(1), new OpenApiDouble(1) },
                        new OpenApiArray { new OpenApiDouble(0), new OpenApiDouble(0) },
                    },
                },
            },
            nameof(GeometryCollection) => new OpenApiArray
            {
                new OpenApiObject
                {
                    ["type"] = new OpenApiString("Point"),
                    ["coordinates"] = new OpenApiArray
                    {
                        new OpenApiDouble(0),
                        new OpenApiDouble(0),
                    },
                },
                new OpenApiObject
                {
                    ["type"] = new OpenApiString("LineString"),
                    ["coordinates"] = new OpenApiArray
                    {
                        new OpenApiArray { new OpenApiDouble(0), new OpenApiDouble(0) },
                        new OpenApiArray { new OpenApiDouble(1), new OpenApiDouble(1) },
                    },
                },
            },
            _ => new OpenApiArray(),
        };
    }
}
