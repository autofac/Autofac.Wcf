// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Autofac.Integration.Wcf
{
    /// <summary>
    /// Feature flags.
    /// </summary>
    [Flags]
#pragma warning disable CA1724
    public enum Features
#pragma warning restore CA1724
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
