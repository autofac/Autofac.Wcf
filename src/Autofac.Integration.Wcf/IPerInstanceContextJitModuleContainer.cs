using Autofac.Core;
using System.Collections.Generic;

namespace Autofac.Integration.Wcf
{
    /// <summary>
    /// A list of just in time (JIT) registrations. This allows for the
    /// current OperationContext or WebOperationContext or any item that is
    /// static per InstanceContext to be registered and usable throughout
    /// the instance context.
    /// </summary>
    public interface IPerInstanceContextJitModuleContainer
    {
        /// <summary>
        /// The list of Jit Modules.
        /// </summary>
        IEnumerable<IModule> Modules { get; }
    }
}
