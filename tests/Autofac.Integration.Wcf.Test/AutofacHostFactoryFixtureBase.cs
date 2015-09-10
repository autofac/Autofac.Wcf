using System;
using System.ServiceModel;
using Xunit;

namespace Autofac.Integration.Wcf.Test
{
    public abstract class AutofacHostFactoryFixtureBase<T>
        where T : AutofacHostFactory, new()
    {
        readonly Uri[] _dummyEndpoints = new[] { new Uri("http://localhost") };

        [Fact]
        public void NullConstructorStringThrowsException()
        {
            var factory = new T();
            var exception = Assert.Throws<ArgumentNullException>(() => factory.CreateServiceHost(null, _dummyEndpoints));
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
                    string expectedMessage = string.Format(AutofacHostFactoryResources.ServiceMustNotBeSingleInstance, typeof(object).FullName);
                    Assert.Equal(expectedMessage, exception.Message);
                });
        }

        [Fact]
        public void HostsSingletonServices()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<TestSingletonService>().SingleInstance();
            TestWithHostedContainer(builder.Build(), () =>
                {
                    var factory = new T();
                    var host = factory.CreateServiceHost(typeof(TestSingletonService).AssemblyQualifiedName, _dummyEndpoints);
                    Assert.NotNull(host);
                    Assert.Equal(typeof(TestSingletonService), host.Description.ServiceType);
                });
        }

        [Fact]
        public void SingletonServiceMustBeRegisteredAsSingleInstance()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<TestSingletonService>().InstancePerDependency();
            TestWithHostedContainer(builder.Build(), () =>
                {
                    var factory = new T();
                    var exception = Assert.Throws<InvalidOperationException>(
                    () => factory.CreateServiceHost(typeof(TestSingletonService).AssemblyQualifiedName, _dummyEndpoints));
                    string expectedMessage = string.Format(AutofacHostFactoryResources.ServiceMustBeSingleInstance, typeof(TestSingletonService).FullName);
                    Assert.Equal(expectedMessage, exception.Message);
                });
        }

        [Fact]
        public void DetectsUnknownImplementationTypes()
        {
            var builder = new ContainerBuilder();
            builder.Register<ITestService>(c => new TestService()).Named<object>("service");
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
                ServiceHostBase hostParameter = null;
                ServiceHostBase actualHost = null;
                bool actionCalled = false;

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

        static void TestWithHostedContainer(IContainer container, Action test)
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
}