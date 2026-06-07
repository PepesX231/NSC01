using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyDashAttack : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    private CubeMovement playerCube; 
    private bool isPlayerFound = false;
    private bool isAttacking = false;
    private bool isDamageActive = false; 

    private float originalSpeed;
    private float originalAcceleration;

    [Header("Detection & Flee Settings")]
    public float detectionRadius = 15f; 
    public float attackRange = 6f; 
    [Range(0f, 1f)]
    public float visionAccuracy = 0.75f;
    public float fleeDistance = 8f;

    [Header("Attack Objects")]
    public GameObject dangerZoneModel; 
    public GameObject damageColliderObject; 

    [Header("Timing & Dash Settings")]
    [Tooltip("ระยะเวลาที่มอนสเตอร์จะ 'หมุนตัวเล็งตามผู้เล่น' ตอนขึ้นพื้นแดง (วินาที)")]
    public float delayBeforeAttack = 0.7f;

    // 🔥 [เพิ่มตัวแปรใหม่] เปิดช่องว่างให้ผู้เล่นกดหลบ
    [Tooltip("ระยะเวลาที่มอนสเตอร์จะ 'หยุดหมุนตัวแล้วยืนนิ่งล็อกมุมค้างไว้' ก่อนจะพุ่งจริง (วินาที) เพื่อให้ผู้เล่นวิ่งหลบออกจากวงแดงทัน")]
    public float lockDirectionDuration = 0.4f;
    
    [Tooltip("ความเร็วในการพุ่งชน ยิ่งเยอะยิ่งไวระดับจรวด (แนะนำ 35 - 50)")]
    public float dashSpeed = 35f;
    
    [Tooltip("ระยะเวลาที่ใช้ในการพุ่งทลวง (วินาที) แนะนำ 0.25 - 0.35 วิ")]
    public float attackDuration = 0.3f; 
    
    [Tooltip("ระยะเวลาคูลดาวน์หลังพุ่งเสร็จ ยืนมึนหัวแป๊บนึง")]
    public float attackCooldown = 1.5f; 

    [Header("Special Distance Kill")]
    public float killRadius = 2.5f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        playerCube = GameObject.FindAnyObjectByType<CubeMovement>();
        if (playerCube != null)
        {
            player = playerCube.transform;
        }
        
        if (dangerZoneModel != null) dangerZoneModel.SetActive(false);
        if (damageColliderObject != null) damageColliderObject.SetActive(false);

        if (agent != null)
        {
            originalSpeed = agent.speed;
            originalAcceleration = agent.acceleration;
        }
    }

    void Update()
    {
        if (player == null || playerCube == null) return; 
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        if (isDamageActive && damageColliderObject != null)
        {
            float distanceToPlayer = Vector3.Distance(damageColliderObject.transform.position, player.position);
            if (distanceToPlayer <= killRadius)
            {
                KillPlayer();
            }
        }

        // 🔮 ระบบหนีกระจกวิเศษ
        if (playerCube.hasMirror && playerCube.isMirrorUsing)
        {
            Vector3 directionToMe = (transform.position - player.position).normalized;
            directionToMe.y = 0f;

            Vector3 playerForward = player.transform.forward;
            playerForward.y = 0f;

            float lookMatch = Vector3.Dot(playerForward.normalized, directionToMe.normalized);
            float currentDistance = Vector3.Distance(player.position, transform.position);

            if (lookMatch >= visionAccuracy && currentDistance <= detectionRadius)
            {
                if (isAttacking)
                {
                    StopAllCoroutines();
                    agent.speed = originalSpeed;
                    agent.acceleration = originalAcceleration;
                    agent.updateRotation = true; 
                    isAttacking = false;
                    isDamageActive = false;
                    if (dangerZoneModel != null) dangerZoneModel.SetActive(false);
                    if (damageColliderObject != null) damageColliderObject.SetActive(false);
                }

                FleeFromPlayer(directionToMe);
                return; 
            }
        }

        if (isAttacking) return; 

        float distance = Vector3.Distance(transform.position, player.position);

        if (!isPlayerFound && distance <= detectionRadius)
        {
            isPlayerFound = true;
            Debug.Log(gameObject.name + " (Dash) Spotted Player!");
        }

        if (isPlayerFound)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);

            if (distance <= attackRange)
            {
                StartCoroutine(DashAttackCoroutine());
            }
        }
    }

    private void FleeFromPlayer(Vector3 directionFromPlayer)
    {
        Vector3 newFleePosition = transform.position + directionFromPlayer.normalized * fleeDistance;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(newFleePosition, out hit, 2.0f, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
    }

    IEnumerator DashAttackCoroutine()
    {
        isAttacking = true;
        
        agent.isStopped = true; 
        if (dangerZoneModel != null) dangerZoneModel.SetActive(true);

        // 🎯 1. จังหวะชาร์จเฟสแรก: มอนสเตอร์จะ "หมุนเล็งตามผู้เล่น" แบบเกาะติด
        float chargeTime = 0f;
        while (chargeTime < delayBeforeAttack)
        {
            Vector3 targetDir = (player.position - transform.position);
            targetDir.y = 0f; 

            if (targetDir != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(targetDir.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 15f);
            }

            chargeTime += Time.deltaTime;
            yield return null; 
        }

        // 🔥 2. จังหวะชาร์จเฟสสอง (ล็อกเป้า): มอนสเตอร์จะ "หยุดหมุนนิ่งทื่อ" เป็นเวลาสั้น ๆ เปิดโอกาสให้ผู้เล่นวิ่งหลบออกจากวงแดง!
        // (ในเฟสนี้ แถบแดงยังอยู่นะครับ แต่ตัวมอนสเตอร์จะไม่หันหัวตามผู้เล่นแล้ว ค้างเติ่งไปเลย)
        yield return new WaitForSeconds(lockDirectionDuration);

        // 🚨 สั่งปิดพื้นสีแดงทันทีเมื่อหมดเวลารอล็อกเป้า! 
        if (dangerZoneModel != null) dangerZoneModel.SetActive(false); 

        // 3. คำนวณทิศทางสุดท้ายที่มันล็อกค้างไว้จากแนวหน้ากล่องชนจริง
        Vector3 dashDirection = transform.forward; 
        if (damageColliderObject != null)
        {
            dashDirection = damageColliderObject.transform.forward;
        }
        dashDirection.y = 0f; 
        dashDirection = dashDirection.normalized;

        if (agent != null)
        {
            agent.updateRotation = false; 
        }

        if (dashDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dashDirection);
        }

        // 4. สับสปีดพุ่งชนเต็มสูบไปยังทิศที่ล็อกไว้
        agent.isStopped = false; 
        agent.speed = dashSpeed; 
        agent.acceleration = 9999f; 
        
        Vector3 dashDestination = transform.position + (dashDirection * (dashSpeed * attackDuration));
        agent.SetDestination(dashDestination);

        if (damageColliderObject != null) damageColliderObject.SetActive(true);
        isDamageActive = true;

        yield return new WaitForSeconds(attackDuration);

        // 5. พุ่งชนเสร็จเรียบร้อย ปิดตัวชนจริง
        isDamageActive = false;
        if (damageColliderObject != null) damageColliderObject.SetActive(false);

        // 6. คืนค่าความเร็วปกติ
        agent.isStopped = true;
        agent.speed = originalSpeed;
        agent.acceleration = originalAcceleration;
        if (agent != null)
        {
            agent.updateRotation = true; 
        }

        // 7. คูลดาวน์มึนหัวชั่วขณะก่อนเริ่มไล่ใหม่
        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
        agent.isStopped = false; 
    }

    private void KillPlayer()
    {
        Debug.Log("💥 " + gameObject.name + " พุ่งชนโดนเต็ม ๆ ตาย!");
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}