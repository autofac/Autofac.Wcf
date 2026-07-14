// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;

namespace Autofac.Integration.Wcf.Test;

/// <summary>
/// A <see cref="FactAttribute"/> that only runs on Windows.
/// </summary>
/// <remarks>
/// <para>
/// WCF self-hosting (named pipes, full ServiceHost pipeline) is unavailable
/// under Mono/.NET on macOS and Linux, so end-to-end hosting tests are skipped
/// off Windows while still running on CI.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class WindowsFactAttribute : FactAttribute
{
    public WindowsFactAttribute()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Skip = "WCF self-hosting is only supported on Windows.";
        }
    }
}
