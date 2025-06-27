
namespace NoDev.Infinity.Build.InfinityBuilder.Manifests
{
    internal interface IManifestCompiler
    {
        string CompileForClient(GameManifest gameManifest, ToolManifest toolManifest);

        string CompileForServer(GameManifest gameManifest, ToolManifest toolManifest);
    }
}
