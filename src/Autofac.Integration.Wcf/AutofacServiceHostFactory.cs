// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceModel;

namespace Autofac.Integration.Wcf;

/// <summary>
/// Creates <see cref="ServiceHost"/> instances for WCF.
/// </summary>
public class AutofacServiceHostFactory : AutofacHostFactory
{
    /// <summary>
    /// Creates a <see cref="ServiceHost"/> for a specified type of service with a specific base address.
    /// </summary>
    /// <param name="serviceType">Specifies the type of service to host.</param>
    /// <param name="baseAddresses">The <see cref="Array"/> of type <see cref="Uri"/> that contains the base addresses for the service hosted.</param>
    /// <returns>
    /// A <see cref="ServiceHost"/> for the type of service specified with a specific base address.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="serviceType" /> or <paramref name="baseAddresses" /> is <see langword="null" />.
    /// </exception>
    protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
    {
        if (serviceType == null)
        {
            throw new ArgumentNullException(nameof(serviceType));
        }

        if (baseAddresses == null)
        {
            throw new ArgumentNullException(nameof(baseAddresses));
        }

        return new ServiceHost(serviceType, baseAddresses);
    }

    /// <summary>
    /// Creates a <see cref="ServiceHost"/> for a specified type of service with a specific base address.
    /// </summary>
    /// <param name="singletonInstance">Specifies the singleton service instance to host.</param>
    /// <param name="baseAddresses">The <see cref="Array"/> of type <see cref="Uri"/> that contains the base addresses for the service hosted.</param>
    /// <returns>
    /// A <see cref="ServiceHost"/> for the singleton service instance specified with a specific base address.
    /// </returns>
    protected override ServiceHost CreateSingletonServiceHost(object singletonInstance, Uri[] baseAddresses)
    {
        if (singletonInstance == null)
        {
            throw new ArgumentNullException(nameof(singletonInstance));
        }

        if (baseAddresses == null)
        {
            throw new ArgumentNullException(nameof(baseAddresses));
        }

        return new ServiceHost(singletonInstance, baseAddresses);
    }
}
