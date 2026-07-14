// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceModel;

namespace Autofac.Integration.Wcf.Test.Stubs;

/// <summary>
/// Stateful service contract used to verify singleton hosting and interception.
/// </summary>
[ServiceContract]
public interface ISingletonService
{
    /// <summary>
    /// Increments and returns a running counter, so callers can observe whether
    /// the same instance answers successive calls.
    /// </summary>
    /// <returns>
    /// The incremented counter value.
    /// </returns>
    [OperationContract]
    int Increment();
}
