// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;

namespace Autofac.Integration.Wcf.Test.Stubs;

/// <summary>
/// Base <see cref="IEchoService"/> implementation that takes a
/// <see cref="TrackedDependency"/> so tests can assert constructor injection and
/// lifetime. Each instance gets a unique ID.
/// </summary>
public abstract class EchoServiceBase : IEchoService
{
    private static int _instanceCounter;

    private readonly TrackedDependency _dependency;
    private readonly string _instanceId;

    /// <summary>
    /// Initializes a new instance of the <see cref="EchoServiceBase"/> class.
    /// </summary>
    /// <param name="dependency">
    /// The injected dependency whose ID is exposed via <see cref="GetDependencyId"/>.
    /// </param>
    protected EchoServiceBase(TrackedDependency dependency)
    {
        _dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));
        _instanceId = "svc-" + Interlocked.Increment(ref _instanceCounter);
    }

    /// <inheritdoc/>
    public string GetDependencyId() => _dependency.Id;

    /// <inheritdoc/>
    public string GetInstanceId() => _instanceId;

    /// <inheritdoc/>
    public string Echo(string message) => message;
}
