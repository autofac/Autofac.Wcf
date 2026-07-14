// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceModel;
using Autofac.Integration.Wcf.Test.Stubs;

namespace Autofac.Integration.Wcf.Test;

public class ContractForwardingProxyFixture
{
    [Fact]
    public void Create_ForwardsCallsToTarget()
    {
        var target = new SingletonService();
        var proxy = (ISingletonService)ContractForwardingProxy.Create(typeof(ISingletonService), target);

        Assert.Equal(1, proxy.Increment());
        Assert.Equal(2, proxy.Increment());
    }

    [Fact]
    public void Create_ProxyImplementsContractWithoutDeclaringIt()
    {
        var target = new SingletonService();
        var proxy = ContractForwardingProxy.Create(typeof(ISingletonService), target);

        // Must implement the contract (so WCF can host it) but not declare its own
        // ServiceContract (the condition WCF rejects).
        Assert.IsAssignableFrom<ISingletonService>(proxy);
        Assert.Empty(proxy.GetType().GetCustomAttributes(typeof(ServiceContractAttribute), false));
    }

    [Fact]
    public void Create_RequiresContractInterface()
    {
        Assert.Throws<ArgumentNullException>(() => ContractForwardingProxy.Create(null!, new SingletonService()));
    }

    [Fact]
    public void Create_RequiresTarget()
    {
        Assert.Throws<ArgumentNullException>(() => ContractForwardingProxy.Create(typeof(ISingletonService), null!));
    }

    [Fact]
    public void WrapIfNecessary_LeavesCleanInstanceUnwrapped()
    {
        // A normal instance carries the contract on its interface, not the class,
        // so it is hosted directly.
        var instance = new SingletonService();
        var result = ContractForwardingProxy.WrapIfNecessary(instance, typeof(SingletonService));

        Assert.Same(instance, result);
    }

    [Fact]
    public void WrapIfNecessary_RequiresInstance()
    {
        Assert.Throws<ArgumentNullException>(() => ContractForwardingProxy.WrapIfNecessary(null!, typeof(SingletonService)));
    }

    [Fact]
    public void WrapIfNecessary_RequiresServiceType()
    {
        Assert.Throws<ArgumentNullException>(() => ContractForwardingProxy.WrapIfNecessary(new SingletonService(), null!));
    }

    [Fact]
    public void WrapIfNecessary_WrapsInstanceThatDeclaresAndInheritsContract()
    {
        // ConflictingContractProxy both declares and inherits ServiceContract,
        // the shape WCF rejects, so it must be wrapped.
        var instance = new ConflictingContractProxy();
        var result = ContractForwardingProxy.WrapIfNecessary(instance, typeof(SingletonService));

        Assert.NotSame(instance, result);
        Assert.IsAssignableFrom<ISingletonService>(result);
        Assert.Empty(result.GetType().GetCustomAttributes(typeof(ServiceContractAttribute), false));

        // Calls still reach the wrapped instance.
        Assert.Equal(1, ((ISingletonService)result).Increment());
    }
}
