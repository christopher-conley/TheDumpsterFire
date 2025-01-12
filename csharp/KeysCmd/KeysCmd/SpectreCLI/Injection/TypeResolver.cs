using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

namespace RosettaTools.CLI.KeysCmd.SpectreCLI
{

    /// <summary>
    /// Implements type resolution for dependency injection in the Spectre.Console CLI framework.
    /// </summary>
    public sealed class TypeResolver : ITypeResolver, IDisposable
    {
        private readonly IHost _host;

        /// <summary>
        /// Initializes a new instance of the TypeResolver class.
        /// </summary>
        /// <param name="provider">The host that provides the service provider.</param>
        public TypeResolver(IHost provider)
        {
            _host = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public object? Resolve(Type? type)
        {
            return type != null ? _host.Services.GetService(type) : null;
        }

        public void Dispose()
        {
            _host.Dispose();
        }
    }
}