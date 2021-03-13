using System.Reflection;

namespace TT.Cronjobs.Blitz
{
    public interface IVersionProvider
    {
        public string Version { get; }
    }

    class AssemblyVersionProvider : IVersionProvider
    {
        private readonly Assembly _assembly;

        public AssemblyVersionProvider(Assembly assembly)
        {
            _assembly = assembly;
        }

        public string Version => _assembly.ManifestModule!.ModuleVersionId.ToString();
    }
}