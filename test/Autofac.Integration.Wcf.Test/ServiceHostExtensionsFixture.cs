// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
            var serviceHost = new ServiceHost(typeof(ServiceType));
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => serviceHost.AddDependencyInjectionBehavior(null, new ContainerBuilder().Build()));
            Assert.Equal("contractType", exception.ParamName);
        }

        [Fact]
        public void AddDependencyInjectionBehavior_NullContainer_ThrowsException()
        {
            var serviceHost = new ServiceHost(typeof(ServiceType));
            var exception = Assert.Throws<ArgumentNullException>(
                () => serviceHost.AddDependencyInjectionBehavior(typeof(IContractType), null));
            Assert.Equal("container", exception.ParamName);
        }

        [Fact]
        public void AddDependencyInjectionBehavior_NullParameters_ThrowsException()
        {
            var serviceHost = new ServiceHost(typeof(ServiceType));
            var exception = Assert.Throws<ArgumentNullException>(
                () => serviceHost.AddDependencyInjectionBehavior(typeof(IContractType), new ContainerBuilder().Build(), null));
            Assert.Equal("parameters", exception.ParamName);
        }

        [Fact]
        public void AddDependencyInjectionBehavior_ContractTypeNotRegistered_ThrowsException()
        {
            var serviceHost = new ServiceHost(typeof(ServiceType));
            var contractType = typeof(IContractType);
            var exception = Assert.Throws<ArgumentException>(
                () => serviceHost.AddDependencyInjectionBehavior(contractType, new ContainerBuilder().Build()));
            Assert.Equal("contractType", exception.ParamName);
            var message = string.Format(ServiceHostExtensionsResources.ContractTypeNotRegistered, contractType.FullName);
            Assert.Contains(message, exception.Message);
        }

        [Fact]
        public void AddDependencyInjectionBehavior_ContractTypeRegistered_ServiceBehaviorConfigured()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new ServiceType()).As<IContractType>();
            IContainer container = builder.Build();

            var serviceHost = new ServiceHost(typeof(ServiceType));
            serviceHost.AddDependencyInjectionBehavior(typeof(IContractType), container);

            var serviceBehaviorCount = serviceHost.Description.Behaviors
                .OfType<AutofacDependencyInjectionServiceBehavior>()
                .Count();
            Assert.Equal(1, serviceBehaviorCount);
        }

        [Fact]
        public void AddDependencyInjectionBehavior_SingleInstanceContextMode_ServiceBehaviorIgnored()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new SingletonServiceType()).As<IContractType>();
            var container = builder.Build();

            var serviceHost = new ServiceHost(typeof(SingletonServiceType));
            serviceHost.AddDependencyInjectionBehavior(typeof(IContractType), container);

            var serviceBehaviorCount = serviceHost.Description.Behaviors
                .OfType<AutofacDependencyInjectionServiceBehavior>()
                .Count();
            Assert.Equal(0, serviceBehaviorCount);
        }

        [Fact]
        public void AddDependencyInjectionBehaviorWithGenericArgument_ContractTypeRegistered_ServiceBehaviorConfigured()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new ServiceType()).As<IContractType>();
            var container = builder.Build();

            var serviceHost = new ServiceHost(typeof(ServiceType));
            serviceHost.AddDependencyInjectionBehavior<IContractType>(container);

            var serviceBehaviorCount = serviceHost.Description.Behaviors
                .OfType<AutofacDependencyInjectionServiceBehavior>()
                .Count();
            Assert.Equal(1, serviceBehaviorCount);
        }

        internal interface IContractType
        {
        }

        internal class ServiceType : IContractType
        {
        }

        [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
        internal class SingletonServiceType : IContractType
        {
        }
    }
}
