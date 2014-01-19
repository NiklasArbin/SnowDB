using Castle.Windsor;
using Castle.Windsor.Installer;
using NUnit.Framework;
using Snow.Core;

namespace Snow.Tests
{
    [SetUpFixture]
    public class TestBootStrapper
    {
        public static WindsorContainer Container;

        [SetUp]
        [NCrunch.Framework.Serial]
        public void Setup()
        {
            Container = new WindsorContainer();
            Container.Install(FromAssembly.This());
        }

        [TearDown]
        [NCrunch.Framework.Serial]
        public void TearDown()
        {
            var store = Container.Resolve<IDocumentStore>();
            store.Dispose();
            Container.Dispose();
        }
    }
}
