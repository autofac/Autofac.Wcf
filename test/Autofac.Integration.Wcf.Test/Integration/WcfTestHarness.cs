// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceModel;

namespace Autofac.Integration.Wcf.Test.Integration;

/// <summary>
/// Helpers for standing up a real WCF <see cref="ServiceHost"/> over a
/// <see cref="NetNamedPipeBinding"/>, invoking it through a client channel, and
/// tearing everything down.
/// </summary>
/// <remarks>
/// <para>
/// These helpers exercise the full WCF client/dispatcher/instance-provider
/// pipeline. They run on Windows but not under Mono/macOS, where named-pipe
/// hosting is unsupported.
/// </para>
/// </remarks>
internal static class WcfTestHarness
{
    /// <summary>
    /// Creates a unique base address so concurrent or repeated runs do not
    /// collide on a pipe name.
    /// </summary>
    /// <returns>
    /// A unique <c>net.pipe</c> base address.
    /// </returns>
    public static Uri CreateBaseAddress()
        => new("net.pipe://localhost/autofac-wcf-test/" + Guid.NewGuid().ToString("N"));

    /// <summary>
    /// Hosts a service at a named-pipe endpoint, opens it, invokes the given
    /// client interaction, then tears down the channel and host.
    /// </summary>
    /// <typeparam name="TContract">
    /// The service contract interface.
    /// </typeparam>
    /// <param name="serviceHost">
    /// The host to open. Disposed by this method.
    /// </param>
    /// <param name="baseAddress">
    /// The base address the host was created with.
    /// </param>
    /// <param name="act">
    /// The client interaction to run against the open host.
    /// </param>
    public static void HostAndInvoke<TContract>(ServiceHost serviceHost, Uri baseAddress, Action<TContract> act)
        where TContract : class
    {
        var binding = new NetNamedPipeBinding();
        var address = new Uri(baseAddress, "service");
        serviceHost.AddServiceEndpoint(typeof(TContract), binding, address);
        serviceHost.Open();

        try
        {
            var channelFactory = new ChannelFactory<TContract>(binding, new EndpointAddress(address));
            try
            {
                var channel = channelFactory.CreateChannel();
                var succeeded = false;
                try
                {
                    act(channel);
                    ((IClientChannel)(object)channel).Close();
                    succeeded = true;
                }
                finally
                {
                    if (!succeeded)
                    {
                        ((IClientChannel)(object)channel).Abort();
                    }
                }
            }
            finally
            {
                ((IDisposable)channelFactory).Dispose();
            }
        }
        finally
        {
            serviceHost.Close();
        }
    }

    /// <summary>
    /// Runs a test body with the given container installed as the global
    /// <see cref="AutofacHostFactory.Container"/>, restoring the previous state
    /// afterward.
    /// </summary>
    /// <param name="container">
    /// The container to install.
    /// </param>
    /// <param name="test">
    /// The test body to run.
    /// </param>
    public static void WithHostedContainer(IContainer container, Action test)
    {
        AutofacHostFactory.Container = container;
        try
        {
            test();
        }
        finally
        {
            AutofacHostFactory.Container = null;
        }
    }
}
