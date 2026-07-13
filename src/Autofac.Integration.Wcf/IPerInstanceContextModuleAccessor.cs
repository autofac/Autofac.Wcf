// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceModel;
using System.ServiceModel.Web;
using Autofac.Core;

namespace Autofac.Integration.Wcf;

/// <summary>
/// Provides Autofac modules to register per <see cref="InstanceContext"/>.
/// </summary>
/// <remarks>
/// <para>
/// Implement this interface to supply modules that are registered when
/// an instance context is created, giving access to context-static items
/// such as <see cref="OperationContext"/> or
/// <see cref="WebOperationContext"/> throughout the instance context.
/// </para>
/// </remarks>
public interface IPerInstanceContextModuleAccessor
{
    /// <summary>
    /// Gets the list of per-instance-context modules to register.
    /// </summary>
    IEnumerable<IModule> Modules
    {
        get;
    }
}
