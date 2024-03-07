

using System.Collections.Generic;

public class PlayerStateFactory
{
    PlayerStateMachine context;
    Dictionary<PlayerStates, PlayerBaseState> states = new();

    public PlayerStateFactory(PlayerStateMachine context)
    {
        this.context = context;
        states[PlayerStates.Idle] = new PlayerIdleState(context, this);
        states[PlayerStates.Walk] = new PlayerWalkState(context, this);
        states[PlayerStates.Run] = new PlayerRunState(context, this);
        states[PlayerStates.Jump] = new PlayerJumpState(context, this);
        states[PlayerStates.Grounded] = new PlayerGroundedState(context, this);
        states[PlayerStates.Fall] = new PlayerFallState(context, this);
    }

    public PlayerBaseState Idle() { return states[PlayerStates.Idle]; }
    public PlayerBaseState Walk() { return states[PlayerStates.Walk]; }
    public PlayerBaseState Run() { return states[PlayerStates.Run]; }
    public PlayerBaseState Jump() { return states[PlayerStates.Jump]; }
    public PlayerBaseState Grounded() { return states[PlayerStates.Grounded]; }
    public PlayerBaseState Fall() { return states[PlayerStates.Fall]; }
}

public enum PlayerStates
{
    Idle,Walk, Run, Jump, Grounded, Fall
}
