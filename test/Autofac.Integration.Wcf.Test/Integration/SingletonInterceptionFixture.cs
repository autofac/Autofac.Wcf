// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceModel;
using Autofac.Extras.DynamicProxy;
using Autofac.Integration.Wcf.Test.Stubs;

namespace Autofac.Integration.Wcf.Test.Integration;

/// <summary>
/// Opens a real host for a singleton service configured with interface
/// interception and verifies it routes through both the interceptor and the
/// Autofac-resolved instance.
/// </summary>
/// <remarks>
/// <para>
/// This mirrors the reported configuration:
/// <c>SingleInstance().EnableInterfaceInterceptors().InterceptedBy(...)</c>. The
/// resolved instance is a Castle proxy that both declares and inherits the
/// service contract, which previously broke singleton hosting.
/// </para>
/// </remarks>
public class SingletonInterceptionFixture
{
    [WindowsFact]
    public void InterceptedSingletonServiceCanBeHostedAndInvoked()
    {
        // Issue 31: intercepted singleton hosting.
        var interceptor = new CountingInterceptor();
        var builder = new ContainerBuilder();
        builder.RegisterInstance(interceptor);
        builder.RegisterType<SingletonService>()
            .As<ISingletonService>()
            .SingleInstance()
            .EnableInterfaceInterceptors()
            .InterceptedBy(typeof(CountingInterceptor));

        var baseAddress = WcfTestHarness.CreateBaseAddress();
        WcfTestHarness.WithHostedContainer(builder.Build(), () =>
        {
            var factory = new AutofacServiceHostFactory();
            var host = (ServiceHost)factory.CreateServiceHost(
                typeof(ISingletonService).AssemblyQualifiedName!,
                new[] { baseAddress });

            WcfTestHarness.HostAndInvoke<ISingletonService>(host, baseAddress, channel =>
            {
                var first = channel.Increment();
                var second = channel.Increment();

                // The same instance answers both calls, and interception runs
                // each time.
                Assert.Equal(1, first);
                Assert.Equal(2, second);
                Assert.Equal(2, interceptor.CallCount);
            });
        });
    }
}
