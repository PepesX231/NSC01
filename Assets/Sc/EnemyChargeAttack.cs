using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyChargeAttack : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    private bool isPlayerFound = false;
    private bool isAttacking = false;

    [Header("Detection Settings")]
    [Tooltip("ระยะที่ศัตรูมองเห็นผู้เล่น")]
    public float detectionRadius = 15f; 
    [Tooltip("ระยะที่ศัตรูจะเริ่มเตรียมทุบ")]
    public float attackRange = 4f; 

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

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Cube").transform; 
        
        // บังคับปิดวัตถุโจมตีทั้งหมดตอนเริ่มเกม
        if (dangerZoneModel != null) dangerZoneModel.SetActive(false);
        if (damageColliderObject != null) damageColliderObject.SetActive(false);
    }

    void Update()
    {
        if (player == null || isAttacking) return; 

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // --- ลอจิก 1: ตรวจจับเจอ Player ---
        if (!isPlayerFound && distanceToPlayer <= detectionRadius)
        {
            isPlayerFound = true;
            Debug.Log("Enemy Found Player! Chasing...");
        }

        if (isPlayerFound)
        {
            // --- ลอจิก 2: เดินไล่ตามผู้เล่น ---
            agent.SetDestination(player.position);

            // --- ลอจิก 3: ถึงระยะเริ่มขบวนการโจมตี ---
            if (distanceToPlayer <= attackRange)
            {
                StartCoroutine(DelayedAttackCoroutine());
            }
        }
    }

    // ==========================================================
    // ลอจิกขบวนการทุบ: พื้นแดงโผล่ -> รอ 1 วิ -> เปิดตัวชนจริง -> คูลดาวน์
    // ==========================================================
    IEnumerator DelayedAttackCoroutine()
    {
        isAttacking = true;
        agent.isStopped = true; // หยุดเดินทันทีเพื่อตั้งท่าทุบ
        Debug.Log("Danger Zone Active! Warning Player...");

        // 1. เปิดแผ่นสี่เหลี่ยมสีแดงโผล่มาเตือนบนพื้นทันที
        if (dangerZoneModel != null) dangerZoneModel.SetActive(true);

        // 2. หน่วงเวลาตามค่ากำหนด (เช่น 1 วินาที) จังหวะนี้ผู้เล่นต้องวิ่งหลบออกไป
        yield return new WaitForSeconds(delayBeforeAttack);

        // 3. ครบเวลาแล้ว! สั่งเปิดตัวชนกล่องล่องหนทำดาเมจจริงขึ้นมาทุบทันที
        Debug.Log("BOOM! Real Attack Active!");
        if (damageColliderObject != null) damageColliderObject.SetActive(true);

        // 4. เปิดพิกัดค้างไว้แป๊บนึงเพื่อให้ระบบสัญชาตญาณความตายทำงาน
        yield return new WaitForSeconds(attackDuration);

        // 5. ทุบเสร็จแล้ว สั่งปิดซ่อนทั้งแผ่นพื้นแดงและตัวชนจริงพร้อมกัน
        if (dangerZoneModel != null) dangerZoneModel.SetActive(false);
        if (damageColliderObject != null) damageColliderObject.SetActive(false);

        // 6. คูลดาวน์: ให้ศัตรูยืนนิ่งพักเหนื่อยแป๊บนึง ก่อนจะให้สิทธิ์วิ่งไล่กวดต่อ
        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
        if (agent != null) agent.isStopped = false; 
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}