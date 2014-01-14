using Castle.Windsor;
using Castle.Windsor.Installer;
using NUnit.Framework;

namespace Snow.Tests
{
    [SetUpFixture]
    public class TestBootStrapper
    {
        public static WindsorContainer Container;

        [SetUp]
        public void Setup()
        {
            Container = new WindsorContainer();
            Container.Install(FromAssembly.This());
        }

        [TearDown]
        public void TearDown()
        {
            Container.Dispose();
        }
    }
}
