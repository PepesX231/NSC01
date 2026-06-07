using UnityEngine;

public class ObjectSightCheck : MonoBehaviour
{
    private CubeMovement playerCube; 
    private MeshRenderer meshRenderer; 
    private Collider objCollider;     

    [Header("Sight Settings")]
    [Range(0f, 1f)]
    [Tooltip("ค่าความตรงสายตา: ยิ่งใกล้ 1 ยิ่งต้องหันหน้าจ้องตรงๆ วัตถุถึงจะโผล่ (แนะนำตั้ง 0.7 - 0.8)")]
    public float visionAccuracy = 0.75f;

    [Header("📏 Distance Settings")]
    [Tooltip("เปิด/ปิด ระบบเช็กระยะห่าง (ถ้าติ๊กถูกไว้ ต้องเข้าใกล้ตามระยะวัตถุถึงจะโผล่)")]
    public bool useDistanceCheck = true;

    [Tooltip("ระยะห่างสูงสุด (เมตร) ที่ผู้เล่นต้องอยู่ภายในขอบเขตนี้ วัตถุถึงจะยอมโผล่มา")]
    public float maxDistance = 8f;

    // ตัวแปรสำหรับระบุกรณีพิเศษ (สัมผัสค้าง)
    private bool isPlayerTouching = false; 

    void Start()
    {
        playerCube = Object.FindAnyObjectByType<CubeMovement>();
        meshRenderer = GetComponent<MeshRenderer>();
        objCollider = GetComponent<Collider>();
    }

    void Update()
    {
        if (playerCube == null || !playerCube.hasMirror)
        {
            SetVisibility(false);
            return;
        }

        if (playerCube.isMirrorUsing)
        {
            // 1. หาเวกเตอร์ทิศทางจาก Cube มาหาวัตถุ
            Vector3 directionToMe = (transform.position - playerCube.transform.position).normalized;
            directionToMe.y = 0f; 

            // 2. ดึงทิศทางการหันหน้าของ Cube
            Vector3 playerForward = playerCube.transform.forward;
            playerForward.y = 0f;

            // 3. คำนวณหามุมมองสายตาด้วย Dot Product
            float lookMatch = Vector3.Dot(playerForward.normalized, directionToMe.normalized);

            // 📏 4. [เพิ่มลอจิก] คำนวณระบบตรวจสอบระยะห่าง (Distance Check)
            bool isWithinRange = true; // ตั้งต้นเป็น true ไว้ก่อน
            if (useDistanceCheck)
            {
                // วัดระยะห่างระหว่างตัว Player กับ วัตถุนี้จริง ๆ 
                float currentDistance = Vector3.Distance(playerCube.transform.position, transform.position);
                
                // ถ้าระยะปัจจุบันห่างเกินกว่า maxDistance แปลว่าอยู่ไกลเกินไป
                if (currentDistance > maxDistance)
                {
                    isWithinRange = false;
                }
            }

            // 🚨 5. รวมร่างเงื่อนไข:
            // (หันหน้าจ้องตรงเกณฑ์ AND ตัวอยู่ภายในระยะที่กำหนด) OR (ตัวกำลังยืนชนอยู่)
            if ((lookMatch >= visionAccuracy && isWithinRange) || isPlayerTouching)
            {
                SetVisibility(true);
            }
            else
            {
                // ถ้าไม่ตรงเงื่อนไขข้างบน ให้ล่องหนหายไปตามลอจิกเดิม
                SetVisibility(false);
            }
        }
        else
        {
            SetVisibility(false);
        }
    }

    void SetVisibility(bool isVisible)
    {
        if (meshRenderer != null && meshRenderer.enabled != isVisible)
        {
            meshRenderer.enabled = isVisible;
        }
        
        // ข้อควรระวัง: เราจะไม่ปิดกล่องชนหลัก (objCollider) ตอนล่องหน 
        // เพื่อให้ระบบ Trigger สามารถเช็กระยะเวลาเดินชนสัมผัสได้ตลอดเวลาครับพี่
    }

    // ==========================================================
    // ฟังก์ชันดักจับสัมผัส (Trigger) จากโค้ดเดิมของพี่
    // ==========================================================
    
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CubeMovement>() != null)
        {
            isPlayerTouching = true;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<CubeMovement>() != null)
        {
            isPlayerTouching = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CubeMovement>() != null)
        {
            isPlayerTouching = false;
        }
    }

    // วาดวงกลม Gizmos สีเขียวแสดงระยะ maxDistance ในหน้า Scene มองเห็นง่ายเวลาจัดด่านครับ
    void OnDrawGizmosSelected()
    {
        if (useDistanceCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, maxDistance);
        }
    }
}