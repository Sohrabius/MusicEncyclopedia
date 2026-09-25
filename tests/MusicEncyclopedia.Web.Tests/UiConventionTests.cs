using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MusicEncyclopedia.Web.Tests;

public sealed partial class UiConventionTests
{
    [Fact]
    public void RazorViews_UseSemanticColorUtilities()
    {
        var root = FindSolutionRoot();
        var webRoot = Path.Combine(root.FullName, "src", "MusicEncyclopedia.Web");
        var viewRoots = new[]
        {
            Path.Combine(webRoot, "Views"),
            Path.Combine(webRoot, "Areas", "Admin", "Views")
        };

        var violations = viewRoots
            .SelectMany(path => Directory.EnumerateFiles(path, "*.cshtml", SearchOption.AllDirectories))
            .SelectMany(path => RawPaletteUtility().Matches(File.ReadAllText(path))
                .Select(match => $"{Path.GetRelativePath(root.FullName, path)}: {match.Value}"))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        violations.Should().BeEmpty(
            "view colors must use the semantic palette configured in wwwroot/js/tailwind-theme.js");
    }

    [Fact]
    public void PublicAndAdminLayouts_LoadTheSharedSemanticTheme()
    {
        var root = FindSolutionRoot();
        var webRoot = Path.Combine(root.FullName, "src", "MusicEncyclopedia.Web");
        var layouts = new[]
        {
            Path.Combine(webRoot, "Views", "Shared", "_Layout.cshtml"),
            Path.Combine(webRoot, "Areas", "Admin", "Views", "Shared", "_AdminLayout.cshtml")
        };

        foreach (var layout in layouts)
        {
            var markup = File.ReadAllText(layout);
            markup.Should().Contain("~/js/tailwind-theme.js");
            markup.Should().Contain("~/css/site.css");
        }
    }

    [Fact]
    public void LaunchLayouts_ExposePersianRtlKeyboardNavigation()
    {
        var root = FindSolutionRoot();
        var webRoot = Path.Combine(root.FullName, "src", "MusicEncyclopedia.Web");
        var publicLayout = File.ReadAllText(Path.Combine(webRoot, "Views", "Shared", "_Layout.cshtml"));
        var adminLayout = File.ReadAllText(Path.Combine(webRoot, "Areas", "Admin", "Views", "Shared", "_AdminLayout.cshtml"));
        var siteScript = File.ReadAllText(Path.Combine(webRoot, "wwwroot", "js", "site.js"));

        publicLayout.Should().Contain("href=\"#main-content\"");
        publicLayout.Should().Contain("id=\"main-content\"");
        publicLayout.Should().Contain("aria-controls=\"mobile-nav\"");
        publicLayout.Should().Contain("aria-hidden=\"true\" inert");

        adminLayout.Should().Contain("<html dir=\"rtl\" lang=\"fa\"");
        adminLayout.Should().Contain("href=\"#main-content\"");
        adminLayout.Should().Contain("id=\"main-content\"");
        adminLayout.Should().Contain("aria-controls=\"sidebar\"");
        adminLayout.Should().Contain("event.key === 'Escape'");
        adminLayout.Should().Contain("sidebar.inert");

        siteScript.Should().Contain("e.key === 'Escape'");
        siteScript.Should().Contain("mobile.inert");
        siteScript.Should().Contain("prefers-reduced-motion: reduce");
        siteScript.Should().Contain("بازگشت به بالای صفحه");
    }

    [Fact]
    public void RazorForms_UseKeyboardFocusAndAnnounceFieldErrors()
    {
        var root = FindSolutionRoot();
        var webRoot = Path.Combine(root.FullName, "src", "MusicEncyclopedia.Web");
        var views = Directory.EnumerateFiles(webRoot, "*.cshtml", SearchOption.AllDirectories)
            .Select(path => (Path: path, Markup: File.ReadAllText(path)))
            .ToArray();

        var pointerFocusViolations = views
            .Where(view => PointerFocusUtility().IsMatch(view.Markup))
            .Select(view => Path.GetRelativePath(root.FullName, view.Path))
            .ToArray();
        pointerFocusViolations.Should().BeEmpty("focus rings should appear for keyboard focus via focus-visible");

        var errorAnnouncementViolations = views
            .SelectMany(view => ValidationSpan().Matches(view.Markup)
                .Where(match => !match.Value.Contains("aria-live=\"polite\"", StringComparison.Ordinal))
                .Select(_ => Path.GetRelativePath(root.FullName, view.Path)))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        errorAnnouncementViolations.Should().BeEmpty("field validation messages must be announced by assistive technology");

        var decorativeIconViolations = views
            .Where(view => UnhiddenIcon().IsMatch(view.Markup))
            .Select(view => Path.GetRelativePath(root.FullName, view.Path))
            .ToArray();
        decorativeIconViolations.Should().BeEmpty("Font Awesome glyphs are decorative and must not pollute accessible names");

        var iconButtonViolations = views
            .SelectMany(view => IconOnlyButton().Matches(view.Markup)
                .Where(match => !(match.Groups["attributes"].Value.Contains("aria-label=", StringComparison.Ordinal)
                        || match.Groups["attributes"].Value.Contains("title=", StringComparison.Ordinal))
                    || !match.Groups["attributes"].Value.Contains("min-w-11", StringComparison.Ordinal)
                    || !match.Groups["attributes"].Value.Contains("min-h-11", StringComparison.Ordinal))
                .Select(_ => Path.GetRelativePath(root.FullName, view.Path)))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        iconButtonViolations.Should().BeEmpty("icon-only buttons need a Persian accessible name and a 44px touch target");
    }

    [Fact]
    public void SharedResourceKeys_AreUniqueIgnoringCase()
    {
        var root = FindSolutionRoot();
        var resourceRoot = Path.Combine(root.FullName, "src", "MusicEncyclopedia.Web", "Resources");

        foreach (var path in Directory.EnumerateFiles(resourceRoot, "SharedResources*.resx"))
        {
            var duplicates = XDocument.Load(path)
                .Descendants("data")
                .Select(element => element.Attribute("name")?.Value)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .GroupBy(name => name!, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToArray();

            duplicates.Should().BeEmpty($"{Path.GetFileName(path)} must build without case-insensitive duplicate resource warnings");
        }
    }

    [Fact]
    public void TextTokens_MeetWcagAaContrastOnSurface()
    {
        var root = FindSolutionRoot();
        var css = File.ReadAllText(Path.Combine(root.FullName, "src", "MusicEncyclopedia.Web", "wwwroot", "css", "site.css"));
        var colors = CssColorToken().Matches(css)
            .ToDictionary(match => match.Groups[1].Value, match => match.Groups[2].Value, StringComparer.Ordinal);
        var foregroundTokens = new[] { "ink", "ink-2", "ink-3", "accent", "error", "success" };

        foreach (var token in foregroundTokens)
        {
            ContrastRatio(colors[token], colors["surface"]).Should().BeGreaterThanOrEqualTo(4.5,
                $"--color-{token} is used for normal text on --color-surface");
        }
    }

    private static double ContrastRatio(string foreground, string background)
    {
        static double Luminance(string hex)
        {
            var channels = Enumerable.Range(0, 3)
                .Select(index => Convert.ToInt32(hex.Substring(1 + index * 2, 2), 16) / 255d)
                .Select(value => value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4))
                .ToArray();
            return 0.2126 * channels[0] + 0.7152 * channels[1] + 0.0722 * channels[2];
        }

        var first = Luminance(foreground);
        var second = Luminance(background);
        return (Math.Max(first, second) + 0.05) / (Math.Min(first, second) + 0.05);
    }

    private static DirectoryInfo FindSolutionRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "MusicEncyclopedia.sln")))
            {
                return directory;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the MusicEncyclopedia solution root.");
    }

    [GeneratedRegex(
        @"(?:bg|text|border|ring|divide|placeholder|decoration|from|to|via)-(?:white|black|slate|gray|zinc|neutral|stone|red|orange|amber|yellow|lime|green|emerald|teal|cyan|sky|blue|indigo|violet|purple|fuchsia|pink|rose)(?:-[0-9]{2,3})?(?:/[0-9]+)?",
        RegexOptions.CultureInvariant)]
    private static partial Regex RawPaletteUtility();

    [GeneratedRegex(@"(?<!visible)focus:(?:outline-none|ring(?:-[^\s\""']+)?|border(?:-[^\s\""']+)?)", RegexOptions.CultureInvariant)]
    private static partial Regex PointerFocusUtility();

    [GeneratedRegex(@"<span\b[^>]*asp-validation-for\s*=\s*\""[^\""']+\""[^>]*>", RegexOptions.CultureInvariant)]
    private static partial Regex ValidationSpan();

    [GeneratedRegex(@"--color-([a-z0-9-]+):\s*(#[0-9A-Fa-f]{6})\s*;", RegexOptions.CultureInvariant)]
    private static partial Regex CssColorToken();

    [GeneratedRegex(@"<i(?=\s|>)(?![^>]*aria-hidden\s*=\s*[""']true[""'])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex UnhiddenIcon();

    [GeneratedRegex(@"<button\b(?<attributes>[^>]*)>\s*<i\b[^>]*>\s*</i>\s*</button>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex IconOnlyButton();
}
