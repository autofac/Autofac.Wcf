// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Threading;

namespace Autofac.Integration.Wcf.Test.Stubs;

/// <summary>
/// Thread-safe log of <see cref="TrackedDependency"/> construction and disposal.
/// Registered as a single instance so the service side and the test observe the
/// same data.
/// </summary>
public class DependencyActivity
{
    private readonly ConcurrentDictionary<string, bool> _disposed = new();
    private int _createdCount;

    /// <summary>
    /// Gets the number of dependencies created so far.
    /// </summary>
    public int CreatedCount => Volatile.Read(ref _createdCount);

    /// <summary>
    /// Gets the number of dependencies that have been disposed.
    /// </summary>
    public int DisposedCount => _disposed.Count(kvp => kvp.Value);

    /// <summary>
    /// Records the creation of a dependency and returns its new ID.
    /// </summary>
    /// <returns>
    /// A unique ID for the created dependency.
    /// </returns>
    public string Created()
    {
        var id = "dep-" + Interlocked.Increment(ref _createdCount);
        _disposed[id] = false;
        return id;
    }

    /// <summary>
    /// Records that the dependency with the given ID was disposed.
    /// </summary>
    /// <param name="id">
    /// The dependency ID.
    /// </param>
    public void Disposed(string id) => _disposed[id] = true;

    /// <summary>
    /// Determines whether the dependency with the given ID was disposed.
    /// </summary>
    /// <param name="id">
    /// The dependency ID.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if disposed; otherwise <see langword="false"/>.
    /// </returns>
    public bool WasDisposed(string id) => _disposed.TryGetValue(id, out var disposed) && disposed;
}
