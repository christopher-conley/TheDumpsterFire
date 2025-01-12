namespace RosettaTools.CLI.KeysCmd.SpectreCLI.Meta
{

    public static class AppInfo
    {

        public static class KeysBranch
        {
            private static string _branchName = "keys";
            private static string _description = "Retrieve a user's public SSH keys listed in AD. "
                + "If the -o/--openssh flag is specified, commands will NEVER write anything to stdout that isn't "
                + "an SSH public key or an empty null-terminated string, because that's what OpenSSH expects to receive "
                + "when looking up keys via its \"AuthorizedKeysCommand\" sshd_config configuration option.";
            private static string[] _aliases = ["sshkeys", "userkeys"];

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

            public static class Commands
            {

                private static GetCommand get = new()
                {
                    Examples =
                    [
                        $"keys get -u \"SOMEDOMAIN\\someuser\" --openssh",
                        $"keys get -o --user \"SOMEDOMAIN\\someuser\" -k \"AlternativeSSHkeysADAttribute\"",
                        $"keys get --key-attribute \"AlternativeSSHkeysADAttribute\" -o --user \"SOMEDOMAIN\\someuser\"",
                        $"keys list --user \"SOMEDOMAIN\\someuser\"",
                        $"keys lookup -u \"SOMEDOMAIN\\someuser\" -k \"SomeOtherSSHKeysADAttribute\"",
                        $"keys show --user \"SOMEDOMAIN\\someuser\" -o",
                        $"sshkeys list -u SOMEDOMAIN\\someuser --key-attribute \"AlternativeSSHkeysADAttribute\"",
                        $"sshkeys get --user \"SOMEDOMAIN\\someuser\"",
                        $"sshkeys get --user \"SOMEDOMAIN\\someuser\" -o",
                        $"userkeys lookup -u \"SOMEDOMAIN\\someuser\"",
                        $"userkeys list --user SOMEDOMAIN\\someuser -k \"DifferentSSHkeysADAttribute\"",
                    ]
                };

                public static GetCommand Get { get => get; set => get = value; }
            }
        }

        public static class ConfigBranch
        {
            private static string _branchName = "config";
            private static string _description = "KeysCmd configuration-related commands.";
            private static string[] _aliases = ["configuration", "settings"];

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

            public static class Commands
            {

                public static class Show
                {
                    private static string _commandName = "show";
                    private static string _description = "Display the current configuration settings.";
                    private static string[] _aliases = ["get", "display"];
                    private static string[] _examples = [
                        "config show",
                        "config get",
                        "config display"
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

                public static class Edit
                {
                    private static string _commandName = "edit";
                    private static string _description = "Launch and edit the JSON configuration file.";
                    private static string[] _aliases = ["change", "modify"];
                    private static string[] _examples = [
                        "config edit",
                        "config change",
                        "config modify"
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

                public static class New
                {
                    private static string _commandName = "new";
                    private static string _description = "Write out a default configuration file.";
                    private static string[] _aliases = ["writedefault", "create"];
                    private static string[] _examples = [
                        "config new",
                        "config create",
                        "config writedefault"
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

            }
        }

        public class GetCommand
        {

            private string _commandName;
            private string _description;
            private string[] _aliases;
            private string[]? _examples;

            public string CommandName
            {
                get { return _commandName; }
                protected internal set { _commandName = value; }
            }
            public string Description
            {
                get { return _description; }
                protected internal set { _description = value; }
            }

            public string[] Aliases
            {
                get { return _aliases; }
                protected internal set { _aliases = value; }
            }

            public string[]? Examples
            {
                get
                {
                    _examples ??= [];
                    return _examples;
                }
                protected internal set { _examples = value; }
            }

            public GetCommand()
            {
                _commandName = "get";
                _description = "Retrieve public ssh keys for a given user from AD.";
                _aliases = ["lookup", "list", "show"];
            }
        }
    }
}