// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// We can't run tests in parallel because many tests depend
// on the global static AutofacHostFactory.Container.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
