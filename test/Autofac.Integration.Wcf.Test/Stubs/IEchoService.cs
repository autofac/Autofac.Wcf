// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceModel;

namespace Autofac.Integration.Wcf.Test.Stubs;

/// <summary>
/// General-purpose service contract for hosting and pipeline tests. Operations
/// expose enough state to assert lifetime, dependency injection, and instance
/// identity.
/// </summary>
[ServiceContract]
public interface IEchoService
{
    /// <summary>
    /// Returns the ID of the injected dependency, proving constructor injection
    /// reached the live service instance.
    /// </summary>
    /// <returns>
    /// The injected dependency's identifier.
    /// </returns>
    [OperationContract]
    string GetDependencyId();

    /// <summary>
    /// Returns a per-instance identifier so callers can tell whether successive
    /// calls hit the same service instance.
    /// </summary>
    /// <returns>
    /// The service instance identifier.
    /// </returns>
    [OperationContract]
    string GetInstanceId();

    /// <summary>
    /// Echoes the supplied message back to the caller.
    /// </summary>
    /// <param name="message">
    /// The message to echo.
    /// </param>
    /// <returns>
    /// The same message.
    /// </returns>
    [OperationContract]
    string Echo(string message);
}
