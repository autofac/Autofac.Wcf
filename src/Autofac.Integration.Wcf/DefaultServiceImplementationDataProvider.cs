// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using Autofac.Core;
using Autofac.Core.Lifetime;

namespace Autofac.Integration.Wcf
{
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
        /// The constructor string passed in to the service host factory
        /// that is used to determine which type to host/use as a service
        /// implementation.
        /// </param>
        /// <returns>
        /// A <see cref="ServiceImplementationData"/>
        /// object containing information about which type to use in
        /// the service host and which type to use to resolve the implementation.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This resolver takes the constructor string stored in the .svc file
        /// and resolves a matching keyed or typed service from the root
        /// application container. That resolved type is used both for the
        /// service host as well as the implementation type.
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

            ServiceRegistration serviceRegistration;
            if (!AutofacHostFactory.Container.ComponentRegistry.TryGetServiceRegistration(serviceBeingResolved, out serviceRegistration))
            {
                var serviceType = Type.GetType(value, false);
                if (serviceType != null)
                {
                    serviceBeingResolved = new TypedService(serviceType);
                    AutofacHostFactory.Container.ComponentRegistry.TryGetServiceRegistration(serviceBeingResolved, out serviceRegistration);
                }
            }

            if (serviceRegistration == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, AutofacHostFactoryResources.ServiceNotRegistered, value));
            }

            var data = new ServiceImplementationData
            {
                ConstructorString = value,
                ServiceTypeToHost = serviceRegistration.Registration.Activator.LimitType,
                ImplementationResolver = l => l.ResolveComponent(new ResolveRequest(serviceBeingResolved, serviceRegistration, Enumerable.Empty<Parameter>()))
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

        private static bool IsRegistrationSingleInstance(IComponentRegistration registration) =>
            registration.Sharing == InstanceSharing.Shared && registration.Lifetime is RootScopeLifetime;

        private static bool IsSingletonWcfService(Type implementationType)
        {
            var behavior = implementationType
                .GetCustomAttributes(typeof(ServiceBehaviorAttribute), true)
                .OfType<ServiceBehaviorAttribute>()
                .FirstOrDefault();

            return behavior != null && behavior.InstanceContextMode == InstanceContextMode.Single;
        }
    }
}
