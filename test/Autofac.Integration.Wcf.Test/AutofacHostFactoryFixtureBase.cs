// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceModel;
using Autofac.Extras.DynamicProxy;
using Autofac.Integration.Wcf.Test.Stubs;

namespace Autofac.Integration.Wcf.Test;

public abstract class AutofacHostFactoryFixtureBase<T>
    where T : AutofacHostFactory, new()
{
    private readonly Uri[] _dummyEndpoints = { new Uri("http://localhost") };

    [Fact]
    public void NullConstructorStringThrowsException()
    {
        var factory = new T();
        var exception = Assert.Throws<ArgumentNullException>(() => factory.CreateServiceHost(null!, _dummyEndpoints));
        Assert.Equal("constructorString", exception.ParamName);
    }

    [Fact]
    public void EmptyConstructorStringThrowsException()
    {
        var factory = new T();
        var exception = Assert.Throws<ArgumentException>(() => factory.CreateServiceHost(string.Empty, _dummyEndpoints));
        Assert.Equal("constructorString", exception.ParamName);
    }

    [Fact]
    public void HostsKeyedServices()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<object>().Named<object>("service");
        TestWithHostedContainer(builder.Build(), () =>
            {
                var factory = new T();
                var host = factory.CreateServiceHost("service", _dummyEndpoints);
                Assert.NotNull(host);
            });
    }

    [Fact]
    public void HostsTypedServices()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<object>();
        TestWithHostedContainer(builder.Build(), () =>
            {
                var factory = new T();
                var host = factory.CreateServiceHost(typeof(object).FullName, _dummyEndpoints);
                Assert.NotNull(host);
            });
    }

    [Fact]
    public void HostsTypedServicesAsServices()
    {
        var builder = new ContainerBuilder();
        builder.Register(c => "Test").As<object>();
        TestWithHostedContainer(builder.Build(), () =>
            {
                var factory = new T();
                var host = factory.CreateServiceHost(typeof(object).FullName, _dummyEndpoints);
                Assert.NotNull(host);
                Assert.Equal(typeof(string), host.Description.ServiceType);
            });
    }

    [Fact]
    public void NonSingletonServiceMustNotBeRegisteredAsSingleInstance()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<object>().SingleInstance();
        TestWithHostedContainer(builder.Build(), () =>
            {
                var factory = new T();
                var exception = Assert.Throws<InvalidOperationException>(() => factory.CreateServiceHost(typeof(object).FullName, _dummyEndpoints));
                var expectedMessage = string.Format(AutofacHostFactoryResources.ServiceMustNotBeSingleInstance, typeof(object).FullName);
                Assert.Equal(expectedMessage, exception.Message);
            });
    }

    [Fact]
    public void HostsSingletonServices()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<SingletonService>().SingleInstance();
        TestWithHostedContainer(builder.Build(), () =>
            {
                var factory = new T();
                var host = factory.CreateServiceHost(typeof(SingletonService).AssemblyQualifiedName, _dummyEndpoints);
                Assert.NotNull(host);
                Assert.Equal(typeof(SingletonService), host.Description.ServiceType);
            });
    }

    [Fact]
    public void HostsSingletonServicesRegisteredWithInterfaceInterceptors()
    {
        // Issue 31: a SingleInstance service with interface interceptors resolves
        // to a Castle proxy whose type both declares and inherits a
        // ServiceContract. Wrapping it in a DispatchProxy lets the host build.
        var builder = new ContainerBuilder();
        builder.RegisterType<CountingInterceptor>();
        builder.RegisterType<SingletonService>()
            .As<ISingletonService>()
            .SingleInstance()
            .EnableInterfaceInterceptors()
            .InterceptedBy(typeof(CountingInterceptor));
        TestWithHostedContainer(builder.Build(), () =>
            {
                var factory = new T();
                var host = factory.CreateServiceHost(typeof(ISingletonService).AssemblyQualifiedName, _dummyEndpoints);
                Assert.NotNull(host);

                // The hosted type must implement the contract but not declare its
                // own ServiceContract. The Castle proxy declares one; the
                // DispatchProxy wrapper does not.
                Assert.True(typeof(ISingletonService).IsAssignableFrom(host.Description.ServiceType));
                Assert.Empty(host.Description.ServiceType.GetCustomAttributes(typeof(ServiceContractAttribute), false));
            });
    }

    [Fact]
    public void SingletonServiceMustBeRegisteredAsSingleInstance()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<SingletonService>().InstancePerDependency();
        TestWithHostedContainer(builder.Build(), () =>
            {
                var factory = new T();
                var exception = Assert.Throws<InvalidOperationException>(
                () => factory.CreateServiceHost(typeof(SingletonService).AssemblyQualifiedName, _dummyEndpoints));
                var expectedMessage = string.Format(AutofacHostFactoryResources.ServiceMustBeSingleInstance, typeof(SingletonService).FullName);
                Assert.Equal(expectedMessage, exception.Message);
            });
    }

    [Fact]
    public void DetectsUnknownImplementationTypes()
    {
        var builder = new ContainerBuilder();
        builder.Register<IEchoService>(c => new PerCallEchoService(new TrackedDependency(new DependencyActivity()))).Named<object>("service");
        TestWithHostedContainer(builder.Build(), () =>
            {
                var factory = new T();
                Assert.Throws<InvalidOperationException>(() => factory.CreateServiceHost("service", _dummyEndpoints));
            });
    }

    [Fact]
    public void DetectsUnknownServiceTypes()
    {
        var builder = new ContainerBuilder();

        // No service registered at all.
        TestWithHostedContainer(builder.Build(), () =>
            {
                var factory = new T();
                Assert.Throws<InvalidOperationException>(() => factory.CreateServiceHost("service", _dummyEndpoints));
            });
    }

    [Fact]
    public void ExecutesHostConfigurationActionWhenSet()
    {
        try
        {
            ServiceHostBase? hostParameter = null;
            ServiceHostBase? actualHost = null;
            var actionCalled = false;

            AutofacHostFactory.HostConfigurationAction = host =>
            {
                hostParameter = host;
                actionCalled = true;
            };

            var builder = new ContainerBuilder();
            builder.RegisterType<object>();
            TestWithHostedContainer(builder.Build(), () =>
                {
                    var factory = new T();
                    actualHost = factory.CreateServiceHost(typeof(object).FullName, _dummyEndpoints);
                    Assert.NotNull(actualHost);
                });

            Assert.Same(hostParameter, actualHost);
            Assert.True(actionCalled);
        }
        finally
        {
            AutofacHostFactory.HostConfigurationAction = null;
        }
    }

    private static void TestWithHostedContainer(IContainer container, Action test)
    {
        AutofacHostFactory.Container = container;
        try
        {
            test();
        }
        finally
        {
            AutofacHostFactory.Container = null;
        }
    }
}
