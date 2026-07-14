// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceModel;

namespace Autofac.Integration.Wcf.Test.Stubs;

/// <summary>
/// Singleton echo service. A single instance serves every call across all
/// channels for the life of the host.
/// </summary>
[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
public class SingletonEchoService : EchoServiceBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SingletonEchoService"/> class.
    /// </summary>
    /// <param name="dependency">
    /// The injected dependency.
    /// </param>
    public SingletonEchoService(TrackedDependency dependency)
        : base(dependency)
    {
    }
}
