using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerBaseState, IRootState
{


    public PlayerGroundedState(PlayerStateMachine context, PlayerStateFactory playerStateFactory) : base(context, playerStateFactory)
    { 
        isRootState = true;
        
    }

    public void HandleGravity()
    {
        ctx.CurrentMovementY = ctx.AppliedMovementY = ctx.Gravity;
    }

    public override void CheckSwitchStates()
    {
        //Debug.Log(ctx.CharacterController.isGrounded);
        //Zýplama tuþuna basýlmýþsa, State deðiþiyoruz.
        if (ctx.IsJumpPressed && !ctx.RequireNewJumpPress)
        {
            SwitchState(factory.Jump());
        }
        //Zýplama tuþuna basýlmamýþsa ve player yerde deðilse, fall state geçiyoruz.
        else if (!ctx.CharacterController.isGrounded)
        {
            
            SwitchState(factory.Fall());
        }
    }

    public override void EnterState()
    {
        //Sub State'i EnterState'de çaðýrýyoruz çünkü StateFactory state'leri bir dictionary'de cache'liyor. Bu state'e her girdiðimiz'de Substate'lerden birini seçmemiz gerekli.
        InitializeSubState();
        HandleGravity();
    }

    public override void ExitState()
    {
        
    }

    public override void InitializeSubState()
    {
        if(!ctx.IsMovementPressed && ctx.IsRunPressed)
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
        CheckSwitchStates();
    }
}
