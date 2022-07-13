// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ServiceModel;
using Autofac.Builder;

namespace Autofac.Integration.Wcf
{
    /// <summary>
    /// Extend the registration syntax with WCF-specific helpers.
    /// </summary>
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Dispose the channel instance in such a way that exceptions aren't thrown
        /// if a faulted channel is closed.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set release action for.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        /// <remarks>This will eat exceptions generated in the closing of the channel.</remarks>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle>
            UseWcfSafeRelease<TLimit, TActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration)
        {
            // When a channel is closed in WCF, the Dispose method calls Close internally.
            // If the channel is in a faulted state, the Close method will throw, yielding
            // an incorrect exception to be thrown during disposal. This extension fixes
            // that design problem.
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
}
