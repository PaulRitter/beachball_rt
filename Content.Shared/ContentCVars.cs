using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared;

[CVarDefs]
public sealed class ContentCVars: CVars
{
    // ----- BALL CVARS -----
    /// <summary>
    ///     Factor by which the ball gravity will be multiplied.
    /// </summary>
    public static readonly CVarDef<float> BallGravityMultiplier =
        CVarDef.Create("ball.gravity", 0.7f, CVar.REPLICATED | CVar.SERVER);

    /// <summary>
    ///     Factor by which the calculated bounce vector will be multiplied
    /// </summary>
    public static readonly CVarDef<float> BallBounceVectorMultiplier =
        CVarDef.Create("ball.bounce_vector_mult", 2f, CVar.REPLICATED | CVar.SERVER);

    /// <summary>
    ///     Factor by which the own velocity will be multiplied on bounce
    /// </summary>
    public static readonly CVarDef<float> BallBounceOwnVelocityMultiplier =
        CVarDef.Create("ball.bounce_own_velocity_mult", 2f, CVar.REPLICATED | CVar.SERVER);
    
    /// <summary>
    ///     Factor by which the other velocity will be multiplied on bounce
    /// </summary>
    public static readonly CVarDef<float> BallBounceOtherVelocityMultiplier =
        CVarDef.Create("ball.bounce_other_velocity_mult", 2f, CVar.REPLICATED | CVar.SERVER);
    
    /// <summary>
    ///     Factor by which the other velocity will be multiplied on bounce
    /// </summary>
    public static readonly CVarDef<float> BallBounceWallMultiplier =
        CVarDef.Create("ball.bounce_wall_mult", 0.6f, CVar.REPLICATED | CVar.SERVER);
        
    // ----- PLAYER CVARS -----
        
    /// <summary>
    ///     Factor by which the player gravity will be multiplied.
    /// </summary>
    public static readonly CVarDef<float> PlayerGravityMultiplier =
        CVarDef.Create("player.gravity", 1f, CVar.REPLICATED | CVar.SERVER);

    // ----- GAME CVARS -----
        
    /// <summary>
    ///     Score needed to win.
    /// </summary>
    public static readonly CVarDef<int> GameWinScore =
        CVarDef.Create("game.win_score", 11, CVar.SERVERONLY | CVar.SERVER);
    
    /// <summary>
    ///     Seconds to wait after the game has ended before restarting.
    /// </summary>
    public static readonly CVarDef<float> GameRestartTimer =
        CVarDef.Create("game.restart_timer", 1f, CVar.SERVERONLY | CVar.SERVER);
}