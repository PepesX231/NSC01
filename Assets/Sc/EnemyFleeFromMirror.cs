using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyFleeFromMirror : MonoBehaviour
{
    private NavMeshAgent agent;
    private CubeMovement playerCube;

    [Header("Sight & Flee Settings")]
    [Range(0f, 1f)]
    [Tooltip("ค่าความตรงสายตา: ล้อมาจาก ObjectSightCheck (ยิ่งใกล้ 1 ยิ่งต้องหันหน้าจ้องตรง ๆ ศัตรูถึงจะยอมหนี)")]
    public float visionAccuracy = 0.75f;

    [Tooltip("ระยะห่างสูงสุด (เมตร) ที่ผู้เล่นส่องกระจกมาถึงแล้วศัตรูจะตกใจวิ่งหนี")]
    public float maxDistance = 10f;

    [Tooltip("ระยะทางที่ศัตรูจะคำนวณสับเกียร์หมาวิ่งเตลิดหนีออกไป")]
    public float fleeDistance = 8f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // ค้นหาผู้เล่น CubeMovement ในสนามอัตโนมัติ
        playerCube = Object.FindAnyObjectByType<CubeMovement>();
    }

    void Update()
    {
        // ถ้าไม่มีผู้เล่น, ผู้เล่นยังไม่ได้เก็บกระจก หรือผู้เล่นไม่ได้กดยกกระจกขึ้นมาใช้งาน (isMirrorUsing == false) -> ไม่ต้องหนี
        if (playerCube == null || !playerCube.hasMirror || !playerCube.isMirrorUsing)
        {
            return;
        }

        // 🚨 กันบั๊ก AI: เช็กความพร้อมของ NavMeshAgent ก่อนใช้งาน
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        // ==========================================================
        // 🔮 ถอดลอจิก Dot Product ส่องกระจกมาจาก ObjectSightCheck ของพี่เป๊ะ ๆ
        // ==========================================================
        
        // 1. หาเวกเตอร์ทิศทางจากตัว Player ไปหาตัวศัตรูตัวนี้
        Vector3 directionToMe = (transform.position - playerCube.transform.position).normalized;
        directionToMe.y = 0f; // ตัดแกน Y ทิ้งเพื่อคิดแค่ระนาบพื้นดิน

        // 2. ดึงทิศทางการหันหน้าของตัว Player ออกมา
        Vector3 playerForward = playerCube.transform.forward;
        playerForward.y = 0f;

        // 3. คำนวณความตรงในการจ้องมอง (Dot Product)
        float lookMatch = Vector3.Dot(playerForward.normalized, directionToMe.normalized);

        // 4. คำนวณระยะห่างระหว่างผู้เล่นกับศัตรูตัวนี้
        float currentDistance = Vector3.Distance(playerCube.transform.position, transform.position);

        // 🚨 เงื่อนไขการหนี: ถ้าผู้เล่นหันหน้าจ้องตรงมาหา (lookMatch >= visionAccuracy) และอยู่ในระยะส่องกระจก (currentDistance <= maxDistance)
        if (lookMatch >= visionAccuracy && currentDistance <= maxDistance)
        {
            FleeFromPlayer(directionToMe);
        }
    }

    private void FleeFromPlayer(Vector3 directionFromPlayer)
    {
        // คำนวณจุดหมายปลายทางใหม่ที่จะวิ่งหนี: ทิศทางที่ผู้เล่นส่องมา คูณด้วยระยะหนี
        Vector3 newFleePosition = transform.position + directionFromPlayer.normalized * fleeDistance;

        // ป้องกัน AI เดินชนขอบแมพจมกำแพง: สุ่มหาพิกัดแผ่นสีฟ้า NavMesh ที่ใกล้ที่สุดรอบจุดหมายหนี 2 เมตร
        NavMeshHit hit;
        if (NavMesh.SamplePosition(newFleePosition, out hit, 2.0f, NavMesh.AllAreas))
        {
            // ปลุก AI ให้ก้าวขา และส่งพิกัดให้วิ่งหนีสุดชีวิตทันที
            agent.isStopped = false;
            agent.SetDestination(hit.position);
            
            Debug.Log($"😱 {gameObject.name} โดนส่องกระจกเข้าหน้าจัง ๆ! วิ่งหนีเร็ว!");
        }
    }

    // วาดวงกลมสีชมพูในหน้า Scene เพื่อให้พี่มองเห็นรัศมีระยะกลัวของศัตรูตัวนี้ได้ง่ายขึ้นครับ
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
}