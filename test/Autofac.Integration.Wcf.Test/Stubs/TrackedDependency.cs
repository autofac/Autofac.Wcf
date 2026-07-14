// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Integration.Wcf.Test.Stubs;

/// <summary>
/// A disposable dependency whose construction and disposal are recorded on a
/// shared <see cref="DependencyActivity"/>, letting tests observe how many
/// instances were created and disposed across service invocations.
/// </summary>
public class TrackedDependency : IDisposable
{
    private readonly DependencyActivity _activity;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackedDependency"/> class.
    /// </summary>
    /// <param name="activity">
    /// The shared activity log this dependency records to.
    /// </param>
    public TrackedDependency(DependencyActivity activity)
    {
        _activity = activity ?? throw new ArgumentNullException(nameof(activity));
        Id = _activity.Created();
    }

    /// <summary>
    /// Gets the unique ID assigned to this instance at construction.
    /// </summary>
    public string Id
    {
        get;
    }

    /// <inheritdoc/>
    public void Dispose() => _activity.Disposed(Id);
}
