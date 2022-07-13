// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using System.ServiceModel;
using Autofac.Core;

namespace Autofac.Integration.Wcf
{
    /// <summary>
    /// Adds dependency injection related methods to service hosts.
    /// </summary>
    public static class ServiceHostExtensions
    {
        /// <summary>
        /// Adds the custom service behavior required for dependency injection.
        /// </summary>
        /// <typeparam name="T">The web service contract type.</typeparam>
        /// <param name="serviceHost">The service host.</param>
        /// <param name="container">The container.</param>
        public static void AddDependencyInjectionBehavior<T>(this ServiceHostBase serviceHost, ILifetimeScope container) =>
            AddDependencyInjectionBehavior(serviceHost, typeof(T), container);

        /// <summary>
        /// Adds the custom service behavior required for dependency injection.
        /// </summary>
        /// <param name="serviceHost">The service host.</param>
        /// <param name="contractType">The web service contract type.</param>
        /// <param name="container">The container.</param>
        public static void AddDependencyInjectionBehavior(this ServiceHostBase serviceHost, Type contractType, ILifetimeScope container) =>
            AddDependencyInjectionBehavior(serviceHost, contractType, container, Enumerable.Empty<Parameter>());

        /// <summary>
        /// Adds the custom service behavior required for dependency injection.
        /// </summary>
        /// <typeparam name="T">The web service contract type.</typeparam>
        /// <param name="serviceHost">The service host.</param>
        /// <param name="container">The container.</param>
        /// <param name="parameters">Parameters for the instance.</param>
        public static void AddDependencyInjectionBehavior<T>(this ServiceHostBase serviceHost, ILifetimeScope container, IEnumerable<Parameter> parameters) =>
            AddDependencyInjectionBehavior(serviceHost, typeof(T), container, parameters);

        /// <summary>
        /// Adds the custom service behavior required for dependency injection.
        /// </summary>
        /// <param name="serviceHost">The service host.</param>
        /// <param name="contractType">The web service contract type.</param>
        /// <param name="container">The container.</param>
        /// <param name="parameters">Parameters for the instance.</param>
        public static void AddDependencyInjectionBehavior(this ServiceHostBase serviceHost, Type contractType, ILifetimeScope container, IEnumerable<Parameter> parameters)
        {
            if (serviceHost == null)
            {
                throw new ArgumentNullException(nameof(serviceHost));
            }

            if (contractType == null)
            {
                throw new ArgumentNullException(nameof(contractType));
            }

            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var serviceBehavior = serviceHost.Description.Behaviors.Find<ServiceBehaviorAttribute>();
            if (serviceBehavior != null && serviceBehavior.InstanceContextMode == InstanceContextMode.Single)
            {
                return;
            }

            var serviceToResolve = new TypedService(contractType);
            if (!container.ComponentRegistry.TryGetServiceRegistration(serviceToResolve, out ServiceRegistration serviceRegistration))
            {
                var message = string.Format(CultureInfo.CurrentCulture, ServiceHostExtensionsResources.ContractTypeNotRegistered, contractType.FullName);
                throw new ArgumentException(message, nameof(contractType));
            }

            var data = new ServiceImplementationData
            {
                ConstructorString = contractType.AssemblyQualifiedName,
                ServiceTypeToHost = contractType,
                ImplementationResolver = l => l.ResolveComponent(new ResolveRequest(serviceToResolve, serviceRegistration, parameters)),
            };

            var behavior = new AutofacDependencyInjectionServiceBehavior(container, data);
            serviceHost.Description.Behaviors.Add(behavior);
        }
    }
}
