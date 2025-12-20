using static PSPhlebotomist.Common.Statics.ANSI;

namespace Serilog.Templates.Themes
{
    public class CustomThemes
    {
        public static TemplateTheme Sixteenish { get; } = TemplateThemes.Sixteenish;

        internal static class TemplateThemes
        {
            public static TemplateTheme Sixteenish { get; } = new(
                new Dictionary<TemplateThemeStyle, string>
                {
                    [TemplateThemeStyle.Text] = FGColor("ghwhite"),
                    [TemplateThemeStyle.SecondaryText] = FGCode(121, 192, 255),
                    [TemplateThemeStyle.TertiaryText] = FGColor("darkgoldenrod"),
                    [TemplateThemeStyle.Invalid] = "\x1b[33m",
                    [TemplateThemeStyle.Null] = "\x1b[34m",
                    [TemplateThemeStyle.Name] = "\u001b[38;5;0081m",
                    [TemplateThemeStyle.String] = FGCode(210, 168, 255),
                    [TemplateThemeStyle.Number] = FGColor("ghred"),
                    [TemplateThemeStyle.Boolean] = "\x1b[34m",
                    [TemplateThemeStyle.Scalar] = "\x1b[32m",
                    [TemplateThemeStyle.LevelVerbose] = "\x1b[30;1m",
                    [TemplateThemeStyle.LevelDebug] = "\x1b[1m",
                    [TemplateThemeStyle.LevelInformation] = "\x1b[36;1m",
                    [TemplateThemeStyle.LevelWarning] = "\x1b[33;1m",
                    [TemplateThemeStyle.LevelError] = "\x1b[31;1m",
                    [TemplateThemeStyle.LevelFatal] = "\x1b[31;1m",
                });
        }
    }
}
