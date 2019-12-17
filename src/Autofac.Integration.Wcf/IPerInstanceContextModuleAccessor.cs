using Autofac.Core;
using System.Collections.Generic;

namespace Autofac.Integration.Wcf
{
    /// <summary>
    /// A list of module registrations. This allows for the current
    /// <see cref="System.ServiceModel.OperationContext"/>
    /// or <see cref="System.ServiceModel.Web.WebOperationContext"/>
    /// or any item that is static per
    /// <see cref="System.ServiceModel.InstanceContext"/>
    /// to be registered and usable throughout the instance context.
    /// </summary>
    public interface IPerInstanceContextModuleAccessor
    {
        /// <summary>
        /// Gets the list of per-instance-context modules to register.
        /// </summary>
        IEnumerable<IModule> Modules { get; }
    }
}