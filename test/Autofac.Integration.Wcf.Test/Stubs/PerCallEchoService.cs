// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceModel;

namespace Autofac.Integration.Wcf.Test.Stubs;

/// <summary>
/// Per-call echo service. WCF creates a fresh instance context, and thus a
/// fresh Autofac lifetime scope, for every operation.
/// </summary>
[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
public class PerCallEchoService : EchoServiceBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PerCallEchoService"/> class.
    /// </summary>
    /// <param name="dependency">
    /// The injected dependency.
    /// </param>
    public PerCallEchoService(TrackedDependency dependency)
        : base(dependency)
    {
    }
}
