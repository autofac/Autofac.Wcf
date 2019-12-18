using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Util;
using Xunit;

namespace Autofac.Integration.Wcf.Test
{
    public class AutofacInstanceContextFixture
    {
        [Fact]
        public void Ctor_RequiresParentScope()
        {
            Assert.Throws<ArgumentNullException>(() => new AutofacInstanceContext(null));
        }

        [Fact]
        public void Current_NoOperationContext()
        {
            Assert.Null(AutofacInstanceContext.Current);
        }

        [Fact]
        public void Dispose_InstancesDisposed()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<DisposeTracker>();
            var container = builder.Build();

            var impData = new ServiceImplementationData()
            {
                ConstructorString = "TestService",
                ServiceTypeToHost = typeof(DisposeTracker),
                ImplementationResolver = l => l.Resolve<DisposeTracker>()
            };

            var context = new AutofacInstanceContext(container);
            var disposable = (DisposeTracker)context.Resolve(impData);
            Assert.False(disposable.IsDisposedPublic);
            context.Dispose();
            Assert.True(disposable.IsDisposedPublic);
        }

        [Fact]
        public void Dispose_RegistrationInstancesDisposed()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<DisposeTracker>();
            var container = builder.Build();
            IComponentRegistration registration;
            container.ComponentRegistry.TryGetRegistration(new TypedService(typeof(DisposeTracker)), out registration);
            var context = new AutofacInstanceContext(container);
            var disposable = (DisposeTracker)context.ResolveComponent(
                new ResolveRequest(new TypedService(typeof(DisposeTracker)),
                registration,
                Enumerable.Empty<Parameter>()));
            Assert.False(disposable.IsDisposedPublic);
            context.Dispose();
            Assert.True(disposable.IsDisposedPublic);
        }

        [Fact]
        public void Resolve_RequiresServiceImplementationData()
        {
            var context = new AutofacInstanceContext(new ContainerBuilder().Build());
            Assert.Throws<ArgumentNullException>(() => context.Resolve(null));
        }

        [Fact]
        public void Resolve_ResolvesInstanceContextRegisteredModules()
        {
            var builder = new ContainerBuilder();
            var accessor = new PerInstanceContextModuleAccessor
            {
                Modules = new[] { new WcfPerIntanceContextModule() }
            };
            builder.RegisterInstance(accessor).As<IPerInstanceContextModuleAccessor>();
            var container = builder.Build();
            AutofacHostFactory.Features |= Features.InstancePerContextModules;
            var context = new AutofacInstanceContext(container);
            var service = context.OperationLifetime.Resolve<IExampleService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void Resolve_InstanceContextModulesRegistrationHandlesNullModules()
        {
            var builder = new ContainerBuilder();
            var accessor = new PerInstanceContextModuleAccessor(); 
            builder.RegisterInstance(accessor).As<IPerInstanceContextModuleAccessor>();
            var container = builder.Build();
            var context = new AutofacInstanceContext(container);
            Assert.NotNull(context);
        }

        [Fact]
        public void Resolve_InstanceContextRegisteredModulesHandlesResolveOptionalNull()
        {
            var builder = new ContainerBuilder();
            var container = builder.Build();
            var context = new AutofacInstanceContext(container);
            Assert.NotNull(context);
        }

        private interface IExampleService
        {
            int Id { get; }
        }

        private class ExampleService : IExampleService
        {
            public int Id { get; set; }
        }

        private class WcfPerIntanceContextModule : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterType<ExampleService>()
                       .As<IExampleService>()
                       .SingleInstance();
            }
        }

        public class PerInstanceContextModuleAccessor : IPerInstanceContextModuleAccessor
        {
            public IEnumerable<IModule> Modules { get; set; }
        }

        private class DisposeTracker : Disposable
        {
            public bool IsDisposedPublic
            {
                get
                {
                    return this.IsDisposed;
                }
            }
        }
    }
}
