

public class PlayerRunState : PlayerBaseState
{

    public PlayerRunState(PlayerStateMachine context, PlayerStateFactory playerStateFactory) : base(context, playerStateFactory) { }

    public override void CheckSwitchStates()
    {
        if (!ctx.IsMovementPressed)
        {
            SwitchState(factory.Idle());
        }
        else if (ctx.IsMovementPressed && !ctx.IsRunPressed)
        {
            SwitchState(factory.Walk());
        }
    }

    public override void EnterState()
    {
        ctx.Animator.SetBool(ctx.IsWalkingHash, true);
        ctx.Animator.SetBool(ctx.IsRunningHash, true);
    }

    public override void ExitState()
    {
        
    }

    public override void InitializeSubState()
    {
        
    }

    public override void UpdateState()
    {
        
        ctx.AppliedMovementX = ctx.CurrentMovementInput.x * ctx.RunSpeed;
        ctx.AppliedMovementZ = ctx.CurrentMovementInput.y * ctx.RunSpeed;
        CheckSwitchStates();
    }
}
