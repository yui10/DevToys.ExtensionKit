namespace DevToys.ExtensionKit;

[Export(typeof(IResourceAssemblyIdentifier))]
[Name(nameof(DevToysExtensionKitResourceManagerAssemblyIdentifier))]
internal sealed class DevToysExtensionKitResourceManagerAssemblyIdentifier : IResourceAssemblyIdentifier
{
    public ValueTask<FontDefinition[]> GetFontDefinitionsAsync()
    {
        throw new NotImplementedException();
    }
}

