using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainwax.CLI.ChainwaxSpectre.Meta {

    public static class AppInfo {

        public static class BranchAdd {
            private static string _branchName = "add";
            private static string _description = "Add a symbolic or hard link.";
            private static string[] _aliases = [ "create", "new" ];


            public static string BranchName
            {
                get { return _branchName; }
                private set { _branchName = value; }
            }

            public static string Description
            {
                get { return _description; }
                private set { _description = value; }
            }

            public static string[] Aliases
            {
                get { return _aliases; }
                private set { _aliases = value; }
            }

            public static class Commands {

                public static class Symlink {
                    private static string _commandName = "symlink";
                    private static string _description = "Create a symbolic link to a file or directory. The target may reside on a different drive/volume/filesystem.";
                    private static string[] _aliases = [ "symbolic", "symboliclink" ];
                    private static string _helpExampleDir = Environment.GetEnvironmentVariable("USERPROFILE") ?? "C:\\Users\\username";
                    private static string[] _examples = [
                        $"add symlink --target {_helpExampleDir}\\Documents\\file.jpg --link-name {_helpExampleDir}\\Desktop\\file.jpg",
                        $"add symlink -t {_helpExampleDir}\\AppData\\Local -l {_helpExampleDir}\\Desktop\\LocalAppData"
                    ];

                    public static string CommandName
                    {
                        get { return _commandName; }
                        private set { _commandName = value; }
                    }
                    public static string Description
                    {
                        get { return _description; }
                        private set { _description = value; }
                    }

                    public static string[] Aliases
                    {
                        get { return _aliases; }
                        private set { _aliases = value; }
                    }

                    public static string[] Examples
                    {
                        get { return _examples; }
                        private set { _examples = value; }
                    }
                }

                public static class Hardlink {
                    private static string _commandName = "hardlink";
                    private static string _description = "Create a hard link to a file or directory. The target MAY NOT reside on a different drive/volume/filesystem.";
                    private static string[] _aliases = [ "hard" ];

                    public static string CommandName
                    {
                        get { return _commandName; }
                        private set { _commandName = value; }
                    }
                    public static string Description
                    {
                        get { return _description; }
                        private set { _description = value; }
                    }

                    public static string[] Aliases
                    {
                        get { return _aliases; }
                        private set { _aliases = value; }
                    }
                }
            }
        }
    }
}