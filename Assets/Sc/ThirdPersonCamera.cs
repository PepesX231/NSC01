using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;           
    
    [Header("Distance Settings")]
    public float distance = 5.0f;      
    public float height = 2.0f;        
    
    [Header("Speed Settings")]
    public float xSpeed = 6.0f;        
    public float ySpeed = 6.0f;        
    
    [Header("Clamping Roll")]
    public float yMinLimit = -20f;     
    public float yMaxLimit = 80f;      

    // ==========================================
    // เพิ่มส่วนนี้: ตั้งค่าสำหรับเอฟフェกต์จอยืด (FOV Effect)
    // ==========================================
    [Header("Fov Effect (Sprint)")]
    public float normalFov = 60f;       // FOV ตอนเดินปกติ
    public float sprintFov = 75f;       // FOV ตอนกดวิ่ง (ยิ่งเยอะ จอยิ่งยืดกว้าง)
    public float fovSpeed = 8f;         // ความเร็วในการยืด/หดของจอ

    private float x = 0.0f;
    private float y = 0.0f;
    private Camera cam;
    private CubeMovement playerMovement; // ตัวอ้างอิงสคริปต์เดินของ Cube

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // ดึงคอมโพเนนต์กล้องมาใช้คิดค่า FOV
        cam = GetComponent<Camera>();
        if (cam != null) cam.fieldOfView = normalFov;

        // ค้นหาสคริปต์เดินที่อยู่บนตัว Cube อัตโนมัติ
        if (target != null)
        {
            playerMovement = target.GetComponent<CubeMovement>();
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            float mouseX = 0f;
            float mouseY = 0f;

#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                mouseX = Mouse.current.delta.x.ReadValue() * xSpeed * 0.1f;
                mouseY = Mouse.current.delta.y.ReadValue() * ySpeed * 0.1f;
            }
#else
            mouseX = Input.GetAxis("Mouse X") * xSpeed;
            mouseY = Input.GetAxis("Mouse Y") * ySpeed;
#endif

            x += mouseX;
            y -= mouseY;

            y = Mathf.Clamp(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);

            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + (target.position + Vector3.up * height);

            transform.rotation = rotation;
            transform.position = position;

            // ==========================================
            // ทำงานระบบจอยืด (Smooth FOV Lerp)
            // ==========================================
            if (cam != null && playerMovement != null)
            {
                // เช็กว่าผู้เล่นกำลังกดวิ่ง และ ตัวละครมีการขยับเขยื้อนจริงๆ ไหม
                bool isRunning = playerMovement.IsRunningNow();
                
                // เลือกเป้าหมายองศากล้อง: ถ้ารันอยู่ให้ใช้ sprintFov ถ้าไม่ให้ใช้ normalFov
                float targetFov = isRunning ? sprintFov : normalFov;
                
                // ค่อยๆ ปรับค่าความกว้างกล้องให้สมูท ไม่กระตุกตา
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, fovSpeed * Time.deltaTime);
            }
        }
    }
}