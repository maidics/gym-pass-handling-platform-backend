using System.Reflection;
using System.Runtime.CompilerServices;
using AutoMapper;
using AutoMapper.Internal;
using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Entities;
using NUnit.Framework;

namespace FitPass.Application.UnitTests.Common.Mappings;

public class MappingTests
{
    private readonly IConfigurationProvider _configuration;
    private readonly IMapper _mapper;

    public MappingTests()
    {
        _configuration = new MapperConfiguration(config => 
            config.AddMaps(Assembly.GetAssembly(typeof(IApplicationDbContext))));

        _mapper = _configuration.CreateMapper();
    }

    [Test]
    public void ShouldHaveValidConfiguration()
    {
        _configuration.AssertConfigurationIsValid();
    }

    [Test]
    public void ShouldDiscoverAllMappings()
    {
        var typeMaps = _configuration.Internal().GetAllTypeMaps();

        var mappings = typeMaps.ToList();

        TestContext.Out.WriteLine($"\tFound {mappings.Count} mapping(s).");

        foreach (var map in typeMaps)
        {
            TestContext.Out.WriteLine($"{map.SourceType.Name} -> {map.DestinationType.Name}");
        }

        Assert.That(mappings, Is.Not.Empty, "No mappings were discovered.");
    }

    [Test]
    [TestCaseSource(nameof(GetMappingTestCases))]
    public void ShouldSupportMappingFromSourceToDestination(Type source, Type destination)
    {
        var instance = GetInstanceOf(source);

        _mapper.Map(instance, source, destination);
    }

    private object GetInstanceOf(Type type)
    {
        if (type.GetConstructor(Type.EmptyTypes) != null)
            return Activator.CreateInstance(type)!;

        // Type without parameterless constructor
        return RuntimeHelpers.GetUninitializedObject(type);
    }

    private static IEnumerable<TestCaseData> GetMappingTestCases()
    {
        IConfigurationProvider config = new MapperConfiguration(cfg =>
            cfg.AddMaps(Assembly.GetAssembly(typeof(IApplicationDbContext))));

        var typeMaps = config.Internal().GetAllTypeMaps();

        foreach(var map in typeMaps)
        {
            yield return new TestCaseData(map.SourceType, map.DestinationType)
                .SetName($"{map.SourceType.Name} -> {map.DestinationType.Name}");
        }
    }
}
