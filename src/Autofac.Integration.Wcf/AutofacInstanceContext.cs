// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceModel;
using Autofac.Core;

namespace Autofac.Integration.Wcf;

/// <summary>
/// Manages instance lifecycle using an Autofac inner container.
/// </summary>
/// <remarks>
/// <para>
/// This instance context extension creates a child lifetime scope based
/// on a scope provided and resolves service instances from that child scope.
/// </para>
/// <para>
/// When this instance context is disposed, the lifetime scope it creates
/// (which contains the resolved service instance) is also disposed.
/// </para>
/// </remarks>
public class AutofacInstanceContext : IExtension<InstanceContext>, IDisposable, IComponentContext
{
    private bool _disposed;

    /// <summary>
    /// Gets the current <see cref="AutofacInstanceContext"/>
    /// for the operation.
    /// </summary>
    /// <value>
    /// The <see cref="AutofacInstanceContext"/> associated
    /// with the current <see cref="OperationContext"/> if
    /// one exists; or <see langword="null" /> if there isn't one.
    /// </value>
    /// <remarks>
    /// <para>
    /// In a singleton service, there won't be a current <see cref="AutofacInstanceContext"/>
    /// because singleton services are resolved at the time the service host begins
    /// rather than on each operation.
    /// </para>
    /// </remarks>
    public static AutofacInstanceContext? Current
    {
        get
        {
            var operationContext = OperationContext.Current;
            var instanceContext = operationContext?.InstanceContext;
            return instanceContext?.Extensions.Find<AutofacInstanceContext>();
        }
    }

    /// <summary>
    /// Gets the request/operation lifetime.
    /// </summary>
    /// <value>
    /// An <see cref="ILifetimeScope"/> that this instance
    /// context will use to resolve service instances.
    /// </value>
    public ILifetimeScope OperationLifetime { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AutofacInstanceContext"/> class.
    /// </summary>
    /// <param name="container">
    /// The outer container/lifetime scope from which the instance scope
    /// will be created.
    /// </param>
    public AutofacInstanceContext(ILifetimeScope container)
    {
        if (container == null)
        {
            throw new ArgumentNullException(nameof(container));
        }

        OperationLifetime = !AutofacHostFactory.Features.HasFlag(Features.InstancePerContextModules)
            ? container.BeginLifetimeScope()
            : container.BeginLifetimeScope(builder =>
                {
                    var modules = container.ResolveOptional<IPerInstanceContextModuleAccessor>();
                    if (modules?.Modules != null && modules.Modules.Any())
                    {
                        foreach (var module in modules.Modules)
                        {
                            builder.RegisterModule(module);
                        }
                    }
                });
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="AutofacInstanceContext"/> class.
    /// </summary>
    ~AutofacInstanceContext() => Dispose(false);

    /// <summary>
    /// Enables an extension object to find out when it has been aggregated.
    /// Called when the extension is added to the
    /// <see cref="IExtensibleObject{T}.Extensions"/> property.
    /// </summary>
    /// <param name="owner">The extensible object that aggregates this extension.</param>
    public void Attach(InstanceContext owner)
    {
    }

    /// <summary>
    /// Enables an object to find out when it is no longer aggregated.
    /// Called when an extension is removed from the
    /// <see cref="IExtensibleObject{T}.Extensions"/> property.
    /// </summary>
    /// <param name="owner">The extensible object that aggregates this extension.</param>
    public void Detach(InstanceContext owner)
    {
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or
    /// resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Handles disposal of managed and unmanaged resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true" /> to dispose of managed resources (during a manual execution
    /// of <see cref="Dispose()"/>); or
    /// <see langword="false" /> if this is getting run as part of finalization where
    /// managed resources may have already been cleaned up.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Free managed resources
                OperationLifetime.Dispose();
            }

            _disposed = true;
        }
    }

    /// <inheritdoc />
    public IComponentRegistry ComponentRegistry => OperationLifetime.ComponentRegistry;

    /// <inheritdoc />
    public object ResolveComponent(in ResolveRequest request) => OperationLifetime.ResolveComponent(request);

    /// <summary>
    /// Retrieve a service instance from the context.
    /// </summary>
    /// <param name="serviceData">
    /// Data object containing information about how to resolve the service
    /// implementation instance.
    /// </param>
    /// <returns>The service instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="serviceData" /> is <see langword="null" />.
    /// </exception>
    public object Resolve(ServiceImplementationData serviceData)
    {
        if (serviceData == null)
        {
            throw new ArgumentNullException(nameof(serviceData));
        }

        return serviceData.ImplementationResolver!(OperationLifetime);
    }
}
