using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class CubeMovement : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float walkSpeed = 5f;       
    public float runSpeed = 9f;        
    
    [Header("Jumping & Gravity")]
    public float jumpHeight = 2f;      
    public float gravity = 9.81f;      

    [Header("Rotation Settings")]
    public float rotationSpeed = 10f;  

    [Header("Jetpack Settings")]
    public bool hasJetpack = false;       
    public float jetpackForce = 12f;      
    public GameObject jetpackBackpack;    
    public GameObject jetpackEffectObject;
    
    [Header("⚡ Jetpack Fuel (จำกัด 3 วิ)")]
    public float maxJetpackDuration = 3f;
    public float currentJetpackFuel = 3f;
    [Tooltip("ความเร็วในการฟื้นฟูเจ็ตแพ็คตอนอยู่บนพื้น (หน่วยต่อวิ)")]
    public float jetpackRegenRate = 1.5f;

    [Header("Pet Settings")]
    public GameObject lightPetObject;     

    [Header("Mirror Settings")]
    public bool hasMirror = false;         
    public GameObject mirrorFollow;        
    public GameObject handMirror;          
    public GameObject mirrorWorldObjects;  
    
    [HideInInspector] 
    public bool isMirrorUsing = false;    

    [Header("🚨 Dash Settings")]
    public float dashSpeed = 25f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 0.6f;
    public float dashSpinSpeed = 1440f; 
    public float pushForce = 20f;

    [Header("🏃 Run Stamina (จำกัด 3 วิ)")]
    public float maxRunDuration = 3f;
    public float currentRunStamina = 3f;
    [Tooltip("ความเร็วในการฟื้นฟูสเตมินาวิ่งตอนปล่อยปุ่ม (หน่วยต่อวิ)")]
    public float runRegenRate = 1.5f;
    private bool isExhausted = false; 

    [Header("💀 Fall Damage Settings")]
    public float voidFallLevel = -10f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private Vector2 inputMove;
    private bool isShiftPressed; 
    private bool isSpacePressed; 
    private bool isSpaceHeld;    

    // 🔥 [ตัวแปรใหม่สำหรับแก้บั๊กไฟพ่นไว]
    private float spacePressedTime = 0f; 
    private float holdThreshold = 0.15f; // ต้องกดปุ่มค้างไว้นานเกิน 0.15 วิ ถึงจะนับว่าตั้งใจบินเจ็ตแพ็ค

    private bool isDashing = false;
    private bool canDash = true;
    private Vector3 dashDirection;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        currentRunStamina = maxRunDuration;
        currentJetpackFuel = maxJetpackDuration;

        if (jetpackBackpack != null) jetpackBackpack.SetActive(false);
        if (jetpackEffectObject != null) jetpackEffectObject.SetActive(false);
        if (lightPetObject != null) lightPetObject.SetActive(false);
        
        if (mirrorFollow != null) mirrorFollow.SetActive(false);
        if (handMirror != null) handMirror.SetActive(false);
        if (mirrorWorldObjects != null) mirrorWorldObjects.SetActive(false);
    }

    void Update()
    {
        // 💀 1. ระบบดักตกโลกตาย
        if (transform.position.y < voidFallLevel)
        {
            Die();
            return;
        }

        // 🔮 2. ระบบกระจกวิเศษ
        if (Keyboard.current != null && hasMirror && Keyboard.current.rKey.wasPressedThisFrame)
        {
            isMirrorUsing = !isMirrorUsing; 
            ToggleMirror();
        }

        // ⚡ 3. ระบบ Dash
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && canDash && !isDashing)
        {
            StartCoroutine(PerformDashCoroutine());
        }

        if (isDashing) return;

        // 🚶‍♂️ 4. ตรวจเช็กพื้นดิน
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
            spacePressedTime = 0f; // รีเซ็ตเวลากดค้างเมื่อเท้าแตะพื้น
        }

        // ดักจับสถานะปุ่มจากคีย์บอร์ด
        if (Keyboard.current != null)
        {
            isShiftPressed = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
            isSpaceHeld = Keyboard.current.spaceKey.isPressed; 
        }

        // จับเวลาสะสมการกดปุ่ม Spacebar ค้างไว้
        if (isSpaceHeld)
        {
            spacePressedTime += Time.deltaTime;
        }
        else
        {
            spacePressedTime = 0f; // ปล่อยปุ่มเมื่อไหร่ รีเซ็ตเวลาทิ้งทันที
        }

        // 🏃‍♂️ 5. ลอจิกคุม Stamina การวิ่ง
        bool isMoving = inputMove.magnitude > 0.1f;
        if (isShiftPressed && isMoving && !isExhausted && currentRunStamina > 0f)
        {
            currentRunStamina -= Time.deltaTime;
            if (currentRunStamina <= 0f)
            {
                currentRunStamina = 0f;
                isExhausted = true;
            }
        }
        else
        {
            if (currentRunStamina < maxRunDuration)
            {
                currentRunStamina += runRegenRate * Time.deltaTime;
                if (currentRunStamina >= maxRunDuration)
                {
                    currentRunStamina = maxRunDuration;
                    isExhausted = false;
                }
            }
        }

        float currentSpeed = (isShiftPressed && !isExhausted) ? runSpeed : walkSpeed;

        // คำนวณทิศทางตามมุมกล้อง
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        forward.y = 0f; right.y = 0f;
        forward.Normalize(); right.Normalize();

        Vector3 finalMoveDirection = (forward * inputMove.y + right * inputMove.x).normalized;
        controller.Move(finalMoveDirection * currentSpeed * Time.deltaTime);

        // --- ระบบกระโดดปกติบนพื้นดิน ---
        if (isSpacePressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * 2f * gravity);
            isGrounded = false; 
        }
        isSpacePressed = false; 

        // ==========================================================
        // 🚀 6. ลอจิกคุมพลังงานฟิสิกส์ Jetpack (ระบบตรวจสอบเวลากดค้างจริง)
        // ==========================================================
        // 🔥 [จุดแก้หัวใจสำคัญ] เจ็ตแพ็คจะพ่นไฟทำงานได้ก็ต่อเมื่อ:
        // ตัวอยู่กลางอากาศ + น้ำมันยังเหลือ + และผู้เล่นต้อง "กดปุ่ม Spacebar ค้างไว้จนเวลาเกินขีดจำกัด (0.15 วิ)" เท่านั้น
        if (hasJetpack && isSpaceHeld && !isGrounded && spacePressedTime >= holdThreshold && currentJetpackFuel > 0f)
        {
            velocity.y = jetpackForce; 
            currentJetpackFuel -= Time.deltaTime;

            if (currentJetpackFuel < 0f) currentJetpackFuel = 0f;

            // เปิดไฟเอฟเฟกต์ค้างนิ่ง ๆ ไม่มีกระพริบ
            if (jetpackEffectObject != null && !jetpackEffectObject.activeSelf) 
                jetpackEffectObject.SetActive(true);
        }
        else
        {
            // ปิดไฟทันทีเมื่อปล่อยนิ้ว, กดแค่จังหวะกระโดดสั้น ๆ, น้ำมันหมด หรือแตะพื้น
            if (jetpackEffectObject != null && jetpackEffectObject.activeSelf) 
                jetpackEffectObject.SetActive(false);
        }

        // คืนเชื้อเพลิงเมื่ออยู่บนพื้นดินปกติ
        if (isGrounded && currentJetpackFuel < maxJetpackDuration)
        {
            currentJetpackFuel += jetpackRegenRate * Time.deltaTime;
            if (currentJetpackFuel > maxJetpackDuration) currentJetpackFuel = maxJetpackDuration;
        }

        // คำนวณแรงโน้มถ่วง
        velocity.y -= gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (finalMoveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(finalMoveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    IEnumerator PerformDashCoroutine()
    {
        isDashing = true;
        canDash = false;
        velocity = Vector3.zero;

        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        forward.y = 0f; right.y = 0f;
        forward.Normalize(); right.Normalize();
        
        Vector3 inputDir = (forward * inputMove.y + right * inputMove.x).normalized;
        dashDirection = inputDir != Vector3.zero ? inputDir : transform.forward;

        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            controller.Move(dashDirection * dashSpeed * Time.deltaTime);
            transform.Rotate(Vector3.up, dashSpinSpeed * Time.deltaTime);
            yield return null; 
        }

        transform.rotation = Quaternion.LookRotation(dashDirection);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isDashing && hit.gameObject.CompareTag("KnockbackEnemy"))
        {
            float hitAngle = Vector3.Dot(dashDirection.normalized, hit.normal.normalized);

            if (hitAngle <= -0.5f) 
            {
                Rigidbody enemyRb = hit.gameObject.GetComponent<Rigidbody>();
                UnityEngine.AI.NavMeshAgent enemyAgent = hit.gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();

                if (enemyRb != null)
                {
                    if (enemyAgent != null) enemyAgent.enabled = false; 
                    enemyRb.isKinematic = false; 

                    Vector3 pushDirection = hit.gameObject.transform.position - transform.position;
                    pushDirection.y = 0.5f; 
                    pushDirection.Normalize();

                    enemyRb.AddForce(pushDirection * pushForce, ForceMode.VelocityChange);
                    Debug.Log($"🎯 หน้าทิ่มชนประสานงาเต็มเปา! ปลด Kinematic และผลัก {hit.gameObject.name} ปลิวตกรันเวย์!");
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "JetpackItem")
        {
            hasJetpack = true; 
            if (jetpackBackpack != null) jetpackBackpack.SetActive(true);
            Destroy(other.gameObject);
        }
        if (other.gameObject.name == "PetItem")
        {
            if (lightPetObject != null) lightPetObject.SetActive(true);
            Destroy(other.gameObject);
        }
        if (other.gameObject.name == "MirrorItem")
        {
            hasMirror = true; 
            if (mirrorFollow != null) mirrorFollow.SetActive(true);
            Destroy(other.gameObject);
        }
    }

    private void ToggleMirror()
    {
        if (isMirrorUsing)
        {
            if (mirrorFollow != null) mirrorFollow.SetActive(false); 
            if (handMirror != null) handMirror.SetActive(true);      
            if (mirrorWorldObjects != null) mirrorWorldObjects.SetActive(true); 
        }
        else
        {
            if (handMirror != null) handMirror.SetActive(false);     
            if (mirrorWorldObjects != null) mirrorWorldObjects.SetActive(false); 
            if (mirrorFollow != null) mirrorFollow.SetActive(true);  
        }
    }

    public void Die()
    {
        Debug.Log(">>> PLAYER DIED! <<<<");
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name); 
    }

    void OnMove(InputValue value) { inputMove = value.Get<Vector2>(); }
    void OnSprint(InputValue value) { }
    void OnJump(InputValue value) { if (value.isPressed) isSpacePressed = true; }

    public bool IsRunningNow()
    {
        bool isMoving = Mathf.Abs(inputMove.x) > 0.1f || Mathf.Abs(inputMove.y) > 0.1f;
        return isShiftPressed && isMoving && !isExhausted; 
    }
}