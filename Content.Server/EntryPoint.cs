using JetBrains.Annotations;
using Robust.Server.ServerStatus;
using Robust.Shared.ContentPack;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

// DEVNOTE: Games that want to be on the hub can change their namespace prefix in the "manifest.yml" file.
namespace Content.Server;

[UsedImplicitly]
public sealed class EntryPoint : GameServer
{
    public override void Init() {
        base.Init();

        // Configure ACZ correctly.
        IoCManager.Resolve<IStatusHost>().SetAczInfo(
            "Content.Client", new []{"Content.Client", "Content.Shared"});
        
        var factory = IoCManager.Resolve<IComponentFactory>();

        factory.DoAutoRegistrations();

        foreach (var ignoreName in IgnoredComponents.List)
        {
            factory.RegisterIgnore(ignoreName);
        }

        ServerContentIoC.Register();

        IoCManager.BuildGraph();
            
        factory.GenerateNetIds();

        // DEVNOTE: This is generally where you'll be setting up the IoCManager further.
    }
}