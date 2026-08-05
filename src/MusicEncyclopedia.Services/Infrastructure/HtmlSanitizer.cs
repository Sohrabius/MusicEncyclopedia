using System.Net;
using System.Text.RegularExpressions;

namespace MusicEncyclopedia.Services.Infrastructure;

/// <summary>
/// Lightweight HTML sanitizer that strips dangerous tags and attributes.
/// Only allows a predefined set of safe tags and protocols (spec 17.3).
/// </summary>
public static class HtmlSanitizer
{
    // Allowed HTML tags per spec Section 17.3
    private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "p", "br", "strong", "em", "u", "blockquote",
        "ul", "ol", "li", "a", "h2", "h3", "h4", "span"
    };

    // Safe URL protocols
    private static readonly HashSet<string> AllowedProtocols = new(StringComparer.OrdinalIgnoreCase)
    {
        "http", "https", "mailto"
    };

    // Removes <script>...</script> and <style>...</style> blocks entirely (including content)
    private static readonly Regex ScriptStyleRegex = new(
        @"<script\b[^>]*>.*?</script\s*>|<style\b[^>]*>.*?</style\s*>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    // Removes event handler attributes: onclick, onload, onerror, etc.
    private static readonly Regex EventHandlerRegex = new(
        @"\s+on\w+\s*=\s*""[^""]*""",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Matches any HTML tag (opening, closing, self-closing)
    private static readonly Regex TagRegex = new(
        @"</?([a-zA-Z0-9]+)(?:\s[^>]*)?\s*/?>",
        RegexOptions.Compiled);

    // Extracts the href attribute value from an anchor tag
    private static readonly Regex HrefRegex = new(
        @"href\s*=\s*""([^""]+)""",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Sanitizes the provided HTML string, removing dangerous tags and attributes.
    /// </summary>
    /// <param name="html">The raw HTML input.</param>
    /// <returns>Safe HTML containing only allowed tags with properly encoded content.</returns>
    public static string Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        // Step 1: Remove script and style blocks entirely (including their content)
        var result = ScriptStyleRegex.Replace(html, string.Empty);

        // Step 2: Remove event handler attributes (onclick, onload, onerror, etc.)
        result = EventHandlerRegex.Replace(result, string.Empty);

        // Step 3: Process each HTML tag — keep allowed ones, encode everything else
        result = TagRegex.Replace(result, ReplaceTag);

        return result;
    }

    /// <summary>
    /// Callback for TagRegex that either keeps the tag (if allowed) or HTML-encodes it.
    /// </summary>
    private static string ReplaceTag(Match match)
    {
        var fullTag = match.Value;
        var tagName = match.Groups[1].Value;

        // Non-allowed tags are HTML-encoded to render as safe text
        if (!AllowedTags.Contains(tagName))
            return WebUtility.HtmlEncode(fullTag);

        // Closing tag — keep as-is (safe)
        if (fullTag.StartsWith("</", StringComparison.OrdinalIgnoreCase))
            return $"</{tagName}>";

        // Self-closing check
        var isSelfClosing = fullTag.TrimEnd().EndsWith("/>", StringComparison.OrdinalIgnoreCase);

        // Anchor tags require special href handling
        if (string.Equals(tagName, "a", StringComparison.OrdinalIgnoreCase))
            return SanitizeAnchorTag(fullTag, isSelfClosing);

        // Other allowed tags — strip all attributes
        return isSelfClosing ? $"<{tagName} />" : $"<{tagName}>";
    }

    /// <summary>
    /// Sanitizes an anchor tag: only the href attribute is preserved, and only
    /// if it uses an allowed protocol (http, https, mailto).
    /// </summary>
    private static string SanitizeAnchorTag(string fullTag, bool isSelfClosing)
    {
        var hrefMatch = HrefRegex.Match(fullTag);

        if (!hrefMatch.Success)
        {
            // No href attribute — return a bare anchor tag (still functional for anchoring)
            return isSelfClosing ? "<a />" : "<a>";
        }

        var href = hrefMatch.Groups[1].Value;

        // Validate the URL scheme / protocol
        var colonIndex = href.IndexOf(':');
        if (colonIndex > 0)
        {
            var protocol = href[..colonIndex].Trim().ToLowerInvariant();
            if (!AllowedProtocols.Contains(protocol))
            {
                // Disallowed protocol — encode the entire tag as safe text
                return WebUtility.HtmlEncode(fullTag);
            }
        }

        // Safe href — preserve the href attribute with HTML-encoded value
        return $"<a href=\"{WebUtility.HtmlEncode(href)}\">";
    }
}
