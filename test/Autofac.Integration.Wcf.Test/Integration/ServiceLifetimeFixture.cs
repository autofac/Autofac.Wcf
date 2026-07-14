// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceModel;
using System.ServiceModel.Description;
using Autofac.Integration.Wcf.Test.Stubs;

namespace Autofac.Integration.Wcf.Test.Integration;

/// <summary>
/// Opens a real WCF host and pushes messages through the full pipeline,
/// verifying lifetime, disposal, and dependency injection for each
/// <see cref="InstanceContextMode"/>.
/// </summary>
public class ServiceLifetimeFixture
{
    [WindowsFact]
    public void HostConfigurationAction_AppliesToOpenedHost()
    {
        var builder = new ContainerBuilder();
        builder.RegisterInstance(new DependencyActivity());
        builder.RegisterType<TrackedDependency>().InstancePerLifetimeScope();
        builder.RegisterType<PerCallEchoService>().AsSelf().As<IEchoService>().InstancePerDependency();

        var baseAddress = WcfTestHarness.CreateBaseAddress();
        var applied = false;
        try
        {
            AutofacHostFactory.HostConfigurationAction = host =>
            {
                applied = true;
                host.Description.Behaviors.Add(new ServiceMetadataBehavior());
            };

            WcfTestHarness.WithHostedContainer(builder.Build(), () =>
            {
                var host = (ServiceHost)new AutofacServiceHostFactory().CreateServiceHost(
                    typeof(PerCallEchoService).AssemblyQualifiedName!,
                    new[] { baseAddress });

                WcfTestHarness.HostAndInvoke<IEchoService>(host, baseAddress, channel =>
                {
                    Assert.Equal("hi", channel.Echo("hi"));
                });

                Assert.True(applied);
                Assert.NotNull(host.Description.Behaviors.Find<ServiceMetadataBehavior>());
            });
        }
        finally
        {
            AutofacHostFactory.HostConfigurationAction = null;
        }
    }

    [WindowsFact]
    public void PerCallService_InjectsConstructorDependency()
    {
        var activity = new DependencyActivity();
        var builder = new ContainerBuilder();
        builder.RegisterInstance(activity);
        builder.RegisterType<TrackedDependency>().InstancePerLifetimeScope();
        builder.RegisterType<PerCallEchoService>().AsSelf().As<IEchoService>().InstancePerDependency();

        var baseAddress = WcfTestHarness.CreateBaseAddress();
        WcfTestHarness.WithHostedContainer(builder.Build(), () =>
        {
            var host = (ServiceHost)new AutofacServiceHostFactory().CreateServiceHost(
                typeof(PerCallEchoService).AssemblyQualifiedName!,
                new[] { baseAddress });

            string dependencyId = null!;
            WcfTestHarness.HostAndInvoke<IEchoService>(host, baseAddress, channel =>
            {
                dependencyId = channel.GetDependencyId();
                Assert.Equal("echo", channel.Echo("echo"));
            });

            Assert.False(string.IsNullOrEmpty(dependencyId));
            Assert.True(activity.WasDisposed(dependencyId));
        });
    }

    [WindowsFact]
    public void PerCallService_ResolvesNewInstanceAndDisposesDependencyEachCall()
    {
        var activity = new DependencyActivity();
        var builder = new ContainerBuilder();
        builder.RegisterInstance(activity);
        builder.RegisterType<TrackedDependency>().InstancePerLifetimeScope();
        builder.RegisterType<PerCallEchoService>().AsSelf().As<IEchoService>().InstancePerDependency();

        var baseAddress = WcfTestHarness.CreateBaseAddress();
        WcfTestHarness.WithHostedContainer(builder.Build(), () =>
        {
            var host = (ServiceHost)new AutofacServiceHostFactory().CreateServiceHost(
                typeof(PerCallEchoService).AssemblyQualifiedName!,
                new[] { baseAddress });

            string firstInstance = null!;
            string secondInstance = null!;
            WcfTestHarness.HostAndInvoke<IEchoService>(host, baseAddress, channel =>
            {
                firstInstance = channel.GetInstanceId();
                secondInstance = channel.GetInstanceId();
            });

            // Per-call: a new instance per operation, each dependency disposed
            // when its call's instance context ends.
            Assert.NotEqual(firstInstance, secondInstance);
            Assert.Equal(2, activity.CreatedCount);
            Assert.Equal(2, activity.DisposedCount);
        });
    }

    [WindowsFact]
    public void PerSessionService_ReusesInstanceForChannelThenDisposes()
    {
        var activity = new DependencyActivity();
        var builder = new ContainerBuilder();
        builder.RegisterInstance(activity);
        builder.RegisterType<TrackedDependency>().InstancePerLifetimeScope();
        builder.RegisterType<PerSessionEchoService>().AsSelf().As<IEchoService>().InstancePerDependency();

        var baseAddress = WcfTestHarness.CreateBaseAddress();
        WcfTestHarness.WithHostedContainer(builder.Build(), () =>
        {
            var host = (ServiceHost)new AutofacServiceHostFactory().CreateServiceHost(
                typeof(PerSessionEchoService).AssemblyQualifiedName!,
                new[] { baseAddress });

            string firstInstance = null!;
            string secondInstance = null!;
            WcfTestHarness.HostAndInvoke<IEchoService>(host, baseAddress, channel =>
            {
                firstInstance = channel.GetInstanceId();
                secondInstance = channel.GetInstanceId();
            });

            // Per-session: one instance answers both calls, disposed when the
            // channel (session) closes.
            Assert.Equal(firstInstance, secondInstance);
            Assert.Equal(1, activity.CreatedCount);
            Assert.Equal(1, activity.DisposedCount);
        });
    }

    [WindowsFact]
    public void SingletonService_SharesInstanceAcrossCalls()
    {
        var activity = new DependencyActivity();
        var builder = new ContainerBuilder();
        builder.RegisterInstance(activity);
        builder.RegisterType<TrackedDependency>().SingleInstance();
        builder.RegisterType<SingletonEchoService>().AsSelf().As<IEchoService>().SingleInstance();

        var baseAddress = WcfTestHarness.CreateBaseAddress();
        WcfTestHarness.WithHostedContainer(builder.Build(), () =>
        {
            var host = (ServiceHost)new AutofacServiceHostFactory().CreateServiceHost(
                typeof(SingletonEchoService).AssemblyQualifiedName!,
                new[] { baseAddress });

            // A clean (non-proxied) singleton is hosted directly, so WCF reflects
            // the concrete type.
            Assert.Equal(typeof(SingletonEchoService), host.Description.ServiceType);

            var instanceIds = new List<string>();
            WcfTestHarness.HostAndInvoke<IEchoService>(host, baseAddress, channel =>
            {
                instanceIds.Add(channel.GetInstanceId());
                instanceIds.Add(channel.GetInstanceId());
            });

            Assert.Single(instanceIds.Distinct());
            Assert.Equal(1, activity.CreatedCount);
        });
    }
}
