// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A1 PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Linq;
using System.ServiceModel;
using Xunit;

namespace Autofac.Integration.Wcf.Test
{
    public class ServiceHostExtensionsFixture
    {
        [Fact]
        public void AddDependencyInjectionBehavior_NullContractType_ThrowsException()
        {
            ServiceHost serviceHost = new ServiceHost(typeof(ServiceType));
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => serviceHost.AddDependencyInjectionBehavior(null, new ContainerBuilder().Build()));
            Assert.Equal("contractType", exception.ParamName);
        }

        [Fact]
        public void AddDependencyInjectionBehavior_NullContainer_ThrowsException()
        {
            ServiceHost serviceHost = new ServiceHost(typeof(ServiceType));
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => serviceHost.AddDependencyInjectionBehavior(typeof(IContractType), null));
            Assert.Equal("container", exception.ParamName);
        }

        [Fact]
        public void AddDependencyInjectionBehavior_ContractTypeNotRegistered_ThrowsException()
        {
            ServiceHost serviceHost = new ServiceHost(typeof(ServiceType));
            Type contractType = typeof(IContractType);
            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => serviceHost.AddDependencyInjectionBehavior(contractType, new ContainerBuilder().Build()));
            Assert.Equal("contractType", exception.ParamName);
            string message = string.Format(ServiceHostExtensionsResources.ContractTypeNotRegistered, contractType.FullName);
            Assert.Contains(message, exception.Message);
        }

        [Fact]
        public void AddDependencyInjectionBehavior_ContractTypeRegistered_ServiceBehaviorConfigured()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.Register(c => new ServiceType()).As<IContractType>();
            IContainer container = builder.Build();

            ServiceHost serviceHost = new ServiceHost(typeof(ServiceType));
            serviceHost.AddDependencyInjectionBehavior(typeof(IContractType), container);

            int serviceBehaviorCount = serviceHost.Description.Behaviors
                .OfType<AutofacDependencyInjectionServiceBehavior>()
                .Count();
            Assert.Equal(1, serviceBehaviorCount);
        }

        [Fact]
        public void AddDependencyInjectionBehavior_SingleInstanceContextMode_ServiceBehaviorIgnored()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.Register(c => new SingletonServiceType()).As<IContractType>();
            IContainer container = builder.Build();

            ServiceHost serviceHost = new ServiceHost(typeof(SingletonServiceType));
            serviceHost.AddDependencyInjectionBehavior(typeof(IContractType), container);

            int serviceBehaviorCount = serviceHost.Description.Behaviors
                .OfType<AutofacDependencyInjectionServiceBehavior>()
                .Count();
            Assert.Equal(0, serviceBehaviorCount);
        }

        [Fact]
        public void AddDependencyInjectionBehaviorWithGenericArgument_ContractTypeRegistered_ServiceBehaviorConfigured()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.Register(c => new ServiceType()).As<IContractType>();
            IContainer container = builder.Build();

            ServiceHost serviceHost = new ServiceHost(typeof(ServiceType));
            serviceHost.AddDependencyInjectionBehavior<IContractType>(container);

            int serviceBehaviorCount = serviceHost.Description.Behaviors
                .OfType<AutofacDependencyInjectionServiceBehavior>()
                .Count();
            Assert.Equal(1, serviceBehaviorCount);
        }
    }

    interface IContractType
    {
    }

    class ServiceType : IContractType
    {
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class SingletonServiceType : IContractType
    {
    }
}
