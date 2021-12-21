using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GatheringTimer.Container
{
    public interface IDependency { }

    public class Container {

        private static IContainer container;

        private static void Init()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).Where(
                type => typeof(IDependency).IsAssignableFrom(type) && !type.IsAbstract)
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            container = builder.Build();

        }

        public static IContainer GetContainer()
        {
            if (null == container)
            {
                Init();
            }
            return container;
        }
    }
}
