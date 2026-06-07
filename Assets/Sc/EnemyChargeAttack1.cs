using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyChargeAttack1 : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    private bool isPlayerFound = false;
    private bool isAttacking = false;
    private bool isMovingWithDamage = false; // สถานะเช็กว่ากำลังเดินปล่อยดาเมจอยู่ไหม

    [Header("Detection Settings")]
    [Tooltip("ระยะที่ศัตรูมองเห็นผู้เล่น")]
    public float detectionRadius = 15f; 
    [Tooltip("ระยะที่ศัตรูจะเริ่มเปิดโหมดเตือนการโจมตี")]
    public float attackRange = 4f; 

    [Header("Attack Objects")]
    [Tooltip("ลาก Object พื้นหลังสีแดงใบ้เตือนบนพื้นมาใส่ช่องนี้ (DangerZone)")]
    public GameObject dangerZoneModel; 

    [Tooltip("ลาก Object ตัวชนจริง 'DamageCollider_Empty' มาใส่ช่องนี้")]
    public GameObject damageColliderObject; 

    [Header("Timing Settings")]
    [Tooltip("ระยะเวลาเตือน (วินาที) หลังจากพื้นแดงโผล่ ก่อนจะทำดาเมจจริง")]
    public float delayBeforeAttack = 1f;

    [Tooltip("ระยะเวลาที่ศัตรูจะปล่อยดาเมจค้างไว้พร้อมวิ่งไล่ตามผู้เล่น (วินาที)")]
    public float attackDuration = 5f; 
    
    [Tooltip("ระยะเวลาคูลดาวน์ (วินาที) หยุดยืนนิ่งหลังโจมตีเสร็จครบ 5 วิ Before Next Turn")]
    public float attackCooldown = 2f; 

    [Header("Special Distance Kill (ศัตรูหลายตัว)")]
    [Tooltip("รัศมีความกว้างในการฆ่า (เมตร) วัดจากจุดศูนย์กลางแผ่นแดง ถ้าผู้เล่นหลุดเข้ามาในระยะนี้ตอนปล่อยดาเมจจะตายทันที")]
    public float killRadius = 2.8f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // ค้นหาผู้เล่นด้วยสคริปต์เดิน (หมดปัญหาเรื่องชื่อ Cube ซ้ำหรือมีตัวเลขต่อท้าย)
        CubeMovement playerMovement = GameObject.FindAnyObjectByType<CubeMovement>();
        if (playerMovement != null)
        {
            player = playerMovement.transform;
        }
        
        // บังคับปิดวัตถุโจมตีทั้งหมดตอนเริ่มเกม
        if (dangerZoneModel != null) dangerZoneModel.SetActive(false);
        if (damageColliderObject != null) damageColliderObject.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        // ==========================================================
        // 🚨 ไม้เด็ดสำหรับศัตรูหลายตัว: ตัวไหนปล่อยดาเมจค้างอยู่ ตัวนั้นจะวัดระยะฆ่าผู้เล่นเอง
        // ==========================================================
        if (isMovingWithDamage)
        {
            // บังคับเดินไล่กวดจี้ตูดผู้เล่น
            agent.SetDestination(player.position);

            // คำนวณระยะห่างระหว่าง "แผ่นดาเมจของฉัน" กับ "ตัวผู้เล่น"
            if (damageColliderObject != null)
            {
                float distanceToPlayer = Vector3.Distance(damageColliderObject.transform.position, player.position);
                
                // ถ้าผู้เล่นเดินเข้ามาเหยียบในรัศมีของแผ่นแดงตัวนี้... สั่งฆ่าทันที!
                if (distanceToPlayer <= killRadius)
                {
                    KillPlayer();
                }
            }
            return; // ข้ามลอจิก Update ด้านล่างไปก่อนเพราะอยู่ในสถานะโจมตี
        }

        if (isAttacking) return; 

        float distance = Vector3.Distance(transform.position, player.position);

        // --- ลอจิก 1: ตรวจจับเจอ Player ---
        if (!isPlayerFound && distance <= detectionRadius)
        {
            isPlayerFound = true;
            Debug.Log(gameObject.name + " Found Player! Chasing...");
        }

        if (isPlayerFound)
        {
            // --- ลอจิก 2: เดินไล่ตามผู้เล่นปกติ ---
            agent.SetDestination(player.position);

            // --- ลอจิก 3: เข้าระยะทุบ ปล่อยคอร์รูกทีนโจมตีค้าง 5 วิ ---
            if (distance <= attackRange)
            {
                StartCoroutine(MovingAoEAttackCoroutine());
            }
        }
    }

    IEnumerator MovingAoEAttackCoroutine()
    {
        isAttacking = true;
        agent.isStopped = true; // 1. หยุดยืนนิ่ง ๆ ขู่ก่อน 1 วิ
        
        if (dangerZoneModel != null) dangerZoneModel.SetActive(true);

        // รอหน่วงเวลาเตือน (ช่วงนี้ยังเดินเหยียบได้ไม่ตาย)
        yield return new WaitForSeconds(delayBeforeAttack);

        // 2. เริ่มต้นช่วงเวลานรก 5 วินาที!
        if (damageColliderObject != null) damageColliderObject.SetActive(true); 
        
        agent.isStopped = false;      
        isMovingWithDamage = true;    // เปิดสวิตช์ให้ระบบวัดระยะฆ่าใน Update เริ่มทำงาน

        // ปล่อยค้างไว้ 5 วินาทีให้วิ่งไล่กวดพ่วงสถานะดาเมจไปด้วย
        yield return new WaitForSeconds(attackDuration);

        // 3. หมดเวลา 5 วินาที ปิดระบบทำลายล้างของตัวเองลง
        isMovingWithDamage = false; 
        if (dangerZoneModel != null) dangerZoneModel.SetActive(false);
        if (damageColliderObject != null) damageColliderObject.SetActive(false);

        // 4. ยืนพักเหนื่อยคูลดาวน์นิ่ง ๆ 
        agent.isStopped = true; 
        yield return new WaitForSeconds(attackCooldown);

        // 5. ปลดล็อกพร้อมกลับไปไล่ล่ารอบใหม่
        isAttacking = false;
        if (agent != null) agent.isStopped = false; 
    }

    // ฟังก์ชันสั่งฆ่าผู้เล่นและรีโหลดด่านใหม่
    private void KillPlayer()
    {
        Debug.Log("💥 " + gameObject.name + " killed the player via Distance Check!");
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}