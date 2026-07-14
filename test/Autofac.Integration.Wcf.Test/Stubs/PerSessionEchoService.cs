// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceModel;

namespace Autofac.Integration.Wcf.Test.Stubs;

/// <summary>
/// Per-session echo service (the WCF default). One instance serves all calls on
/// a single client channel.
/// </summary>
[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
public class PerSessionEchoService : EchoServiceBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PerSessionEchoService"/> class.
    /// </summary>
    /// <param name="dependency">
    /// The injected dependency.
    /// </param>
    public PerSessionEchoService(TrackedDependency dependency)
        : base(dependency)
    {
    }
}
