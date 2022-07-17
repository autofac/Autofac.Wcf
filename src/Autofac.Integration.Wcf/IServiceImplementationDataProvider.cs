// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Integration.Wcf;

/// <summary>
/// Defines a strategy for resolving which service type should be
/// used for hosting vs. which type is the actual service implementation.
/// </summary>
public interface IServiceImplementationDataProvider
{
    /// <summary>
    /// Gets data about a service implementation.
    /// </summary>
    /// <param name="value">
    /// The constructor string passed in to the service host factory
    /// that is used to determine which type to host/use as a service
    /// implementation.
    /// </param>
    /// <returns>
    /// A <see cref="ServiceImplementationData"/>
    /// object containing information about which type to use in
    /// the service host and how to resolve the implementation.
    /// </returns>
    ServiceImplementationData GetServiceImplementationData(string value);
}
