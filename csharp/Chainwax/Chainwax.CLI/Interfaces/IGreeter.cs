using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using Chainwax.CLI.ChainwaxSpectre;

namespace Chainwax.CLI.Interfaces {
    public interface IGreeter {
        void Greet(string name);
    }

    public sealed class HelloWorldGreeter : IGreeter {
        public void Greet(string name) {
            AnsiConsole.WriteLine($"Hello {name}!");
        }
    }
}
