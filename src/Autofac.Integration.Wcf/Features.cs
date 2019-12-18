using System;

namespace Autofac.Integration.Wcf
{
    /// <summary>
    /// Feature flags.
    /// </summary>
    [Flags]
    public enum Features
    {
        /// <summary>
        /// No optional features defined.
        /// </summary>
        None = 0,
        /// <summary>
        /// Enables InstancePerContextModules, which allows for modules to be registered
        /// when an <see cref="AutofacInstanceContext"/> is instantiated.
        /// </summary>
        InstancePerContextModules = 1
        // Next will be 2, then 4, and so on in powers of two.
    }
}
