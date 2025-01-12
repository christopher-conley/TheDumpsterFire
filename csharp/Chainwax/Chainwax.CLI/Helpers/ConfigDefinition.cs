using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainwax.CLI.Helpers {
    public class ConfigDefinition {

        [JsonProperty(nameof(Config))]
        public ConfigRoot Config = new();
        public class ConfigRoot {

            [JsonProperty(nameof(Common))]
            public CommonRoot Common = new();

            [JsonProperty(nameof(CLI))]
            public CLIRoot CLI = new();

            [JsonProperty(nameof(CLI))]
            public GUIRoot GUI = new();
        }

        public class CommonRoot {

            [JsonProperty(nameof(DefaultHashAlgorithm))]
            public string DefaultHashAlgorithm { get; set; }
        }

        public class CLIRoot {

            [JsonProperty(nameof(DefaultCLICommand))]
            public string DefaultCLICommand { get; set; }

        }

        public class GUIRoot {
            [JsonProperty(nameof(ShowConsole))]
            public bool ShowConsole { get; set; }

            [JsonProperty(nameof(Theme))]
            public string Theme { get; set; }
            [JsonProperty(nameof(RememberWindowSize))]
            public bool RememberWindowSize { get; set; }
            [JsonProperty(nameof(RememberWindowPosition))]
            public bool RememberWindowPosition { get; set; }
        }
    }
}
