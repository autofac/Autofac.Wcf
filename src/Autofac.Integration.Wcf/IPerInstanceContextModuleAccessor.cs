// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceModel;
using System.ServiceModel.Web;
using Autofac.Core;

namespace Autofac.Integration.Wcf;

/// <summary>
/// A list of module registrations. This allows for the current
/// <see cref="OperationContext"/> or <see cref="WebOperationContext"/>
/// or any item that is static per <see cref="InstanceContext"/>
/// to be registered and usable throughout the instance context.
/// </summary>
public interface IPerInstanceContextModuleAccessor
{
    /// <summary>
    /// Gets the list of per-instance-context modules to register.
    /// </summary>
    IEnumerable<IModule> Modules { get; }
}
