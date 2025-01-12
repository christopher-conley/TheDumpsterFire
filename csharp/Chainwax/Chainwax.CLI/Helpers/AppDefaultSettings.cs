using Chainwax.CLI.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainwax.CLI.Helpers {
    class AppDefaultSettings {
        public bool OnTheChainWax;
        public string AppTitle;
        public bool ShowConsole;
        public string Theme;
        public string DefaultHashAlgorithm;
        public string DefaultCLICommand;
        public bool RememberWindowSize;
        public bool RememberWindowPosition;
        public string ConfigDir;
        public ConfigDefinition DefaultConfig;

        public AppDefaultSettings() {
            OnTheChainWax = true;
            AppTitle = "Chainwax";
            ShowConsole = false;
            Theme = "dark";
            DefaultHashAlgorithm = "SHA256";
            DefaultCLICommand = "--help";
            RememberWindowSize = true;
            RememberWindowPosition = true;
            ConfigDir = AppTitle;

            DefaultConfig = new() {
                Config = new() {

                    Common = new() {
                        DefaultHashAlgorithm = DefaultHashAlgorithm
                    },

                    CLI = new() {
                        DefaultCLICommand = DefaultCLICommand
                    },

                    GUI = new() {
                        ShowConsole = ShowConsole,
                        Theme = Theme,
                        RememberWindowSize = RememberWindowSize,
                        RememberWindowPosition = RememberWindowPosition
                    }
                }
            };
        }

        public static Hashtable GetDefaultSettings() {
            Hashtable returnObject = [];

            foreach (var Field in typeof(AppDefaultSettings).GetFields()) {
                returnObject.Add(Field.Name, Field.GetValue(new AppDefaultSettings()));
            }
            return returnObject;
        }

        public static dynamic GetDefaultSetting(string setting) {
            dynamic? returnObject;
            var defaultSetting = new AppDefaultSettings().GetType().GetFields()
                .Where(x => x.Name == setting)
                .Select(x => x.GetValue(new AppDefaultSettings()));
            //var bleh5 = bleh3.Where(x => x.Name == setting).Select(x => x.GetValue(new AppDefaultSettings()));
            returnObject = typeof(AppDefaultSettings).GetField(setting).GetValue(new AppDefaultSettings()) ?? null;
            return returnObject;
        }

        public static ConfigDefinition GetDefaultConfig() {
            return new AppDefaultSettings().DefaultConfig;
        }
        public static string GetDefaultConfigAsString() {
            return JsonConvert.SerializeObject(new AppDefaultSettings().DefaultConfig);
        }


    }
}
