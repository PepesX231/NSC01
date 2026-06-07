using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyNormalAttack1 : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    private bool isPlayerFound = false;
    private bool isAttacking = false;
    private bool isDamageActive = false; 

    [Header("Detection Settings")]
    public float detectionRadius = 15f; 
    public float attackRange = 4f; 

    [Header("Attack Objects")]
    public GameObject dangerZoneModel; 
    public GameObject damageColliderObject; 

    [Header("Timing Settings")]
    public float delayBeforeAttack = 1f;
    public float attackDuration = 0.3f; 

    [Header("Special Distance Kill")]
    public float killRadius = 2.5f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        CubeMovement playerMovement = GameObject.FindAnyObjectByType<CubeMovement>();
        if (playerMovement != null)
        {
            player = playerMovement.transform;
        }
        
        if (dangerZoneModel != null) dangerZoneModel.SetActive(false);
        if (damageColliderObject != null) damageColliderObject.SetActive(false);
    }

    void Update()
    {
        if (player == null) return; 

        // 🚨 [กันบั๊กชนปลิว] ถ้า Agent โดนสับสวิตช์ปิดใช้งานเพราะผู้เล่นพุ่งชน ให้ข้ามเฟรมนี้ไปเลย
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        if (isDamageActive && damageColliderObject != null)
        {
            float distanceToPlayer = Vector3.Distance(damageColliderObject.transform.position, player.position);
            if (distanceToPlayer <= killRadius)
            {
                KillPlayer();
            }
        }

        if (isAttacking) return; 

        float distance = Vector3.Distance(transform.position, player.position);

        if (!isPlayerFound && distance <= detectionRadius)
        {
            isPlayerFound = true;
            Debug.Log(gameObject.name + " (Normal 1) Found Player! Chasing...");
        }

        if (isPlayerFound)
        {
            agent.SetDestination(player.position);

            if (distance <= attackRange)
            {
                StartCoroutine(DelayedAttackCoroutine());
            }
        }
    }

    // ลอจิกขบวนการทุบของตัวพลีชีพ: พื้นแดงโผล่ -> รอ 1 วิ -> ทุบตูม (0.3 วิ) -> ตัวเองระเบิดหายไปทันที!
    IEnumerator DelayedAttackCoroutine()
    {
        isAttacking = true;
        if (agent != null && agent.enabled && agent.isOnNavMesh) agent.isStopped = true; 

        if (dangerZoneModel != null) dangerZoneModel.SetActive(true);

        yield return new WaitForSeconds(delayBeforeAttack);

        Debug.Log("BOOM! Normal 1 Real Attack Active!");
        if (damageColliderObject != null) damageColliderObject.SetActive(true);
        isDamageActive = true; 

        yield return new WaitForSeconds(attackDuration);

        isDamageActive = false;
        if (dangerZoneModel != null) dangerZoneModel.SetActive(false);
        if (damageColliderObject != null) damageColliderObject.SetActive(false);

        // 🚨 [แก้ไขให้ตรงตัว] โจมตีเสร็จระเบิดตัวเองทำลาย Object ทิ้งทันที ไม่ต้องยืนรอเหนื่อย!
        Debug.Log($"💀 {gameObject.name} Has Exploded/Died after attack!");
        Destroy(gameObject); 
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