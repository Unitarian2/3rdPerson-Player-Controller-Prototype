
using UnityEngine;

public class PlayerFallState : PlayerBaseState
{
    public PlayerFallState(PlayerStateMachine context, PlayerStateFactory playerStateFactory) : base(context, playerStateFactory)
    {
        isRootState = true;
        
    }

    public override void CheckSwitchStates()
    {
        if (ctx.CharacterController.isGrounded)
        {
            SwitchState(factory.Grounded());
        }
    }

    public override void EnterState()
    {
        //Sub State'i EnterState'de çaðýrýyoruz çünkü StateFactory state'leri bir dictionary'de cache'liyor. Bu state'e her girdiðimiz'de Substate'lerden birini seçmemiz gerekli.
        InitializeSubState();
        ctx.Animator.SetBool(ctx.IsFallingHash, true);
    }

    public override void ExitState()
    {
        ctx.Animator.SetBool(ctx.IsFallingHash, false);
    }

    public void HandleGravity()
    {
        float previousYVelocity = ctx.CurrentMovementY;
        ctx.CurrentMovementY = ctx.CurrentMovementY + ctx.Gravity * Time.deltaTime;
        ctx.AppliedMovementY = Mathf.Max((previousYVelocity + ctx.CurrentMovementY) * 0.5f, -20f);
    }
    public override void InitializeSubState()
    {
        if (!ctx.IsMovementPressed && !ctx.IsRunPressed)
        {
            SetSubState(factory.Idle());
        }
        else if (ctx.IsMovementPressed && !ctx.IsRunPressed)
        {
            SetSubState(factory.Walk());
        }
        else
        {
            SetSubState(factory.Run());
        }
    }

    public override void UpdateState()
    {
        
        HandleGravity();
        CheckSwitchStates();
    }
}

