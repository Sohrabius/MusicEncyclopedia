// The class must live in the ROOT namespace: with ResourcesPath = "Resources",
// ResourceManagerStringLocalizerFactory computes the resource base name as
// "{type namespace}.{ResourcesPath}.{type name}", which must equal the embedded
// "MusicEncyclopedia.Web.Resources.SharedResources" resource set.
// The explicit <LogicalName> entries in the .csproj guarantee that match.
namespace MusicEncyclopedia.Web;

/// <summary>
/// Type marker for the shared UI resources.
/// <see cref="Microsoft.Extensions.Localization.IStringLocalizer{T}"/> resolves this
/// type's full name to the embedded "MusicEncyclopedia.Web.Resources.SharedResources"
/// resource set (see the matching .resx files in this folder and the <c>LogicalName</c>
/// entries in MusicEncyclopedia.Web.csproj).
/// </summary>
public sealed class SharedResources
{
    private SharedResources() { }
}
