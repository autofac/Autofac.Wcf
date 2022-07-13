# Autofac.Wcf

Windows Communication Foundation (WCF) integration for [Autofac](https://autofac.org).

[![Build status](https://ci.appveyor.com/api/projects/status/5hf5l1qqncrc15yu?svg=true)](https://ci.appveyor.com/project/Autofac/autofac-yirkj)

Please file issues and pull requests for this package [in this repository](https://github.com/autofac/Autofac.Wcf/issues) rather than in the Autofac core repo.

- [Documentation](https://autofac.readthedocs.io/en/latest/integration/wcf.html)
- [NuGet](https://www.nuget.org/packages/Autofac.Wcf/)
- [Contributing](https://autofac.readthedocs.io/en/latest/contributors.html)
- [Open in Visual Studio Code](https://open.vscode.dev/autofac/Autofac.Wcf)

## Quick Start: Clients

During application startup, for each service register a `ChannelFactory<T>` and a function that uses the factory to open channels:

```c#
var builder = new ContainerBuilder();

// Register the channel factory for the service. Make it
// SingleInstance since you don't need a new one each time.
builder
  .Register(c => new ChannelFactory<ITrackListing>(
    new BasicHttpBinding(),
    new EndpointAddress("http://localhost/TrackListingService")))
  .SingleInstance();

// Register the service interface using a lambda that creates
// a channel from the factory. Include the UseWcfSafeRelease()
// helper to handle proper disposal.
builder
  .Register(c => c.Resolve<ChannelFactory<ITrackListing>>().CreateChannel())
  .As<ITrackListing>()
  .UseWcfSafeRelease();

// You can also register other dependencies.
builder.RegisterType<AlbumPrinter>();

var container = builder.Build();
```

When consuming the service, add a constructor dependency as normal. This example shows an application that prints a track listing to the console using the remote `ITrackListing` service. It does this via the `AlbumPrinter` class:

```c#
public class AlbumPrinter
{
  readonly ITrackListing _trackListing;

  public AlbumPrinter(ITrackListing trackListing)
  {
    _trackListing = trackListing;
  }

  public void PrintTracks(string artist, string album)
  {
    foreach (var track in _trackListing.GetTracks(artist, album))
      Console.WriteLine("{0} - {1}", track.Position, track.Title);
  }
}
```

## Quick Start: Services

To get Autofac integrated with WCF on the service side you need to reference the WCF integration NuGet package, register your services, and set the dependency resolver. You also need to update your .svc files to reference the Autofac service host factory.

Here’s a sample application startup block:

```c#
protected void Application_Start()
{
  var builder = new ContainerBuilder();

  // Register your service implementations.
  builder.RegisterType<TestService.Service1>();

  // Set the dependency resolver.
  var container = builder.Build();
  AutofacHostFactory.Container = container;
}
```

And here’s a sample .svc file.

```aspx
<%@ ServiceHost
    Service="TestService.Service1, TestService"
    Factory="Autofac.Integration.Wcf.AutofacServiceHostFactory, Autofac.Integration.Wcf" %>
```

## Get Help

**Need help with Autofac?** We have [a documentation site](https://autofac.readthedocs.io/) as well as [API documentation](https://autofac.org/apidoc/). We're ready to answer your questions on [Stack Overflow](https://stackoverflow.com/questions/tagged/autofac) or check out the [discussion forum](https://groups.google.com/forum/#forum/autofac).
