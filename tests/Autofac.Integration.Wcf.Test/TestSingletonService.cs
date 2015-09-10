using System.ServiceModel;

namespace Autofac.Integration.Wcf.Test
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class TestSingletonService : ITestService
    {
    }
}