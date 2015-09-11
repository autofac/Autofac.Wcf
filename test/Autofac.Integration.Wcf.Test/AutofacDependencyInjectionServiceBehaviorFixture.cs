using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using Xunit;

namespace Autofac.Integration.Wcf.Test
{
    public class AutofacDependencyInjectionServiceBehaviorFixture
    {
        [Fact]
        public void Ctor_RequiresContainer()
        {
            var data = new ServiceImplementationData();
            Assert.Throws<ArgumentNullException>(() => new AutofacDependencyInjectionServiceBehavior(null, data));
        }

        [Fact]
        public void Ctor_RequiresServiceImplementationData()
        {
            var container = new ContainerBuilder().Build();
            Assert.Throws<ArgumentNullException>(() => new AutofacDependencyInjectionServiceBehavior(container, null));
        }

        [Fact]
        public void ApplyDispatchBehavior_NullServiceDescription()
        {
            var provider = new AutofacDependencyInjectionServiceBehavior(new ContainerBuilder().Build(), new ServiceImplementationData());
            var host = new TestHost();
            Assert.Throws<ArgumentNullException>(() => provider.ApplyDispatchBehavior(null, host));
        }

        [Fact]
        public void ApplyDispatchBehavior_NullServiceHost()
        {
            var provider = new AutofacDependencyInjectionServiceBehavior(new ContainerBuilder().Build(), new ServiceImplementationData());
            var description = new ServiceDescription();
            Assert.Throws<ArgumentNullException>(() => provider.ApplyDispatchBehavior(description, null));
        }

        private class TestHost : ServiceHostBase
        {
            protected override System.ServiceModel.Description.ServiceDescription CreateDescription(out IDictionary<string, System.ServiceModel.Description.ContractDescription> implementedContracts)
            {
                throw new NotImplementedException();
            }
        }
    }
}
