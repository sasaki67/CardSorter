using System;
using NUnit.Framework;

namespace Wave.ScreenTransition.Tests
{
    public sealed class ServiceLocatorTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceLocator.Clear();
        }

        [Test]
        public void RegisterAndResolveReturnsTheRegisteredInstance()
        {
            var service = new TestService();

            Assert.That(ServiceLocator.TryRegister<ITestService, TestService>(service), Is.True);
            Assert.That(ServiceLocator.Resolve<ITestService>(), Is.SameAs(service));
            Assert.That(ServiceLocator.TryRegister<ITestService, TestService>(new TestService()), Is.False);
        }

        [Test]
        public void UnregisterWithDifferentInstanceDoesNotRemoveTheService()
        {
            var registered = new TestService();
            var other = new TestService();
            ServiceLocator.Register<ITestService>(registered);

            Assert.That(ServiceLocator.TryUnregister<ITestService>(other), Is.False);
            Assert.That(ServiceLocator.Resolve<ITestService>(), Is.SameAs(registered));
        }

        [Test]
        public void ClearDisposesRegisteredServices()
        {
            var service = new TestService();
            ServiceLocator.Register<ITestService>(service);

            ServiceLocator.Clear();

            Assert.That(service.IsDisposed, Is.True);
            Assert.That(ServiceLocator.Resolve<ITestService>(), Is.Null);
        }

        private interface ITestService
        {
        }

        private sealed class TestService : ITestService, IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }
    }
}
