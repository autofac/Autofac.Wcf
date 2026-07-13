// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceModel;

namespace Autofac.Integration.Wcf.Test.Stubs;

/// <summary>
/// Stands in for a Castle interface-interception proxy: it implements a
/// <see cref="ServiceContractAttribute"/>-marked interface and also declares the
/// attribute on itself. WCF rejects such a type, so it exercises the
/// wrapping logic without taking a Castle dependency.
/// </summary>
[ServiceContract]
public class ConflictingContractProxy : ISingletonService
{
    private int _count;

    /// <inheritdoc/>
    [OperationContract]
    public int Increment() => ++_count;
}
