using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{
    //References
    PlayerInputAction playerInputAction;
    CharacterController characterController;
    Animator animator;

    //AnimatorHash for Performance
    int isWalkingHash;
    int isRunningHash;

    //Player input values
    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 currentRunMovement;
    bool isMovementPressed;
    bool isRunPressed;
    float rotationSpeed = 15f;

    //Changleable variables
    [SerializeField] private float runSpeed = 3.0f;

    private void Awake()
    {
        //Get References
        playerInputAction = new PlayerInputAction();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        //AnimatorHash for Performance
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");

        //Input System Callbacks
        playerInputAction.CharacterControls.Move.started += MovementActionCallback;
        playerInputAction.CharacterControls.Move.canceled += MovementActionCallback;
        playerInputAction.CharacterControls.Move.performed += MovementActionCallback;
        playerInputAction.CharacterControls.Run.started += RunActionCallback;
        playerInputAction.CharacterControls.Run.canceled += RunActionCallback;
    }

    void RunActionCallback(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }

    void MovementActionCallback(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();

        //Player X ve Z axis'inde hareket ediyor.
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;//Unity'de Y vektörü vertical position olduðu için Z axis'inde saklýyoruz.
        currentRunMovement.x = currentMovementInput.x * runSpeed;
        currentRunMovement.z = currentMovementInput.y * runSpeed;//Unity'de Y vektörü vertical position olduðu için Z axis'inde saklýyoruz.
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;//Sýfýr dýþýnda bir value geldiyse hareket etme tuþuna basýlmýþtýr.
    }

    void HandleGravity()
    {
        //CharacterController yere deðiyorsa, küçük bir gravity veriyoruz ki floating durumunda kalmasýn.
        if (characterController.isGrounded)
        {
            float groundedGravity = -.05f;
            currentMovement.y = currentRunMovement.y = groundedGravity;
        }
        //Diðer durumlarda Gravity uyguluyoruz.
        else
        {
            float gravity = -9.8f;
            currentMovement.y += gravity;
            currentRunMovement.y += gravity;
        }
    }

    void HandleRotation()
    {
        Vector3 positionToLookAt;
        //Player hareket ettiði yöne doðru bakmalý
        positionToLookAt = new Vector3(currentMovement.x, 0f, currentMovement.z);

        Quaternion currentRotation = transform.rotation;

        if (isMovementPressed)//Hareket etmiyorken dönmüyoruz.
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            //Snappy dönüþ hareketleri olmamasý için Lerp kullandýk.
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
    }

    void HandleAnimation()
    {
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);

        //Eðer hareket tuþuna basýlmýþsa ve karakter yürümüyorsa, yürüme animasyonu çalýþýr.
        if (isMovementPressed && !isWalking)
        {
            animator.SetBool(isWalkingHash, true);
        }
        //Eðer hareket tuþuna basýlmamýþsa ve karakter yürüyorsa, yürüme animasyonu durdurulur.
        else if (!isMovementPressed && isWalking)
        {
            animator.SetBool(isWalkingHash, false);
        }

        //Eðer hem hareket tuþuna hem de koþma tuþuna basýlmýþsa, ve karakter buna raðmen koþmuyorsa, koþma animasyonu çalýþýr.
        if ((isMovementPressed && isRunPressed) && !isRunning)
        {
            animator.SetBool(isRunningHash, true);
        }
        //Eðer hareket tuþuna veya koþma tuþuna basýlmamýþsa, ve karakter buna raðmen koþuyorsa, koþma animasyonu durdurulur.
        else if ((!isMovementPressed || !isRunPressed) && isRunning)
        {
            animator.SetBool(isRunningHash, false);
        }
    }

    void Update()
    {
        HandleGravity();
        HandleRotation();
        HandleAnimation();

        if (isRunPressed)
        {
            characterController.Move(currentRunMovement * Time.deltaTime);
        }
        else
        {
            characterController.Move(currentMovement * Time.deltaTime);
        }
        
    }


    private void OnEnable()
    {
        playerInputAction.CharacterControls.Enable();
    }

    private void OnDisable()
    {
        playerInputAction.CharacterControls.Disable();
    }
}
