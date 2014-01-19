using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Snow.Core;

namespace Snow.Tests.IoC
{
    public class StoreInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var docStore = new DocumentStore { DataLocation = TestSetup.DataDir, DatabaseName = TestSetup.DatabaseName };
            using (docStore.OpenSession())
            {
                container.Register(
                    Component.For<IDocumentStore>().Named("ExclusiveName").Instance(docStore));
            }
        }

    }
}
