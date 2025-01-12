using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

namespace RosettaTools.CLI.KeysCmd.SpectreCLI
{


    /// <summary>
    /// Implements type registration for dependency injection in the Spectre.Console CLI framework.
    /// </summary>
    public sealed class TypeRegistrar : ITypeRegistrar
    {
        private readonly IHostBuilder _builder;

        /// <summary>
        /// Initializes a new instance of the TypeRegistrar class.
        /// </summary>
        /// <param name="builder">The host builder to use for service registration.</param>
        public TypeRegistrar(IHostBuilder builder)
        {
            _builder = builder;
        }

        public ITypeResolver Build()
        {
            return new TypeResolver(_builder.Build());
        }

        public void Register(Type service, Type implementation)
        {
            _builder.ConfigureServices((_, services) => services.AddSingleton(service, implementation));
        }

        public void RegisterInstance(Type service, object implementation)
        {
            _builder.ConfigureServices((_, services) => services.AddSingleton(service, implementation));
        }

        public void RegisterLazy(Type service, Func<object> func)
        {
#if NET5_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(func);
#else
            if (func is null) {
                throw new ArgumentNullException(nameof(func));
            }
#endif

            _builder.ConfigureServices((_, services) => services.AddSingleton(service, _ => func()));
        }
    }
}
