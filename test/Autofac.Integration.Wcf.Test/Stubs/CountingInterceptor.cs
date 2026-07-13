// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Castle.DynamicProxy;

namespace Autofac.Integration.Wcf.Test.Stubs;

/// <summary>
/// Interceptor that counts calls and then proceeds to the underlying
/// implementation. Used to prove interception still runs when a service is
/// hosted.
/// </summary>
public class CountingInterceptor : IInterceptor
{
    /// <summary>
    /// Gets the number of intercepted calls seen so far.
    /// </summary>
    public int CallCount
    {
        get;
        private set;
    }

    /// <inheritdoc/>
    public void Intercept(IInvocation invocation)
    {
        if (invocation == null)
        {
            throw new ArgumentNullException(nameof(invocation));
        }

        CallCount++;
        invocation.Proceed();
    }
}
