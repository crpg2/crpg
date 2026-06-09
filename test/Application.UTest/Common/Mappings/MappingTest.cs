using AutoMapper;
using Crpg.Application.Common.Mappings;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Mappings;

public class MappingTest
{
    private static readonly IConfigurationProvider ConfigurationProvider = new MapperConfiguration(
        cfg => cfg.AddProfile<MappingProfile>(),
        NullLoggerFactory.Instance);

    [Test]
    public void ShouldHaveValidConfiguration()
    {
        ConfigurationProvider.AssertConfigurationIsValid();
    }
}
