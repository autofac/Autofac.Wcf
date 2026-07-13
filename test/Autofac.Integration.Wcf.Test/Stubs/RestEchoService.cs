// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Integration.Wcf.Test.Stubs;

/// <summary>
/// REST echo service that proves constructor injection works through the
/// WebHttp hosting path.
/// </summary>
public class RestEchoService : IRestEchoService
{
    private readonly TrackedDependency _dependency;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestEchoService"/> class.
    /// </summary>
    /// <param name="dependency">
    /// The injected dependency whose ID is prefixed onto responses.
    /// </param>
    public RestEchoService(TrackedDependency dependency)
        => _dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));

    /// <inheritdoc/>
    public string Echo(string value) => _dependency.Id + ":" + value;
}
