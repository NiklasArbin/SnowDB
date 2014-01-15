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
        [NCrunch.Framework.ExclusivelyUses("Bootstrapper")]
        public void Setup()
        {
            Container = new WindsorContainer();
            Container.Install(FromAssembly.This());
        }

        [TearDown]
        [NCrunch.Framework.ExclusivelyUses("Bootstrapper")]
        public void TearDown()
        {
            Container.Dispose();
        }
    }
}
