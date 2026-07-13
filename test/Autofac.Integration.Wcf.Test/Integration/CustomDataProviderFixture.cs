// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceModel;
using Autofac.Integration.Wcf.Test.Stubs;

namespace Autofac.Integration.Wcf.Test.Integration;

/// <summary>
/// Verifies that a custom <see cref="IServiceImplementationDataProvider"/> is
/// honored end-to-end - the documented extensibility seam that packages such as
/// Autofac.Multitenant.Wcf build on.
/// </summary>
public class CustomDataProviderFixture
{
    [WindowsFact]
    public void CustomProvider_ControlsHostedServiceAndServicesRequest()
    {
        var builder = new ContainerBuilder();
        builder.RegisterInstance(new DependencyActivity());
        builder.RegisterType<TrackedDependency>().InstancePerLifetimeScope();
        builder.RegisterType<PerCallEchoService>().As<IEchoService>().InstancePerDependency();

        var baseAddress = WcfTestHarness.CreateBaseAddress();
        try
        {
            AutofacHostFactory.ServiceImplementationDataProvider = new FixedProvider();
            WcfTestHarness.WithHostedContainer(builder.Build(), () =>
            {
                // Any constructor string works; the provider decides the type.
                var host = (ServiceHost)new AutofacServiceHostFactory().CreateServiceHost("whatever", new[] { baseAddress });
                Assert.Equal(typeof(PerCallEchoService), host.Description.ServiceType);

                WcfTestHarness.HostAndInvoke<IEchoService>(host, baseAddress, channel =>
                {
                    Assert.Equal("via-custom-provider", channel.Echo("via-custom-provider"));
                });
            });
        }
        finally
        {
            AutofacHostFactory.ServiceImplementationDataProvider = null;
        }
    }

    private sealed class FixedProvider : IServiceImplementationDataProvider
    {
        public ServiceImplementationData GetServiceImplementationData(string value)
            => new()
            {
                ConstructorString = value,
                ServiceTypeToHost = typeof(PerCallEchoService),
                ImplementationResolver = scope => scope.Resolve<IEchoService>(),
            };
    }
}
