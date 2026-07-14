// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;
using Autofac.Integration.Wcf.Test.Stubs;

namespace Autofac.Integration.Wcf.Test.Integration;

/// <summary>
/// Opens a real <see cref="WebServiceHost"/> via
/// <see cref="AutofacWebServiceHostFactory"/> and issues an HTTP GET to prove the
/// WebHttp hosting path works and injects dependencies.
/// </summary>
public class WebServiceHostFixture
{
    [WindowsFact]
    public void WebServiceHostFactory_ServicesHttpGetRequest()
    {
        var builder = new ContainerBuilder();
        builder.RegisterInstance(new DependencyActivity());
        builder.RegisterType<TrackedDependency>().InstancePerLifetimeScope();
        builder.RegisterType<RestEchoService>().As<IRestEchoService>().InstancePerDependency();

        // Parallelism is disabled in this assembly, so a unique port avoids
        // collisions across runs.
        var port = 20000 + (Math.Abs(Guid.NewGuid().GetHashCode()) % 20000);
        var baseAddress = new Uri($"http://localhost:{port}/rest");

        WcfTestHarness.WithHostedContainer(builder.Build(), () =>
        {
            var host = (WebServiceHost)new AutofacWebServiceHostFactory().CreateServiceHost(
                typeof(IRestEchoService).AssemblyQualifiedName!,
                new[] { baseAddress });
            host.AddServiceEndpoint(typeof(IRestEchoService), new WebHttpBinding(), baseAddress);
            host.Open();

            try
            {
                using var client = new WebClient();
                var response = client.DownloadString(new Uri(baseAddress + "/echo/hello"));

                // Response is JSON-quoted and carries the injected dependency ID.
                Assert.Contains(":hello", response, StringComparison.Ordinal);
            }
            finally
            {
                host.Close();
            }
        });
    }
}
