using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{


    public PlayerWalkState(PlayerStateMachine context, PlayerStateFactory playerStateFactory) : base(context, playerStateFactory) { }

    public override void CheckSwitchStates()
    {
        if (!ctx.IsMovementPressed)
        {
            SwitchState(factory.Idle());
        }
        else if (ctx.IsMovementPressed && ctx.IsRunPressed)
        {
            SwitchState(factory.Run());
        }
    }

    public override void EnterState()
    {
        ctx.Animator.SetBool(ctx.IsWalkingHash, true);
        ctx.Animator.SetBool(ctx.IsRunningHash, false);
    }

    public override void ExitState()
    {
        
    }

    public override void InitializeSubState()
    {
       
    }

    public override void UpdateState()
    {
        
        ctx.AppliedMovementX = ctx.CurrentMovementInput.x;
        ctx.AppliedMovementZ = ctx.CurrentMovementInput.y;
        CheckSwitchStates();
    }
}
