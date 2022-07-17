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
/// The Autofac service host factory allows you to change
/// the strategy by which service implementations are resolved. You do this by
/// setting the <see cref="ServiceImplementationDataProvider"/>
/// with a strategy implementation.
/// </para>
/// <para>
/// If <see cref="ServiceImplementationDataProvider"/>
/// is <see langword="null" /> a new instance of <see cref="DefaultServiceImplementationDataProvider"/>
/// will be used.
/// </para>
/// <para>
/// You may configure additional behaviors or other aspects of generated
/// service instances by setting the <see cref="HostConfigurationAction"/>.
/// If this value is not <see langword="null" />, generated host instances
/// will be run through that action.
/// </para>
/// </remarks>
public abstract class AutofacHostFactory : ServiceHostFactory
{
    /// <summary>
    /// Gets or sets the container or lifetime scope from which service instances will be retrieved.
    /// </summary>
    /// <value>
    /// An <see cref="ILifetimeScope"/> that will be used to resolve service
    /// implementation instances.
    /// </value>
    public static ILifetimeScope? Container { get; set; }

    /// <summary>
    /// Gets or sets an action that can be used to programmatically configure
    /// service host instances this factory generates.
    /// </summary>
    /// <value>
    /// An <see cref="Action{T}"/> that can be used to configure service host
    /// instances that this factory creates. This action can be used to add
    /// behaviors or otherwise modify the host before it gets returned by
    /// the factory.
    /// </value>
    public static Action<ServiceHostBase>? HostConfigurationAction { get; set; }

    /// <summary>
    /// Gets or sets the service implementation data strategy.
    /// </summary>
    /// <value>
    /// An <see cref="IServiceImplementationDataProvider"/>
    /// that will be used to determine the proper service implementation given
    /// a service constructor string.
    /// </value>
    public static IServiceImplementationDataProvider? ServiceImplementationDataProvider { get; set; }

    /// <summary>
    /// Gets or sets <see cref="Wcf.Features"/> flags.
    /// </summary>
    public static Features Features { get; set; }

    /// <summary>
    /// Creates a <see cref="ServiceHost"/> with specific base addresses and initializes it with specified data.
    /// </summary>
    /// <param name="constructorString">The initialization data passed to the <see cref="ServiceHostBase"/> instance being constructed by the factory.</param>
    /// <param name="baseAddresses">The <see cref="Array"/> of type <see cref="Uri"/> that contains the base addresses for the service hosted.</param>
    /// <returns>
    /// A <see cref="ServiceHost"/> with specific base addresses.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="constructorString" /> or <paramref name="baseAddresses"/> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="constructorString" /> is empty.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the <see cref="Container"/>
    /// is <see langword="null" />.
    /// </exception>
    /// <remarks>
    /// <para>
    /// If <see cref="HostConfigurationAction"/>
    /// is not <see langword="null" />, the new service host instance is run
    /// through the configuration action prior to being returned. This allows
    /// you to programmatically configure behaviors or other aspects of the
    /// host.
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
            host = CreateSingletonServiceHost(singletonInstance, baseAddresses);
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
    /// Creates a <see cref="ServiceHost"/> for a specified type of service with a specific base address.
    /// </summary>
    /// <param name="singletonInstance">Specifies the singleton service instance to host.</param>
    /// <param name="baseAddresses">The <see cref="Array"/> of type <see cref="Uri"/> that contains the base addresses for the service hosted.</param>
    /// <returns>
    /// A <see cref="ServiceHost"/> for the singleton service instance specified with a specific base address.
    /// </returns>
    protected abstract ServiceHost CreateSingletonServiceHost(object singletonInstance, Uri[] baseAddresses);

    private static void ApplyHostConfigurationAction(ServiceHostBase host)
    {
        var action = HostConfigurationAction;
        action?.Invoke(host);
    }
}
