using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
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
    int isJumpingHash;
    int jumpCountHash;


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
    float groundedGravity = -.05f;

    //jump variables
    bool isJumpPressed = false;
    float initialJumpVelocity; 
    bool isJumping = false;
    bool isJumpAnimating = false;
    int jumpCount = 0;
    Dictionary<int, float> initialJumpVelocities = new();
    Dictionary<int, float> jumpGravities = new();
    Coroutine jumpResetCoroutine = null;

    private void Awake()
    {
        //Get References
        playerInputAction = new PlayerInputAction();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        //AnimatorHash for Performance
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");
        jumpCountHash = Animator.StringToHash("jumpCount");

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
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
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
        jumpGravities.Add(0, gravity);
        jumpGravities.Add(1, gravity);
        jumpGravities.Add(2, secondJumpGravity);
        jumpGravities.Add(3, thirdJumpGravity);

    }

    void JumpActionCallback(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
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

    void HandleJump()
    {
        //Þuan zýplama anýnda deðilsek ve karakter yerdeyse ve Jump butonu basýlmýþsa, zýplýyoruz.
        if(!isJumping && characterController.isGrounded && isJumpPressed)
        {
            //Jump combosu tamamlanmamýþsa ResetTimer durduruyoruz ki jumpCount combonun ortasýnda sýfýrlanmasýn.
            if(jumpCount < 3 && jumpResetCoroutine != null)
            {
                StopCoroutine(jumpResetCoroutine);
            }
            //Dictionary dýþýna çýkmamak için ekledik
            else if (jumpCount >= 3)
            {
                jumpCount = 0;
                if(jumpResetCoroutine != null) StopCoroutine(jumpResetCoroutine);
            }
           
            

            //Animation baþlýyor.
            animator.SetBool(isJumpingHash, true);
            isJumpAnimating = true;

            isJumping = true;
            jumpCount += 1;
            animator.SetInteger(jumpCountHash, jumpCount);

            //HandleGravity'de Velocity Verlet(yarým adýmlýk) hesaplama yaptýðýmýz için, initialJumpVelocity'yi yarý yarýya düþürdük.
            currentMovement.y = initialJumpVelocities[jumpCount];
            appliedMovement.y = initialJumpVelocities[jumpCount];

        }
        //Þuan zýplama anýndaysak ve karakter yerdeyse ve Jump butonu basýlmamýþsa, zýplama henüz yeni bitmiþtir.
        else if(isJumping && characterController.isGrounded && !isJumpPressed)
        {
            isJumping = false;
        }
    }

    IEnumerator JumpResetTimer()
    {
        yield return new WaitForSeconds(0.5f);
        jumpCount = 0;
    }

    void HandleGravity()
    {
        //sadece havaya yükselirken y 0'dan büyük olur.
        //Jump butonuna basmayý býrakýrsak küçük jump(fallMultiplier 1f olduðu için), basýlý tutarsak ise full jump(fallMultiplier arttýðý için)
        bool isFalling = currentMovement.y <= 0f || !isJumpPressed;

        //Düþüyorsak, public fallspeed deðeri kullanýyoruz. Yoksa 1.0f default deðer kullanýyoruz. Düþme hýzýný artýrmak için kullanýlýr.
        float fallMultiplier = fallSpeed;

        //CharacterController yere deðiyorsa, küçük bir gravity veriyoruz ki floating durumunda kalmasýn.
        if (characterController.isGrounded)
        {
            //Animation duruyor.
            if (isJumpAnimating) 
            {
                animator.SetBool(isJumpingHash, false);
                isJumpAnimating = false;

                //Jump durduktan bir süre sonra jumpCount sýfýrlanýyor. Böylece jump combosu resetlenmiþ oluyor.
                jumpResetCoroutine = StartCoroutine(JumpResetTimer());

            } 

            currentMovement.y = appliedMovement.y = groundedGravity;
        } 
        //Player düþüþte. Else ile kod tekrarý olsa da ilerde buraya düþüþ anýna özel kodlar eklenebilir o nedenle bu þekilde býraktýk.
        else if (isFalling)
        {
            //Burada Velocity Verlet entegrasyonu kullandýk. Yarým adýmlýk hýz güncellemesi yaptýk böylece frame rate farkýndan doðacak sapmalardan kurtulmak amaçlanýyor.
            float previousYVelocity = currentMovement.y;
            currentMovement.y = currentMovement.y + (jumpGravities[jumpCount] * fallMultiplier * Time.deltaTime);

            //NextYVelocity Clamp'liyoruz, yüksek mesafeden aþaðý düþüyorsak fallMultiplier'ýmýz bizi aþýrý hýzlandýrmasýn diye. -20f düþerken ulaþacaðýmýz max düþme hýzý
            appliedMovement.y = Mathf.Max((previousYVelocity + currentMovement.y) * 0.5f, -20.0f);
        }
        //Diðer durumlarda Gravity uyguluyoruz.
        else
        {
            //Burada Velocity Verlet entegrasyonu kullandýk. Yarým adýmlýk hýz güncellemesi yaptýk böylece frame rate farkýndan doðacak sapmalardan kurtulmak amaçlanýyor.
            float previousYVelocity = currentMovement.y;
            currentMovement.y = currentMovement.y + (jumpGravities[jumpCount] * Time.deltaTime);
            appliedMovement.y = (previousYVelocity + currentMovement.y) * 0.5f;
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

    void HandleMovement()
    {
        if (isRunPressed)
        {
            appliedMovement.x = currentRunMovement.x;
            appliedMovement.z = currentRunMovement.z;
        }
        else
        {
            appliedMovement.x = currentMovement.x;
            appliedMovement.z = currentMovement.z;
        }

        characterController.Move(appliedMovement * Time.deltaTime);
    }

    void Update()
    {
        
        HandleRotation();
        HandleAnimation();
        HandleMovement();


        HandleGravity();//HandleMovement'tan sonra çalýþmalý, çünkü gravity deðeri isGrounded'a göre deðiþecek ve Move metodu HandleMovement'ýn içinde. Jump için yaptýk bunu.
        HandleJump();//Bunu HandleGravity'den sonra çalýþtýrýyoruz çünkü gravity deðerine baðlý olarak iþlemlerini yapýyor.
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
