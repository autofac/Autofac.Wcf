// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Autofac.Integration.Wcf;

/// <summary>
/// Creates service host instances for WCF.
/// </summary>
/// <remarks>
/// <para>
/// You can change the strategy by which service implementations are resolved by
/// setting <see cref="ServiceImplementationDataProvider"/>. If it is
/// <see langword="null" />, a <see cref="DefaultServiceImplementationDataProvider"/>
/// is used.
/// </para>
/// <para>
/// Set <see cref="HostConfigurationAction"/> to configure additional behaviors or
/// other aspects of the generated host instances before they are returned.
/// </para>
/// </remarks>
public abstract class AutofacHostFactory : ServiceHostFactory
{
    /// <summary>
    /// Gets or sets the container or lifetime scope that service instances are
    /// resolved from.
    /// </summary>
    public static ILifetimeScope? Container
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets an action used to configure the service host instances this
    /// factory generates before they are returned.
    /// </summary>
    public static Action<ServiceHostBase>? HostConfigurationAction
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the strategy used to determine the service implementation
    /// for a given constructor string.
    /// </summary>
    public static IServiceImplementationDataProvider? ServiceImplementationDataProvider
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the <see cref="Wcf.Features"/> flags.
    /// </summary>
    public static Features Features
    {
        get; set;
    }

    /// <summary>
    /// Creates a <see cref="ServiceHost"/> with the specified base addresses and
    /// initializes it with the specified data.
    /// </summary>
    /// <param name="constructorString">
    /// The initialization data passed to the host being constructed.
    /// </param>
    /// <param name="baseAddresses">
    /// The base addresses for the hosted service.
    /// </param>
    /// <returns>
    /// A <see cref="ServiceHost"/> with the specified base addresses.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="constructorString" /> or
    /// <paramref name="baseAddresses"/> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="constructorString" /> is empty.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <see cref="Container"/> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    /// <para>
    /// If <see cref="HostConfigurationAction"/> is not <see langword="null" />, the
    /// new host is run through it before being returned.
    /// </para>
    /// </remarks>
    public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
    {
        if (constructorString == null)
        {
            throw new ArgumentNullException(nameof(constructorString));
        }

        if (constructorString.Length == 0)
        {
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ArgumentException_StringEmpty, nameof(constructorString)), nameof(constructorString));
        }

        if (Container == null)
        {
            throw new InvalidOperationException(AutofacHostFactoryResources.ContainerIsNull);
        }

        var dataProvider = ServiceImplementationDataProvider ?? new DefaultServiceImplementationDataProvider();

        var data = dataProvider.GetServiceImplementationData(constructorString);

        if (data.ServiceTypeToHost == null)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, AutofacHostFactoryResources.NoServiceTypeToHost, dataProvider.GetType(), constructorString));
        }

        if (!data.ServiceTypeToHost.IsClass)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, AutofacHostFactoryResources.ImplementationTypeUnknown, constructorString, data.ServiceTypeToHost));
        }

        ServiceHost host;
        if (data.HostAsSingleton)
        {
            var singletonInstance = data.ImplementationResolver!(Container);

            // Issue 31: an intercepted singleton resolves to a dynamic proxy whose
            // type both declares and inherits [ServiceContract], which WCF rejects.
            // Wrap it so WCF sees a type that only inherits the contract; calls
            // still forward to the resolved instance so interception runs.
            var instanceToHost = ContractForwardingProxy.WrapIfNecessary(singletonInstance, data.ServiceTypeToHost);
            host = CreateSingletonServiceHost(instanceToHost, baseAddresses);

            if (!ReferenceEquals(instanceToHost, singletonInstance))
            {
                // The proxy type does not carry the class-level [ServiceBehavior],
                // so re-assert Single mode, which hosting a supplied instance needs.
                // WCF always populates the description with a default
                // ServiceBehaviorAttribute, so Find<> is expected to return a
                // non-null instance here; the null-conditional is defensive only.
                host.Opening += (sender, args) =>
                {
                    var behavior = host.Description.Behaviors.Find<ServiceBehaviorAttribute>();
                    behavior?.InstanceContextMode = InstanceContextMode.Single;
                };
            }
        }
        else
        {
            host = CreateServiceHost(data.ServiceTypeToHost, baseAddresses);
            host.Opening += (sender, args) => host.Description.Behaviors.Add(new AutofacDependencyInjectionServiceBehavior(Container, data));
        }

        ApplyHostConfigurationAction(host);

        return host;
    }

    /// <summary>
    /// Creates a <see cref="ServiceHost"/> for a singleton service instance with
    /// the specified base addresses.
    /// </summary>
    /// <param name="singletonInstance">
    /// The singleton service instance to host.
    /// </param>
    /// <param name="baseAddresses">
    /// The base addresses for the hosted service.
    /// </param>
    /// <returns>
    /// A <see cref="ServiceHost"/> for the singleton instance.
    /// </returns>
    protected abstract ServiceHost CreateSingletonServiceHost(object singletonInstance, Uri[] baseAddresses);

    private static void ApplyHostConfigurationAction(ServiceHostBase host)
    {
        var action = HostConfigurationAction;
        action?.Invoke(host);
    }
}
