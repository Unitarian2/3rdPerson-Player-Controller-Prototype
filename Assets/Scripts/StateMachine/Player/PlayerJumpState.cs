using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    IEnumerator JumpResetTimer()
    {
        yield return new WaitForSeconds(0.5f);
        ctx.JumpCount = 0;
    }
    public PlayerJumpState(PlayerStateMachine context, PlayerStateFactory playerStateFactory) : base(context, playerStateFactory)
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
        HandleJump();
    }

    public override void ExitState()
    {
        ctx.Animator.SetBool(ctx.IsJumpingHash, false);
        if (ctx.IsJumpPressed)
        {
            ctx.RequireNewJumpPress = true;
        }
        

        //Jump durduktan bir süre sonra jumpCount sýfýrlanýyor. Böylece jump combosu resetlenmiþ oluyor.
        ctx.JumpResetCoroutine = ctx.StartCoroutine(JumpResetTimer());
    }

    public override void InitializeSubState()
    {
        if (!ctx.IsMovementPressed && ctx.IsRunPressed)
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

    public void HandleJump()
    {
        //Jump combosu tamamlanmamýþsa ResetTimer durduruyoruz ki jumpCount combonun ortasýnda sýfýrlanmasýn.
        if (ctx.JumpCount < 3 && ctx.JumpResetCoroutine != null)
        {
            ctx.StopCoroutine(ctx.JumpResetCoroutine);
        }
        //Dictionary dýþýna çýkmamak için ekledik
        else if (ctx.JumpCount >= 3)
        {
            ctx.JumpCount = 0;
            if (ctx.JumpResetCoroutine != null) ctx.StopCoroutine(ctx.JumpResetCoroutine);
        }



        //Animation baþlýyor.
        ctx.Animator.SetBool(ctx.IsJumpingHash, true);

        ctx.IsJumping = true;
        ctx.JumpCount += 1;
        ctx.Animator.SetInteger(ctx.JumpCountHash, ctx.JumpCount);

        //HandleGravity'de Velocity Verlet(yarým adýmlýk) hesaplama yaptýðýmýz için, initialJumpVelocity'yi yarý yarýya düþürdük.
        ctx.CurrentMovementY = ctx.InitialJumpVelocities[ctx.JumpCount];
        ctx.AppliedMovementY = ctx.InitialJumpVelocities[ctx.JumpCount];

    }

    void HandleGravity()
    {
        //sadece havaya yükselirken y 0'dan büyük olur.
        //Jump butonuna basmayý býrakýrsak küçük jump(fallMultiplier 1f olduðu için), basýlý tutarsak ise full jump(fallMultiplier arttýðý için)
        bool isFalling = ctx.CurrentMovementY <= 0f || !ctx.IsJumpPressed;

        //Düþüyorsak, public fallspeed deðeri kullanýyoruz. Yoksa 1.0f default deðer kullanýyoruz. Düþme hýzýný artýrmak için kullanýlýr.
        float fallMultiplier = ctx.FallSpeed;

        
        //Player düþüþte. Else ile kod tekrarý olsa da ilerde buraya düþüþ anýna özel kodlar eklenebilir o nedenle bu þekilde býraktýk.
        if (isFalling)
        {
            //Burada Velocity Verlet entegrasyonu kullandýk. Yarým adýmlýk hýz güncellemesi yaptýk böylece frame rate farkýndan doðacak sapmalardan kurtulmak amaçlanýyor.
            float previousYVelocity = ctx.CurrentMovementY;
            ctx.CurrentMovementY = ctx.CurrentMovementY + (ctx.JumpGravities[ctx.JumpCount] * fallMultiplier * Time.deltaTime);

            //NextYVelocity Clamp'liyoruz, yüksek mesafeden aþaðý düþüyorsak fallMultiplier'ýmýz bizi aþýrý hýzlandýrmasýn diye. -20f düþerken ulaþacaðýmýz max düþme hýzý
            ctx.AppliedMovementY = Mathf.Max((previousYVelocity + ctx.CurrentMovementY) * 0.5f, -20.0f);
        }
        //Diðer durumlarda Gravity uyguluyoruz.
        else
        {
            //Burada Velocity Verlet entegrasyonu kullandýk. Yarým adýmlýk hýz güncellemesi yaptýk böylece frame rate farkýndan doðacak sapmalardan kurtulmak amaçlanýyor.
            float previousYVelocity = ctx.CurrentMovementY;
            ctx.CurrentMovementY = ctx.CurrentMovementY + (ctx.JumpGravities[ctx.JumpCount] * Time.deltaTime);
            ctx.AppliedMovementY = (previousYVelocity + ctx.CurrentMovementY) * 0.5f;
        }
    }
}
