﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceModel;

namespace Autofac.Integration.Wcf.Test;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
public class TestSingletonService : ITestService
{
}
