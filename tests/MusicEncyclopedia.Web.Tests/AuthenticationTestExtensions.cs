using System.Net;
using System.Text.RegularExpressions;

namespace MusicEncyclopedia.Web.Tests;

internal static partial class AuthenticationTestExtensions
{
    public static async Task LoginAsync(this HttpClient client, string email, string password)
    {
        var token = await client.GetAntiForgeryTokenAsync("/auth/login");

        var response = await client.PostAsync("/auth/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = password,
            ["RememberMe"] = "false",
            ["__RequestVerificationToken"] = token
        }));

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
    }

    public static async Task<string> GetAntiForgeryTokenAsync(this HttpClient client, string path)
    {
        var page = await client.GetAsync(path);
        page.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await page.Content.ReadAsStringAsync();
        var match = AntiForgeryTokenRegex().Match(html);
        match.Success.Should().BeTrue($"the form at {path} must contain an antiforgery token");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"", RegexOptions.IgnoreCase)]
    private static partial Regex AntiForgeryTokenRegex();
}
