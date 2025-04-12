namespace DevToys.Extensions;

[Export(typeof(IResourceAssemblyIdentifier))]
[Name(nameof(DevToysExtensionsResourceManagerAssemblyIdentifier))]
internal sealed class DevToysExtensionsResourceManagerAssemblyIdentifier : IResourceAssemblyIdentifier
{
    public ValueTask<FontDefinition[]> GetFontDefinitionsAsync()
    {
        throw new NotImplementedException();
    }
}

