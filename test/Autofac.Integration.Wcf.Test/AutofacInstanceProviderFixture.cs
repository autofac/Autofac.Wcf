using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Autofac.Core;
using Autofac.Util;
using Xunit;

namespace Autofac.Integration.Wcf.Test
{
    public class AutofacInstanceProviderFixture
    {
        [Fact]
        public void Ctor_RequiresContainer()
        {
            var data = new ServiceImplementationData();
            Assert.Throws<ArgumentNullException>(() => new AutofacInstanceProvider(null, data));
        }

        [Fact]
        public void Ctor_RequiresServiceImplementationData()
        {
            var container = new ContainerBuilder().Build();
            Assert.Throws<ArgumentNullException>(() => new AutofacInstanceProvider(container, null));
        }

        [Fact]
        public void GetInstance_NullInstanceContext()
        {
            var data = new ServiceImplementationData();
            var container = new ContainerBuilder().Build();
            var provider = new AutofacInstanceProvider(container, data);
            var message = new TestMessage();
            Assert.Throws<ArgumentNullException>(() => provider.GetInstance(null, message));
        }

        [Fact]
        public void ReleaseInstance_NullInstanceContext()
        {
            var data = new ServiceImplementationData();
            var container = new ContainerBuilder().Build();
            var provider = new AutofacInstanceProvider(container, data);
            object instance = new object();
            Assert.Throws<ArgumentNullException>(() => provider.ReleaseInstance(null, instance));
        }

        [Fact]
        public void LifetimeScopeDisposedWhenExceptionThrownInServiceConstructor()
        {
            var builder = new ContainerBuilder();
            var released = false;
            builder.RegisterType<Disposable>().OnRelease(d => released = true);
            builder.RegisterType<BadService>();
            var container = builder.Build();
            var data = new ServiceImplementationData { ImplementationResolver = l => l.Resolve<BadService>() };
            var provider = new AutofacInstanceProvider(container, data);
            var context = new InstanceContext(new object());

            Assert.Throws<DependencyResolutionException>(() => provider.GetInstance(context));

            Assert.True(released);
        }

        private class TestMessage : Message
        {
            public override MessageHeaders Headers
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            protected override void OnWriteBodyContents(System.Xml.XmlDictionaryWriter writer)
            {
                throw new NotImplementedException();
            }

            public override MessageProperties Properties
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override MessageVersion Version
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        class BadService
        {
            // ReSharper disable once UnusedParameter.Local
            public BadService(Disposable disposable)
            {
                throw new Exception("Boom!!!");
            }
        }
    }
}
