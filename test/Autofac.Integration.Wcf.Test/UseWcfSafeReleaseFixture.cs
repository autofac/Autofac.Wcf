// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceModel;

namespace Autofac.Integration.Wcf.Test;

/// <summary>
/// Tests for the client-side <see cref="RegistrationExtensions.UseWcfSafeRelease"/>
/// behavior: a channel is closed on release, but faulted channels (and channels
/// whose close throws) are aborted instead so a faulted-channel exception does
/// not mask the real error.
/// </summary>
public class UseWcfSafeReleaseFixture
{
    [Fact]
    public void ChannelWhoseCloseThrowsCommunicationException_IsAborted()
    {
        var channel = new FakeChannel(CommunicationState.Opened) { CloseException = new CommunicationException() };
        using (var container = BuildContainerWith(channel))
        using (var scope = container.BeginLifetimeScope())
        {
            scope.Resolve<IFakeChannel>();
        }

        Assert.True(channel.CloseCalled);
        Assert.True(channel.AbortCalled);
    }

    [Fact]
    public void ChannelWhoseCloseThrowsTimeoutException_IsAborted()
    {
        var channel = new FakeChannel(CommunicationState.Opened) { CloseException = new TimeoutException() };
        using (var container = BuildContainerWith(channel))
        using (var scope = container.BeginLifetimeScope())
        {
            scope.Resolve<IFakeChannel>();
        }

        Assert.True(channel.AbortCalled);
    }

    [Fact]
    public void ChannelWhoseCloseThrowsUnexpectedException_IsAbortedAndRethrows()
    {
        var channel = new FakeChannel(CommunicationState.Opened) { CloseException = new InvalidOperationException("boom") };
        using var container = BuildContainerWith(channel);

        // An unexpected exception during close is rethrown after aborting.
        Assert.Throws<InvalidOperationException>(() =>
        {
            using var scope = container.BeginLifetimeScope();
            scope.Resolve<IFakeChannel>();
        });

        Assert.True(channel.AbortCalled);
    }

    [Fact]
    public void FaultedChannel_IsAbortedOnRelease()
    {
        var channel = new FakeChannel(CommunicationState.Faulted);
        using (var container = BuildContainerWith(channel))
        using (var scope = container.BeginLifetimeScope())
        {
            scope.Resolve<IFakeChannel>();
        }

        // A faulted channel must be aborted, never closed (closing throws).
        Assert.True(channel.AbortCalled);
        Assert.False(channel.CloseCalled);
    }

    [Fact]
    public void OpenChannel_IsClosedOnRelease()
    {
        var channel = new FakeChannel(CommunicationState.Opened);
        using (var container = BuildContainerWith(channel))
        using (var scope = container.BeginLifetimeScope())
        {
            scope.Resolve<IFakeChannel>();
        }

        Assert.True(channel.CloseCalled);
        Assert.False(channel.AbortCalled);
    }

    private static IContainer BuildContainerWith(FakeChannel channel)
    {
        var builder = new ContainerBuilder();
        builder.Register(_ => channel).As<IFakeChannel>().UseWcfSafeRelease();
        return builder.Build();
    }

    public interface IFakeChannel
    {
    }

    public sealed class FakeChannel : IFakeChannel, ICommunicationObject
    {
        // ICommunicationObject requires these events. The fake never raises them,
        // so the accessors just track handlers on a shared delegate; this avoids
        // both the "unused field-like event" (CS0067) and "empty block" (S108)
        // warnings without suppression.
        private EventHandler? _handlers;

        public FakeChannel(CommunicationState state) => State = state;

        public event EventHandler? Closed
        {
            add => _handlers += value;
            remove => _handlers -= value;
        }

        public event EventHandler? Closing
        {
            add => _handlers += value;
            remove => _handlers -= value;
        }

        public event EventHandler? Faulted
        {
            add => _handlers += value;
            remove => _handlers -= value;
        }

        public event EventHandler? Opened
        {
            add => _handlers += value;
            remove => _handlers -= value;
        }

        public event EventHandler? Opening
        {
            add => _handlers += value;
            remove => _handlers -= value;
        }

        public CommunicationState State
        {
            get;
            private set;
        }

        public Exception? CloseException
        {
            get; set;
        }

        public bool CloseCalled
        {
            get; private set;
        }

        public bool AbortCalled
        {
            get; private set;
        }

        public void Abort()
        {
            AbortCalled = true;
            State = CommunicationState.Closed;
        }

        public void Close()
        {
            CloseCalled = true;
            if (CloseException != null)
            {
                throw CloseException;
            }

            State = CommunicationState.Closed;
        }

        public void Close(TimeSpan timeout) => Close();

        public IAsyncResult BeginClose(AsyncCallback callback, object state) => throw new NotImplementedException();

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state) => throw new NotImplementedException();

        public void EndClose(IAsyncResult result) => throw new NotImplementedException();

        public void Open() => State = CommunicationState.Opened;

        public void Open(TimeSpan timeout) => Open();

        public IAsyncResult BeginOpen(AsyncCallback callback, object state) => throw new NotImplementedException();

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state) => throw new NotImplementedException();

        public void EndOpen(IAsyncResult result) => throw new NotImplementedException();
    }
}
