// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using System.ServiceModel;
using Autofac.Core;
using Autofac.Core.Lifetime;

namespace Autofac.Integration.Wcf;

/// <summary>
/// Simple resolver for WCF service implementations. Allows for single-tenant
/// handling of named or typed services.
/// </summary>
public class DefaultServiceImplementationDataProvider : IServiceImplementationDataProvider
{
    /// <summary>
    /// Gets data about a service implementation.
    /// </summary>
    /// <param name="value">
    /// The constructor string from the service host factory, identifying
    /// the service type to host.
    /// </param>
    /// <returns>
    /// A <see cref="ServiceImplementationData"/> with the service host
    /// type and implementation resolver.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This resolver takes the constructor string stored in the .svc file
    /// and resolves a matching keyed or typed service from the root
    /// application container. That resolved type is used for both the
    /// service host and the implementation type.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the <see cref="AutofacHostFactory.Container"/>
    /// is <see langword="null" />;
    /// if the service indicated by <paramref name="value" />
    /// is not registered with the <see cref="AutofacHostFactory.Container"/>;
    /// or if the service is a singleton that isn't registered as a singleton.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="value" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="value" /> is empty.
    /// </exception>
    public virtual ServiceImplementationData GetServiceImplementationData(string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (value.Length == 0)
        {
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ArgumentException_StringEmpty, nameof(value)));
        }

        if (AutofacHostFactory.Container == null)
        {
            throw new InvalidOperationException(AutofacHostFactoryResources.ContainerIsNull);
        }

        Service serviceBeingResolved = new KeyedService(value, typeof(object));
        if (!AutofacHostFactory.Container.ComponentRegistry.TryGetServiceRegistration(serviceBeingResolved, out var serviceRegistration))
        {
            var serviceType = Type.GetType(value, false);
            if (serviceType != null)
            {
                serviceBeingResolved = new TypedService(serviceType);
                AutofacHostFactory.Container.ComponentRegistry.TryGetServiceRegistration(serviceBeingResolved, out serviceRegistration);
            }
        }

        if (serviceRegistration == default)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, AutofacHostFactoryResources.ServiceNotRegistered, value));
        }

        var data = new ServiceImplementationData
        {
            ConstructorString = value,
            ServiceTypeToHost = serviceRegistration.Registration.Activator.LimitType,
            ImplementationResolver = l => l.ResolveComponent(new ResolveRequest(serviceBeingResolved, serviceRegistration, Enumerable.Empty<Parameter>())),
        };

        var implementationType = serviceRegistration.Registration.Activator.LimitType;
        if (IsSingletonWcfService(implementationType))
        {
            if (!IsRegistrationSingleInstance(serviceRegistration.Registration))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, AutofacHostFactoryResources.ServiceMustBeSingleInstance, implementationType.FullName));
            }

            data.HostAsSingleton = true;
        }
        else
        {
            if (IsRegistrationSingleInstance(serviceRegistration.Registration))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, AutofacHostFactoryResources.ServiceMustNotBeSingleInstance, implementationType.FullName));
            }
        }

        return data;
    }

    private static bool IsRegistrationSingleInstance(IComponentRegistration registration)
        => registration.Sharing == InstanceSharing.Shared && registration.Lifetime is RootScopeLifetime;

    private static bool IsSingletonWcfService(Type implementationType)
    {
        var behavior = implementationType
            .GetCustomAttributes(typeof(ServiceBehaviorAttribute), true)
            .OfType<ServiceBehaviorAttribute>()
            .FirstOrDefault();

        return behavior != null && behavior.InstanceContextMode == InstanceContextMode.Single;
    }
}
