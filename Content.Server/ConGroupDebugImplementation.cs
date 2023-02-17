using Robust.Server.Console;
using Robust.Server.Player;
using Robust.Shared.IoC;

namespace Content.Server;

#if DEBUG
public sealed class ConGroupDebugImplementation : IConGroupControllerImplementation, IPostInjectInit
{
    [Dependency] private readonly IConGroupController _conGroup = default!;
    
    public bool CanCommand(IPlayerSession session, string cmdName) => true;
    public bool CanViewVar(IPlayerSession session) => true;
    public bool CanAdminPlace(IPlayerSession session) => true;
    public bool CanScript(IPlayerSession session) => true;
    public bool CanAdminMenu(IPlayerSession session) => true;
    public bool CanAdminReloadPrototypes(IPlayerSession session) => true;
    public void PostInject()
    {
        _conGroup.Implementation = this;
    }
}
#endif