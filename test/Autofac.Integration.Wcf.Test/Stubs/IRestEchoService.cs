// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceModel;
using System.ServiceModel.Web;

namespace Autofac.Integration.Wcf.Test.Stubs;

/// <summary>
/// REST-style contract used to exercise the WebHttp hosting path over a
/// <see cref="WebHttpBinding"/>.
/// </summary>
[ServiceContract]
public interface IRestEchoService
{
    /// <summary>
    /// Echoes the supplied value via an HTTP GET.
    /// </summary>
    /// <param name="value">
    /// The value to echo.
    /// </param>
    /// <returns>
    /// The dependency-prefixed echo of the value.
    /// </returns>
    [OperationContract]
    [WebGet(UriTemplate = "/echo/{value}")]
    string Echo(string value);
}
