using Autofac;
using Web_Vue.Server.Interfaces;
using Web_Vue.Server.Repositories.Base;

namespace Web_Vue.Server;

/// <summary>
/// Autofac 模組：自動掃描並以 Scoped 生命週期註冊
/// — Services 命名空間下所有 class
/// — Repositories 命名空間下所有 class
/// — IBaseRepository&lt;T&gt; → BaseRepository&lt;T&gt; 泛型開放型別
/// </summary>
public class AppModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var assembly = typeof(AppModule).Assembly;

        // Services：所有在 Web_Vue.Server.Services 命名空間下的具體類別
        builder.RegisterAssemblyTypes(assembly)
               .Where(t => t.Namespace?.StartsWith("Web_Vue.Server.Services") == true
                        && t.IsClass && !t.IsAbstract)
               .AsSelf()
               .InstancePerLifetimeScope();

        // Repositories：所有在 Web_Vue.Server.Repositories 命名空間下的具體類別
        builder.RegisterAssemblyTypes(assembly)
               .Where(t => t.Namespace?.StartsWith("Web_Vue.Server.Repositories") == true
                        && t.IsClass && !t.IsAbstract)
               .AsSelf()
               .InstancePerLifetimeScope();

        // IBaseRepository<T> 泛型開放型別
        builder.RegisterGeneric(typeof(BaseRepository<>))
               .As(typeof(IBaseRepository<>))
               .InstancePerLifetimeScope();
    }
}
