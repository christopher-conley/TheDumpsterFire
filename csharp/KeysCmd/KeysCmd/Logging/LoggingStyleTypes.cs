namespace RosettaTools.CLI.KeysCmd.Logging.StyleTypes
{
    public sealed class SuccessMessage
    {
        public static readonly SuccessMessage Value = new SuccessMessage();

        private SuccessMessage() { }

        public override string ToString() => "[green]SUCCESS[/]";
    }

    public sealed class WarnMessage
    {
        public static readonly WarnMessage Value = new WarnMessage();

        private WarnMessage() { }

        public override string ToString() => "[yellow]WARNING[/]";
    }

    public sealed class FailMessage
    {
        public static readonly FailMessage Value = new FailMessage();

        private FailMessage() { }

        public override string ToString() => "[red]FAILURE[/]";
    }

    public sealed class ErrorMessage
    {
        public static readonly ErrorMessage Value = new ErrorMessage();

        private ErrorMessage() { }

        public override string ToString() => "[red]ERROR[/]";
    }

}
