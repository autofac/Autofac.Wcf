using System.Reflection;
using Xunit;

[assembly: AssemblyTitle("Autofac.Integration.Wcf.Test")]

// We can't run tests in parallel because many tests depend
// on the global static AutofacHostFactory.Container.
[assembly: CollectionBehavior(DisableTestParallelization = true)]