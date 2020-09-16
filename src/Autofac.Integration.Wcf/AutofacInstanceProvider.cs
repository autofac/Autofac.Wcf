// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Autofac.Integration.Wcf
{
    /// <summary>
    /// Retrieves service instances from an Autofac container.
    /// </summary>
    public class AutofacInstanceProvider : IInstanceProvider
    {
        private readonly ILifetimeScope _rootLifetimeScope;
        private readonly ServiceImplementationData _serviceData;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacInstanceProvider"/> class.
        /// </summary>
        /// <param name="rootLifetimeScope">
        /// The lifetime scope from which service instances should be resolved.
        /// </param>
        /// <param name="serviceData">
        /// Data object containing information about how to resolve the service
        /// implementation instance.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="rootLifetimeScope" /> or <paramref name="serviceData" /> is <see langword="null" />.
        /// </exception>
        public AutofacInstanceProvider(ILifetimeScope rootLifetimeScope, ServiceImplementationData serviceData)
        {
            _rootLifetimeScope = rootLifetimeScope ?? throw new ArgumentNullException(nameof(rootLifetimeScope));
            _serviceData = serviceData ?? throw new ArgumentNullException(nameof(serviceData));
        }

        /// <summary>
        /// Returns a service object given the specified <see cref="InstanceContext"/> object.
        /// </summary>
        /// <param name="instanceContext">The current <see cref="InstanceContext"/> object.</param>
        /// <returns>A user-defined service object.</returns>
        public object GetInstance(InstanceContext instanceContext) => GetInstance(instanceContext, null);

        /// <summary>
        /// Returns a service object given the specified <see cref="InstanceContext"/> object.
        /// </summary>
        /// <param name="instanceContext">The current <see cref="InstanceContext"/> object.</param>
        /// <param name="message">The message that triggered the creation of a service object.</param>
        /// <returns>The service object.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="instanceContext" /> is <see langword="null" />.
        /// </exception>
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Dispose ownership transferred to another object")]
        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            if (instanceContext == null)
            {
                throw new ArgumentNullException(nameof(instanceContext));
            }

            var autofacInstanceContext = new AutofacInstanceContext(_rootLifetimeScope);
            instanceContext.Extensions.Add(autofacInstanceContext);

            try
            {
                return autofacInstanceContext.Resolve(_serviceData);
            }
            catch (Exception)
            {
                autofacInstanceContext.Dispose();
                instanceContext.Extensions.Remove(autofacInstanceContext);
                throw;
            }
        }

        /// <summary>
        /// Called when an <see cref="InstanceContext"/> object recycles a service object.
        /// </summary>
        /// <param name="instanceContext">The service's instance context.</param>
        /// <param name="instance">The service object to be recycled.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="instanceContext" /> is <see langword="null" />.
        /// </exception>
        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            if (instanceContext == null)
            {
                throw new ArgumentNullException(nameof(instanceContext));
            }

            var extension = instanceContext.Extensions.Find<AutofacInstanceContext>();
            extension?.Dispose();
        }
    }
}