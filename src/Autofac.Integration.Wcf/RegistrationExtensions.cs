// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceModel;
using Autofac.Builder;

namespace Autofac.Integration.Wcf;

/// <summary>
/// Extend the registration syntax with WCF-specific helpers.
/// </summary>
public static class RegistrationExtensions
{
    /// <summary>
    /// Disposes the channel instance safely, suppressing exceptions thrown
    /// when closing a faulted channel.
    /// </summary>
    /// <typeparam name="TLimit">
    /// Registration limit type.
    /// </typeparam>
    /// <typeparam name="TActivatorData">
    /// Activator data type.
    /// </typeparam>
    /// <typeparam name="TRegistrationStyle">
    /// Registration style.
    /// </typeparam>
    /// <param name="registration">
    /// Registration to set the release action for.
    /// </param>
    /// <returns>
    /// Registration builder allowing the registration to be configured.
    /// </returns>
    /// <remarks>
    /// Exceptions thrown when closing a faulted channel are suppressed.
    /// </remarks>
    public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle>
        UseWcfSafeRelease<TLimit, TActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration)
    {
        // WCF's Dispose calls Close, which throws if the channel is faulted.
        // OnRelease with CloseChannel handles faulted channels via Abort instead.
        if (registration == null)
        {
            throw new ArgumentNullException(nameof(registration));
        }

        return registration.OnRelease(CloseChannel);
    }

    private static void CloseChannel<T>(T channel)
    {
        if (channel is not ICommunicationObject communicationObject)
        {
            return;
        }

        try
        {
            if (communicationObject.State == CommunicationState.Faulted)
            {
                communicationObject.Abort();
            }
            else
            {
                communicationObject.Close();
            }
        }
        catch (TimeoutException)
        {
            communicationObject.Abort();
        }
        catch (CommunicationException)
        {
            communicationObject.Abort();
        }
        catch (Exception)
        {
            communicationObject.Abort();
            throw;
        }
    }
}
