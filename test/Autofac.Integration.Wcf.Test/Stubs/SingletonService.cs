// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceModel;

namespace Autofac.Integration.Wcf.Test.Stubs;

/// <summary>
/// Singleton service implementation used to verify singleton hosting, including
/// the interception scenario where the resolved instance is a dynamic proxy.
/// </summary>
[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
public class SingletonService : ISingletonService
{
    private int _count;

    // Virtual so the same type also works with class interceptors; interface
    // interception does not require it.
    public virtual int Increment() => ++_count;
}
