using System.Reflection;
using System.Runtime.Loader;

namespace Beacon.Server.Plugins.Local
{
    public class PluginAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;

        public PluginAssemblyLoadContext(FileInfo file) : base(file.Name, isCollectible: true)
        {
            _resolver = new AssemblyDependencyResolver(file.FullName);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            return assemblyPath == null ? null : LoadFromAssemblyPath(assemblyPath);
        }

    }
}
