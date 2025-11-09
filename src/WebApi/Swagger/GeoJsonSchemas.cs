using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace WebApi.Swagger;

public class GeoJsonSchemas
{
    public static readonly OpenApiSchema PointSchema = new()
    {
        Type = "object",
        Required = new HashSet<string> { "type", "coordinates" },
        Properties = new Dictionary<string, OpenApiSchema>
        {
            ["type"] = new OpenApiSchema
            {
                Type = "string",
                Enum = new List<IOpenApiAny> { new OpenApiString("Point") },
                ReadOnly = true,
            },
            ["coordinates"] = new OpenApiSchema
            {
                Type = "array",
                Items = new OpenApiSchema { Type = "number", Format = "double" },
                Example = new OpenApiArray { new OpenApiDouble(124.6953125), new OpenApiDouble(-86.2265625), },
            },
        },
        Description = "GeoJSON Point geometry",
    };

    public static readonly OpenApiSchema MultiPointSchema = new()
    {
        Type = "object",
        Required = new HashSet<string> { "type", "coordinates" },
        Properties = new Dictionary<string, OpenApiSchema>
        {
            ["type"] = new OpenApiSchema
            {
                Type = "string",
                Enum = new List<IOpenApiAny> { new OpenApiString("MultiPoint") },
                ReadOnly = true,
            },
            ["coordinates"] = new OpenApiSchema
            {
                Type = "array",
                Items = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema { Type = "number", Format = "double" },
                },
                Example = new OpenApiArray
                {
                    new OpenApiArray { new OpenApiDouble(125.6), new OpenApiDouble(10.1) },
                    new OpenApiArray { new OpenApiDouble(125.7), new OpenApiDouble(10.2) },
                    new OpenApiArray { new OpenApiDouble(125.8), new OpenApiDouble(10.3) },
                },
            },
        },
        Description = "GeoJSON MultiPoint geometry",
    };

    public static readonly OpenApiSchema PolygonSchema = new()
    {
        Type = "object",
        Required = new HashSet<string> { "type", "coordinates" },
        Properties = new Dictionary<string, OpenApiSchema>
        {
            ["type"] = new OpenApiSchema
            {
                Type = "string",
                Enum = new List<IOpenApiAny> { new OpenApiString("Polygon") },
                ReadOnly = true,
            },
            ["coordinates"] = new OpenApiSchema
            {
                Type = "array",
                Items = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema { Type = "number", Format = "double" },
                    },
                },
                Example = new OpenApiArray
                {
                    new OpenApiArray
                    {
                        new OpenApiArray { new OpenApiDouble(125.1), new OpenApiDouble(10.1) },
                        new OpenApiArray { new OpenApiDouble(125.2), new OpenApiDouble(10.1) },
                        new OpenApiArray { new OpenApiDouble(125.2), new OpenApiDouble(10.2) },
                        new OpenApiArray { new OpenApiDouble(125.1), new OpenApiDouble(10.1) },
                    },
                },
            },
        },
        Description = "GeoJSON Polygon geometry",
    };
}
