using System.Text.Json;
using System.Text.Json.Serialization;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Settlements.Models;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;

namespace Crpg.Application.Common.Files;

internal class FileSettlementsSource : ISettlementsSource
{
    private static readonly string CampaignSettlementsPath = FileDataPathResolver.Resolve(
        Path.Combine("Common", "Files", "settlements.json"));

    public async Task<IEnumerable<SettlementCreation>> LoadCampaignSettlements()
    {
        await using var file = File.OpenRead(CampaignSettlementsPath);
        return (await JsonSerializer.DeserializeAsync<IEnumerable<SettlementCreation>>(file, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new GeoJsonConverterFactory(GeometryFactory.Default), new JsonStringEnumConverter() },
        }).AsTask())!;
    }
}
