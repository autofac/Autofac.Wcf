// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using System.Reflection;
using System.ServiceModel;

namespace Autofac.Integration.Wcf;

/// <summary>
/// A <see cref="DispatchProxy"/> that implements a WCF service contract
/// interface and forwards every call to a wrapped target instance.
/// </summary>
/// <remarks>
/// <para>
/// This works around a WCF hosting limitation. When a singleton service is
/// registered with interception, the instance resolved from Autofac is a
/// dynamic proxy. Dynamic-proxy libraries such as Castle copy the type-level
/// attributes from the proxied interface onto the generated proxy class, so the
/// proxy type both declares <see cref="ServiceContractAttribute"/> and inherits
/// it. WCF rejects a class that both defines and inherits a service contract.
/// </para>
/// <para>
/// A <see cref="DispatchProxy"/>-generated type only inherits the contract
/// attribute through the interface it implements; it never declares the
/// attribute on the class itself. Wrapping the resolved instance in one of
/// these proxies gives WCF a type it can host while still routing calls - and
/// thus any interception - through the original instance.
/// </para>
/// <para>
/// Use the <see cref="WrapIfNecessary"/> or <see cref="Create"/> factory methods
/// to obtain an instance. This type and its parameterless constructor must be
/// public because <see cref="DispatchProxy"/> emits the generated subclass into a
/// separate dynamic assembly that can reach only public base members;
/// constructing this type directly produces a proxy with no target.
/// </para>
/// </remarks>
public class ContractForwardingProxy : DispatchProxy
{
    private static readonly MethodInfo _createMethod =
        typeof(DispatchProxy).GetMethod(nameof(Create), Array.Empty<Type>())!;

    private object? _target;

    /// <summary>
    /// Wraps an instance in a contract-forwarding proxy when its runtime type
    /// would prevent WCF from hosting it; otherwise returns it unchanged.
    /// </summary>
    /// <param name="singletonInstance">
    /// The instance resolved from Autofac to be hosted as a WCF singleton.
    /// </param>
    /// <param name="serviceTypeToHost">
    /// The clean concrete service type that describes the service contract.
    /// </param>
    /// <returns>
    /// The original instance if it can be hosted directly, or a
    /// <see cref="DispatchProxy"/> implementing the service contract and
    /// forwarding to it if not.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="singletonInstance"/> or
    /// <paramref name="serviceTypeToHost"/> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    /// <para>
    /// When the service implements more than one contract interface, the first
    /// one found is used for the wrapper. Multiple service contracts on a single
    /// intercepted singleton are not supported.
    /// </para>
    /// </remarks>
    internal static object WrapIfNecessary(object singletonInstance, Type serviceTypeToHost)
    {
        if (singletonInstance == null)
        {
            throw new ArgumentNullException(nameof(singletonInstance));
        }

        if (serviceTypeToHost == null)
        {
            throw new ArgumentNullException(nameof(serviceTypeToHost));
        }

        var instanceType = singletonInstance.GetType();

        // A normal service class carries the contract on its interface, not on the
        // class, and hosts fine. Only a type that also declares its own contract
        // (as dynamic proxies do) needs wrapping.
        if (!DeclaresAndInheritsServiceContract(instanceType))
        {
            return singletonInstance;
        }

        // Prefer a contract interface from the clean service type; fall back to the
        // instance type. If none is found, let WCF surface its own error.
        var contractInterface = FindServiceContractInterface(serviceTypeToHost)
            ?? FindServiceContractInterface(instanceType);
        if (contractInterface == null)
        {
            return singletonInstance;
        }

        return Create(contractInterface, singletonInstance);
    }

    /// <summary>
    /// Wraps a target instance in a <see cref="ContractForwardingProxy"/> that
    /// implements the given contract interface.
    /// </summary>
    /// <param name="contractInterface">
    /// The WCF service contract interface the generated proxy should implement.
    /// </param>
    /// <param name="target">
    /// The instance calls are forwarded to. Must implement the contract.
    /// </param>
    /// <returns>
    /// An object implementing the contract whose runtime type does not declare
    /// <see cref="ServiceContractAttribute"/> and can be hosted by WCF.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="contractInterface"/> or <paramref name="target"/>
    /// is <see langword="null" />.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The caller is responsible for passing a target that implements the
    /// contract; a mismatch is not detected here and surfaces as a failure on the
    /// first forwarded call.
    /// </para>
    /// </remarks>
    internal static object Create(Type contractInterface, object target)
    {
        if (contractInterface == null)
        {
            throw new ArgumentNullException(nameof(contractInterface));
        }

        var proxy = _createMethod.MakeGenericMethod(contractInterface, typeof(ContractForwardingProxy)).Invoke(null, null)!;
        ((ContractForwardingProxy)proxy)._target = target ?? throw new ArgumentNullException(nameof(target));
        return proxy;
    }

    /// <summary>
    /// Forwards the intercepted call to the wrapped target instance.
    /// </summary>
    /// <param name="targetMethod">The method the caller invoked.</param>
    /// <param name="args">The arguments the caller supplied.</param>
    /// <returns>The value returned by the wrapped target.</returns>
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod == null)
        {
            throw new ArgumentNullException(nameof(targetMethod));
        }

        try
        {
            return targetMethod.Invoke(_target, args);
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            // Unwrap so callers see the real exception, not the reflection wrapper.
            throw ex.InnerException;
        }
    }

    private static bool DeclaresAndInheritsServiceContract(Type type)
    {
        var declaresContract = type.GetCustomAttributes(typeof(ServiceContractAttribute), false).Length > 0;
        if (!declaresContract)
        {
            return false;
        }

        return type.GetInterfaces().Any(i => i.GetCustomAttributes(typeof(ServiceContractAttribute), false).Length > 0);
    }

    private static Type? FindServiceContractInterface(Type type)
        => type.GetInterfaces().FirstOrDefault(i => i.GetCustomAttributes(typeof(ServiceContractAttribute), false).Length > 0);
}
