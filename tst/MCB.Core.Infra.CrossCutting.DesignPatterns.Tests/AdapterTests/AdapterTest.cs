using FluentAssertions;
using Mapster;
using MCB.Core.Infra.CrossCutting.DependencyInjection;
using MCB.Core.Infra.CrossCutting.DependencyInjection.Abstractions.Enums;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Abstractions.Adapter;
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

        IoC.Bootstrapper.ConfigureServices(
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
        var address = adapter.Adapt<AddressDto, Address>(addressDto) ?? new Address();
        var address2 = adapter.Adapt<AddressDto, Address>(addressDto, existingTarget: default) ?? new Address();

        // Assert
        address.Id.Should().Be(id);
        address.City.Should().Be(addressDto.City);
        address.Neighborhood.Should().Be(addressDto.Neighborhood);
        address.Number.Should().Be(addressDto.Number);
        address.Street.Should().Be(addressDto.Street);
        address.ZipCode.Should().Be(addressDto.ZipCode);

        address2.Id.Should().Be(id);
        address2.Neighborhood.Should().Be(addressDto.Neighborhood);
        address2.Number.Should().Be(addressDto.Number);
        address2.Street.Should().Be(addressDto.Street);
        address2.ZipCode.Should().Be(addressDto.ZipCode);

    }

    [Fact]
    public void Adapter_Shoul_Be_Adapt_Correctly_With_Existing_Target()
    {
        // Arrange
        var dependencyInjectionContainer = new DependencyInjectionContainer();
        dependencyInjectionContainer.Build();

        IoC.Bootstrapper.ConfigureServices(
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

        var aditionalAddressProperty = 1;
        var addressDto = new AddressDto
        {
            City = "São Paulo",
            Neighborhood = "Se",
            Number = "N/A",
            Street = "Praça da Sé",
            ZipCode = "01001-000"
        };

        // Act
        var address = new Address { AditionalAddressProperty = aditionalAddressProperty };
        address = adapter.Adapt(addressDto, address) ?? new Address();

        // Assert
        address.AditionalAddressProperty.Should().Be(aditionalAddressProperty);
        address.City.Should().Be(addressDto.City);
        address.Neighborhood.Should().Be(addressDto.Neighborhood);
        address.Number.Should().Be(addressDto.Number);
        address.Street.Should().Be(addressDto.Street);
        address.ZipCode.Should().Be(addressDto.ZipCode);
    }

    [Fact]
    public void Adapter_Should_Not_Adapt_Null_Value()
    {
        // Arrange
        var dependencyInjectionContainer = new DependencyInjectionContainer();
        bool hasRaisedException = false;

        IoC.Bootstrapper.ConfigureServices(
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
        dependencyInjectionContainer.Build();
        var adapter = dependencyInjectionContainer.Resolve<IAdapter>();

        // Act
        var address = default(Address);

        try
        {
            address = adapter.Adapt<AddressDto, Address>(null);
        }
        catch (ArgumentNullException ex)
        {
            hasRaisedException = ex.ParamName == "source";
        }

        // Assert
        hasRaisedException.Should().BeTrue();
    }
}