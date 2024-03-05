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
        currentMovement.z = currentMovementInput.y;//Unity'de Y vekt�r� vertical position oldu�u i�in Z axis'inde sakl�yoruz.
        currentRunMovement.x = currentMovementInput.x * runSpeed;
        currentRunMovement.z = currentMovementInput.y * runSpeed;//Unity'de Y vekt�r� vertical position oldu�u i�in Z axis'inde sakl�yoruz.
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;//S�f�r d���nda bir value geldiyse hareket etme tu�una bas�lm��t�r.
    }

    void HandleGravity()
    {
        //CharacterController yere de�iyorsa, k���k bir gravity veriyoruz ki floating durumunda kalmas�n.
        if (characterController.isGrounded)
        {
            float groundedGravity = -.05f;
            currentMovement.y = currentRunMovement.y = groundedGravity;
        }
        //Di�er durumlarda Gravity uyguluyoruz.
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
        //Player hareket etti�i y�ne do�ru bakmal�
        positionToLookAt = new Vector3(currentMovement.x, 0f, currentMovement.z);

        Quaternion currentRotation = transform.rotation;

        if (isMovementPressed)//Hareket etmiyorken d�nm�yoruz.
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            //Snappy d�n�� hareketleri olmamas� i�in Lerp kulland�k.
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
    }

    void HandleAnimation()
    {
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);

        //E�er hareket tu�una bas�lm��sa ve karakter y�r�m�yorsa, y�r�me animasyonu �al���r.
        if (isMovementPressed && !isWalking)
        {
            animator.SetBool(isWalkingHash, true);
        }
        //E�er hareket tu�una bas�lmam��sa ve karakter y�r�yorsa, y�r�me animasyonu durdurulur.
        else if (!isMovementPressed && isWalking)
        {
            animator.SetBool(isWalkingHash, false);
        }

        //E�er hem hareket tu�una hem de ko�ma tu�una bas�lm��sa, ve karakter buna ra�men ko�muyorsa, ko�ma animasyonu �al���r.
        if ((isMovementPressed && isRunPressed) && !isRunning)
        {
            animator.SetBool(isRunningHash, true);
        }
        //E�er hareket tu�una veya ko�ma tu�una bas�lmam��sa, ve karakter buna ra�men ko�uyorsa, ko�ma animasyonu durdurulur.
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
