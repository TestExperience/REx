using NUnit.Framework;
using System.Threading;

namespace REx.Client.Tests
{
    [TestFixture]
    public class RemoteExecutorTests
    {
        private RemoteExeutor _exeutor;

        [TestFixtureSetUp]
        private void SetupTestFixture()
        {
            _exeutor = new RemoteExeutor("localhost");
            _exeutor.DeployService();
        }

        [Test]
        public void ProcessCanBeExecutedOnRemoteNodeTest()
        {
            var finishedManualResetEvent = new ManualResetEvent(false);
            _exeutor.StartProcessAsync("cmd", "/c 'exit 5'",
                i =>
                {
                    finishedManualResetEvent.Set();
                    Assert.That(i, Is.EqualTo(5));
                });
            finishedManualResetEvent.WaitOne(5000);
        }

        [TestFixtureTearDown]
        private void TearDownFixture()
        {
            _exeutor.Dispose();
        }
    }
}
