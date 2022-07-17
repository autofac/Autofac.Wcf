// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceModel;
using System.ServiceModel.Description;

namespace Autofac.Integration.Wcf.Test;

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
        protected override ServiceDescription CreateDescription(out IDictionary<string, ContractDescription> implementedContracts) =>
            throw new NotImplementedException();
    }
}
