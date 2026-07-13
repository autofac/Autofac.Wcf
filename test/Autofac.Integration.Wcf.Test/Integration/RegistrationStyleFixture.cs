// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceModel;
using Autofac.Integration.Wcf.Test.Stubs;

namespace Autofac.Integration.Wcf.Test.Integration;

/// <summary>
/// Exercises the three documented service registration styles (by type, by
/// interface, by name), each hosting a service that services a real request.
/// </summary>
public class RegistrationStyleFixture
{
    [WindowsFact]
    public void RegisterByInterface_ServicesRequest()
    {
        var builder = new ContainerBuilder();
        RegisterDependencies(builder);
        builder.RegisterType<PerCallEchoService>().As<IEchoService>().InstancePerDependency();

        // Constructor string is the contract interface.
        AssertServicesRequest(builder.Build(), typeof(IEchoService).AssemblyQualifiedName!);
    }

    [WindowsFact]
    public void RegisterByName_ServicesRequest()
    {
        var builder = new ContainerBuilder();
        RegisterDependencies(builder);

        // Named services must be registered as object.
        builder.RegisterType<PerCallEchoService>().Named<object>("echo-service").InstancePerDependency();

        // Constructor string is the registered name.
        AssertServicesRequest(builder.Build(), "echo-service");
    }

    [WindowsFact]
    public void RegisterByType_ServicesRequest()
    {
        var builder = new ContainerBuilder();
        RegisterDependencies(builder);
        builder.RegisterType<PerCallEchoService>().InstancePerDependency();

        // Constructor string is the concrete implementation type.
        AssertServicesRequest(builder.Build(), typeof(PerCallEchoService).AssemblyQualifiedName!);
    }

    private static void RegisterDependencies(ContainerBuilder builder)
    {
        builder.RegisterInstance(new DependencyActivity());
        builder.RegisterType<TrackedDependency>().InstancePerLifetimeScope();
    }

    private static void AssertServicesRequest(IContainer container, string constructorString)
    {
        var baseAddress = WcfTestHarness.CreateBaseAddress();
        WcfTestHarness.WithHostedContainer(container, () =>
        {
            var host = (ServiceHost)new AutofacServiceHostFactory().CreateServiceHost(constructorString, new[] { baseAddress });
            WcfTestHarness.HostAndInvoke<IEchoService>(host, baseAddress, channel =>
            {
                Assert.Equal("hello", channel.Echo("hello"));
                Assert.False(string.IsNullOrEmpty(channel.GetDependencyId()));
            });
        });
    }
}
