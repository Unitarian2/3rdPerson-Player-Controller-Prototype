using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
    //References
    PlayerInputAction playerInputAction;
    CharacterController characterController;
    Animator animator;

    //AnimatorHash for Performance
    int isWalkingHash;
    int isRunningHash;
    int isJumpingHash;
    int jumpCountHash;
    int isFallingHash;

    //Player input values
    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 currentRunMovement;
    Vector3 appliedMovement;
    bool isMovementPressed;
    bool isRunPressed;
    float rotationSpeed = 15f;

    //Changleable variables
    [Header("-Run Settings-")]
    [SerializeField] private float runSpeed = 3.0f;
    [Header("-Jump Settings-")]
    public float maxJumpTime = 0.5f;
    public float maxJumpHeight = 1.0f;
    public float fallSpeed = 2.0f;

    //gravity
    float gravity = -9.8f;
    

    //jump variables
    bool isJumpPressed = false;
    float initialJumpVelocity;
    bool isJumping = false;
    bool requireNewJumpPress = false;
    int jumpCount = 0;
    Dictionary<int, float> initialJumpVelocities = new();
    Dictionary<int, float> jumpGravities = new();
    Coroutine jumpResetCoroutine = null;

    //States
    PlayerBaseState currentState;
    PlayerStateFactory states;

    //properties
    public CharacterController CharacterController => characterController;
    public PlayerBaseState CurrentState { get { return currentState; } set { currentState = value; } }
    public Animator Animator => animator;
    public Coroutine JumpResetCoroutine { get { return jumpResetCoroutine; } set { jumpResetCoroutine = value; } }
    public Dictionary<int,float> InitialJumpVelocities => initialJumpVelocities;
    public Dictionary<int,float> JumpGravities => jumpGravities;
    public int JumpCount { get { return jumpCount; } set { jumpCount = value; } }
    public int IsWalkingHash => isWalkingHash;
    public int IsRunningHash => isRunningHash;
    public int IsJumpingHash => isJumpingHash;
    public int IsFallingHash => isFallingHash;
    public int JumpCountHash => jumpCountHash;
    public bool RequireNewJumpPress { get { return requireNewJumpPress; } set { requireNewJumpPress = value; } }
    public bool IsJumping { set => isJumping = value; }
    public bool IsMovementPressed => isMovementPressed;
    public bool IsRunPressed => isRunPressed;
    public bool IsJumpPressed => isJumpPressed;
    public float RunSpeed => runSpeed;
    public float FallSpeed => fallSpeed;
    public float Gravity => gravity;
    public Vector2 CurrentMovementInput => currentMovementInput;
    public float CurrentMovementY { get { return currentMovement.y; } set { currentMovement.y = value; } }
    public float AppliedMovementX { get { return appliedMovement.x; } set { appliedMovement.x = value; } }
    public float AppliedMovementY { get { return appliedMovement.y; } set { appliedMovement.y = value; } }
    public float AppliedMovementZ { get { return appliedMovement.z; } set { appliedMovement.z = value; } }

    private void Awake()
    {
        //Get References
        playerInputAction = new PlayerInputAction();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        //setup state
        states = new PlayerStateFactory(this);
        currentState = states.Grounded();
        currentState.EnterState();

        //AnimatorHash for Performance
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");
        jumpCountHash = Animator.StringToHash("jumpCount");
        isFallingHash = Animator.StringToHash("isFalling");

        //Input System Callbacks
        playerInputAction.CharacterControls.Move.started += MovementActionCallback;
        playerInputAction.CharacterControls.Move.canceled += MovementActionCallback;
        playerInputAction.CharacterControls.Move.performed += MovementActionCallback;
        playerInputAction.CharacterControls.Run.started += RunActionCallback;
        playerInputAction.CharacterControls.Run.canceled += RunActionCallback;
        playerInputAction.CharacterControls.Jump.started += JumpActionCallback;
        playerInputAction.CharacterControls.Jump.canceled += JumpActionCallback;

        SetupJump();
    }

    private void SetupJump()
    {
        //Burada istenen jump deðerleri için hangi gravity ve hangi jump velocity ihtiyacýmýz var onu hesaplýyoruz.
        float timeToApex = maxJumpTime * 0.5f;
        float initialGravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;

        //2. ve 3. zýplama stillerinin Gravity ve Velocity deðerlerini ayarlýyoruz.
        float secondJumpGravity = (-2 * maxJumpHeight + 0.25f) / Mathf.Pow(timeToApex * 1.05f, 2);
        float secondInitialJumpVelocity = (2 * maxJumpHeight + 0.25f) / timeToApex * 1.05f;
        float thirdJumpGravity = (-2 * maxJumpHeight + 0.5f) / Mathf.Pow(timeToApex * 1.1f, 2);
        float thirdInitialJumpVelocity = (2 * maxJumpHeight + 0.5f) / timeToApex * 1.1f;

        //Dictionary'leri dolduruyoruz. jumpGravities'deki 0. eleman jump count resetlendiðinde kullanabilmek için
        initialJumpVelocities.Add(1, initialJumpVelocity);
        initialJumpVelocities.Add(2, secondInitialJumpVelocity);
        initialJumpVelocities.Add(3, thirdInitialJumpVelocity);
        jumpGravities.Add(0, initialGravity);
        jumpGravities.Add(1, initialGravity);
        jumpGravities.Add(2, secondJumpGravity);
        jumpGravities.Add(3, thirdJumpGravity);

    }
    private void Start()
    {
        characterController.Move(appliedMovement * Time.deltaTime);
    }

    void JumpActionCallback(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
        requireNewJumpPress = false;
    }

    void RunActionCallback(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }

    void MovementActionCallback(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();

        ////Player X ve Z axis'inde hareket ediyor.
        //currentMovement.x = currentMovementInput.x;
        //currentMovement.z = currentMovementInput.y;//Unity'de Y vektörü vertical position olduðu için Z axis'inde saklýyoruz.
        //currentRunMovement.x = currentMovementInput.x * runSpeed;
        //currentRunMovement.z = currentMovementInput.y * runSpeed;//Unity'de Y vektörü vertical position olduðu için Z axis'inde saklýyoruz.
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;//Sýfýr dýþýnda bir value geldiyse hareket etme tuþuna basýlmýþtýr.
    }

    void HandleRotation()
    {
        Vector3 positionToLookAt;
        //Player hareket ettiði yöne doðru bakmalý
        positionToLookAt.x = currentMovementInput.x;
        positionToLookAt.y = 0;
        positionToLookAt.z = currentMovementInput.y;
        

        Quaternion currentRotation = transform.rotation;

        if (isMovementPressed)//Hareket etmiyorken dönmüyoruz.
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            //Snappy dönüþ hareketleri olmamasý için Lerp kullandýk.
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

    }

    void Update()
    {
        HandleRotation();
        currentState.UpdateStates();
        characterController.Move(appliedMovement * Time.deltaTime);
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
