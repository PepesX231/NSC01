using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyNormalAttack : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    private CubeMovement playerCube; // ลิงก์ดึงข้อมูลกระจกโดยตรง
    private bool isPlayerFound = false;
    private bool isAttacking = false;
    private bool isDamageActive = false; // เช็กว่าช่วงเฟรมที่ทุบ (0.3 วิ) ดาเมจทำงานอยู่ไหม

    [Header("Detection & Flee Settings")]
    [Tooltip("ระยะที่ศัตรูมองเห็นผู้เล่น")]
    public float detectionRadius = 15f; 
    [Tooltip("ระยะที่ศัตรูจะเริ่มเตรียมทุบ")]
    public float attackRange = 4f; 
    [Range(0f, 1f)]
    [Tooltip("ค่าความตรงสายตาตอนส่องกระจก (ยิ่งใกล้ 1 ยิ่งต้องจ้องตรงๆ ถึงจะหนี)")]
    public float visionAccuracy = 0.75f;
    [Tooltip("ระยะทางที่ศัตรูจะวิ่งเตลิดหนีออกไปตอนโดนส่องกระจก")]
    public float fleeDistance = 8f;

    [Header("Attack Objects")]
    [Tooltip("ลาก Object พื้นหลังสีแดงใบ้เตือนบนพื้นมาใส่ช่องนี้ (DangerZone)")]
    public GameObject dangerZoneModel; 
    [Tooltip("ลาก Object ตัวชนจริง 'DamageCollider_Empty' มาใส่ช่องนี้")]
    public GameObject damageColliderObject; 

    [Header("Timing Settings")]
    [Tooltip("ระยะเวลาเตือน (วินาที) หลังจากพื้นแดงโผล่ ก่อนจะทุบจริง")]
    public float delayBeforeAttack = 1f;
    [Tooltip("ระยะเวลาที่เปิดพิกัดตัวชนจริงค้างไว้ (วินาที)")]
    public float attackDuration = 0.3f; 
    [Tooltip("ระยะเวลาคูลดาวน์ (วินาที) หยุดยืนนิ่งหลังทุบเสร็จ ก่อนจะเริ่มเดินไล่ต่อ")]
    public float attackCooldown = 1.2f; 

    [Header("Special Distance Kill")]
    [Tooltip("รัศมีความกว้างในการฆ่า (เมตร) ถ้าผู้เล่นอยู่ในระยะนี้ตอนทุบจะตายทันที")]
    public float killRadius = 2.5f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        playerCube = GameObject.FindAnyObjectByType<CubeMovement>();
        if (playerCube != null)
        {
            player = playerCube.transform;
        }
        
        // ตั้งค่าเริ่มต้นของ Object แจ้งเตือนและดาเมจ
        if (dangerZoneModel != null) dangerZoneModel.SetActive(false);
        if (damageColliderObject != null) damageColliderObject.SetActive(false);
    }

    void Update()
    {
        if (player == null || playerCube == null) return; 

        // 🚨 [กันบั๊กชนปลิว] ถ้า Agent ไม่พร้อมใช้งาน ให้ข้ามเฟรมนี้ไปเลย
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        // 1. ลอจิกทำดาเมจฆ่าผู้เล่น (เช็กทุกเฟรมถ้าเปิดดาเมจอยู่)
        if (isDamageActive && damageColliderObject != null)
        {
            float distanceToPlayer = Vector3.Distance(damageColliderObject.transform.position, player.position);
            if (distanceToPlayer <= killRadius)
            {
                KillPlayer();
            }
        }

        // 2. 🔮 ระบบหนีกระจกวิเศษ: ตรวจเช็กว่าผู้เล่นยกกระจกส่องหน้ามันอยู่หรือไม่?
        if (playerCube.hasMirror && playerCube.isMirrorUsing)
        {
            // หาเวกเตอร์ทิศทางจากตัว Player มาหาตัวศัตรู
            Vector3 directionToMe = (transform.position - player.position).normalized;
            directionToMe.y = 0f;

            Vector3 playerForward = player.transform.forward;
            playerForward.y = 0f;

            // ตรวจสอบความตรงของมุมสายตา (Dot Product)
            float lookMatch = Vector3.Dot(playerForward.normalized, directionToMe.normalized);
            float currentDistance = Vector3.Distance(player.position, transform.position);

            // ถ้าจ้องมองตรงๆ และอยู่ในระยะสายตาตรวจจับ -> ตัดลอจิกอื่นทิ้งแล้ววิ่งหนีทันที!
            if (lookMatch >= visionAccuracy && currentDistance <= detectionRadius)
            {
                // ถ้ามันกำลังชาร์จโจมตีค้างอยู่ ให้รีเซ็ตหยุดชาร์จทันที (โดนกระจกขัดจังหวะ)
                if (isAttacking)
                {
                    StopAllCoroutines();
                    isAttacking = false;
                    isDamageActive = false;
                    if (dangerZoneModel != null) dangerZoneModel.SetActive(false);
                    if (damageColliderObject != null) dangerZoneModel.SetActive(false);
                }

                FleeFromPlayer(directionToMe);
                return; // 🔥 ออกจากฟังก์ชัน Update ทันที เพื่อไม่ให้คำสั่งเดินไล่ล่าด้านล่างมาทำงานทับซ้อน
            }
        }

        // 3. ลอจิกปกติ: เดินไล่ล่าและเข้าโจมตีเมื่อถึงระยะ
        if (isAttacking) return; 

        float distance = Vector3.Distance(transform.position, player.position);

        if (!isPlayerFound && distance <= detectionRadius)
        {
            isPlayerFound = true;
            Debug.Log(gameObject.name + " (Normal) Found Player! Chasing...");
        }

        if (isPlayerFound)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);

            if (distance <= attackRange)
            {
                StartCoroutine(DelayedAttackCoroutine());
            }
        }
    }

    private void FleeFromPlayer(Vector3 directionFromPlayer)
    {
        // คำนวณจุดปลายทางใหม่ในทิศตรงกันข้าม
        Vector3 newFleePosition = transform.position + directionFromPlayer.normalized * fleeDistance;

        // หาจุดบนแผ่นสีฟ้า NavMesh ที่ปลอดภัยไม่เดินทะลุกำแพง
        NavMeshHit hit;
        if (NavMesh.SamplePosition(newFleePosition, out hit, 2.0f, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
            Debug.Log($"😱 {gameObject.name} กรี๊ด! โดนกระจกส่องหน้า! กำลังวิ่งหนีสุดชีวิต!");
        }
    }

    IEnumerator DelayedAttackCoroutine()
    {
        isAttacking = true;
        if (agent != null && agent.enabled && agent.isOnNavMesh) agent.isStopped = true; 
        
        Debug.Log(gameObject.name + " Danger Zone Active! Warning Player...");

        if (dangerZoneModel != null) dangerZoneModel.SetActive(true);

        yield return new WaitForSeconds(delayBeforeAttack);

        Debug.Log("BOOM! Normal Real Attack Active!");
        if (damageColliderObject != null) damageColliderObject.SetActive(true);
        isDamageActive = true; 

        yield return new WaitForSeconds(attackDuration);

        isDamageActive = false;
        if (dangerZoneModel != null) dangerZoneModel.SetActive(false);
        if (damageColliderObject != null) damageColliderObject.SetActive(false);

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
        if (agent != null && agent.enabled && agent.isOnNavMesh) agent.isStopped = false; 
    }

    private void KillPlayer()
    {
        Debug.Log("💥 " + gameObject.name + " killed the player via Normal Distance Check!");
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