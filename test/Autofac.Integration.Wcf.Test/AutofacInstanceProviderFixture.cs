// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using Autofac.Core;
using Autofac.Util;

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
            var instance = new object();
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
            public override MessageHeaders Headers => throw new NotImplementedException();

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer) => throw new NotImplementedException();

            public override MessageProperties Properties => throw new NotImplementedException();

            public override MessageVersion Version => throw new NotImplementedException();
        }

        internal class BadService
        {
            public BadService(Disposable item) => throw new Exception("Boom!!!");
        }
    }
}
