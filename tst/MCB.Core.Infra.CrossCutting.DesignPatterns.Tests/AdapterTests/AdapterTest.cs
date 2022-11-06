using FluentAssertions;
using Mapster;
using MCB.Core.Infra.CrossCutting.DependencyInjection;
using MCB.Core.Infra.CrossCutting.DependencyInjection.Abstractions.Enums;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Abstractions.Adapter;
using MCB.Core.Infra.CrossCutting.DesignPatterns.DependencyInjection;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Tests.AdapterTests.Models;
using System;
using Xunit;

namespace MCB.Core.Infra.CrossCutting.DesignPatterns.Tests.AdapterTests;

public class AdapterTest
{
    [Fact]
    public void Adapter_Shoul_Be_Adapt_Correctly()
    {
        // Arrange
        var dependencyInjectionContainer = new DependencyInjectionContainer();
        dependencyInjectionContainer.Build();

        Bootstrapper.ConfigureServices(
            dependencyInjectionContainer,
            adapterConfiguration =>
            {
                adapterConfiguration.DependencyInjectionLifecycle = DependencyInjectionLifecycle.Singleton;
                adapterConfiguration.TypeAdapterConfigurationFunction = new Func<TypeAdapterConfig>(() =>
                {
                    var typeAdapterConfig = new TypeAdapterConfig();

                    typeAdapterConfig.ForType<AddressDto, Address>();

                    return typeAdapterConfig;
                });
            }
        );
        var adapter = dependencyInjectionContainer.Resolve<IAdapter>();

        if (adapter == null)
        {
            Assert.False(false);
            return;
        }

        var id = Guid.NewGuid();

        var addressDto = new AddressDto
        {
            Id = id,
            City = "São Paulo",
            Neighborhood = "Se",
            Number = "N/A",
            Street = "Praça da Sé",
            ZipCode = "01001-000"
        };

        // Act
        var addressCollection = new Address?[]
        {
            (Address?)adapter.Adapt(targetType: typeof(Address), source: addressDto),

            (Address?)adapter.Adapt(targetType: typeof(Address), source: addressDto, sourceType: typeof(AddressDto)),

            (Address?)adapter.Adapt(targetType: typeof(Address), source: addressDto, existingTarget: new Address()),
            (Address?)adapter.Adapt(targetType: typeof(Address), sourceType: typeof(AddressDto), source: addressDto, existingTarget: new Address()),

            (Address?)adapter.Adapt(source: addressDto, target: new Address()),

            adapter.Adapt<AddressDto, Address>(source: addressDto),

            adapter.Adapt<Address>(source: addressDto),
            adapter.Adapt<Address>(source: addressDto, existingTarget: new Address()),
        };

        // Assert
        foreach (var address in addressCollection)
        {
            address.Should().NotBeNull();
            address!.Id.Should().Be(id);
            address!.City.Should().Be(addressDto.City);
            address!.Neighborhood.Should().Be(addressDto.Neighborhood);
            address!.Number.Should().Be(addressDto.Number);
            address!.Street.Should().Be(addressDto.Street);
            address!.ZipCode.Should().Be(addressDto.ZipCode);
        }
    }

    [Fact]
    public void Adapter_Should_Not_Adapt_Null_Value()
    {
        // Arrange
        var dependencyInjectionContainer = new DependencyInjectionContainer();
        dependencyInjectionContainer.Build();

        Bootstrapper.ConfigureServices(
            dependencyInjectionContainer,
            adapterConfiguration =>
            {
                adapterConfiguration.DependencyInjectionLifecycle = DependencyInjectionLifecycle.Singleton;
                adapterConfiguration.TypeAdapterConfigurationFunction = new Func<TypeAdapterConfig>(() =>
                {
                    var typeAdapterConfig = new TypeAdapterConfig();

                    typeAdapterConfig.ForType<AddressDto, Address>();

                    return typeAdapterConfig;
                });
            }
        );
        var adapter = dependencyInjectionContainer.Resolve<IAdapter>();

        if (adapter == null)
        {
            Assert.False(false);
            return;
        }

        // Act
        var addressCollection = new Address?[]
        {
            (Address?)adapter.Adapt(targetType: typeof(Address), source: null),

            (Address?)adapter.Adapt(targetType: typeof(Address), source: null, sourceType: typeof(AddressDto)),

            (Address?)adapter.Adapt(targetType: typeof(Address), source: null, existingTarget: null),
            (Address?)adapter.Adapt(targetType: typeof(Address), sourceType: typeof(AddressDto), source: null, existingTarget: null),

            (Address?)adapter.Adapt(source: null, target: null),

            adapter.Adapt<AddressDto, Address>(source: null),

            adapter.Adapt<Address>(source: null),
            adapter.Adapt<Address>(source: null, existingTarget: null),
        };

        // Assert
        foreach (var address in addressCollection)
            address.Should().BeNull();
    }
}