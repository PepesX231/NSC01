using UnityEngine;
using System.Collections;

public class EnemyShooterAttack : MonoBehaviour
{
    private Transform player;
    private CubeMovement playerCube;
    private bool isAttacking = false;
    private bool isBlindedByMirror = false; // เช็กว่ากำลังโดนกระจกส่องตาแตกอยู่ไหม

    [Header("🎯 Ranged Detection Settings")]
    [Tooltip("ระยะหวังผลของป้อมปืน (เมตร) ถ้าผู้เล่นเข้ามาในวงกลมนี้ ป้อมจะเริ่มหมุนเล็งและยิงใส่ทันที")]
    public float attackRange = 12f; 
    
    [Range(0f, 1f)]
    [Tooltip("ค่าความตรงสายตาตอนส่องกระจกกลับมา (ล้อมาจาก ObjectSightCheck ของพี่)")]
    public float visionAccuracy = 0.75f;

    [Header("🚀 Shooter Settings (ระบบสับกระสุน)")]
    [Tooltip("ลาก Prefab ลูกกระสุนที่มีสคริปต์ EnemyBullet มาใส่ช่องนี้")]
    public GameObject bulletPrefab;
    
    [Tooltip("จุดพิกัดที่กระสุนจะโผล่ออกมา (สร้างปุ่ม/ปลายกระบอกปืนเปล่าๆ มาดึงใส่ช่องนี้ได้ครับ)")]
    public Transform shootPoint;
    
    [Tooltip("ความเร็วในการหมุนหน้าเล็งเป้าตามตัวผู้เล่น")]
    public float turnSpeed = 8f;
    
    [Tooltip("ระยะเวลาคูลดาวน์ในการสับกระสุนแต่ละนัด (วินาที) ยิ่งน้อยยิ่งยิงรัว")]
    public float fireRate = 1.5f;

    void Start()
    {
        // ตามหาตัวผู้เล่นในสนาม เพื่อเอาไว้ล็อกเป้าและเช็กระยะห่าง
        playerCube = GameObject.FindAnyObjectByType<CubeMovement>();
        if (playerCube != null)
        {
            player = playerCube.transform;
        }
    }

    void Update()
    {
        if (player == null || playerCube == null) return;

        // วัดระยะห่างระหว่างป้อมปืนตัวนี้ กับ ตัวผู้เล่น Cube
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // ==========================================================
        // 🔮 1. ลอจิกกระจกวิเศษ: ถ้าผู้เล่นส่องกระจกอัดหน้า ป้อมปืนจะเอ๋อ (หยุดยิง)
        // ==========================================================
        if (playerCube.hasMirror && playerCube.isMirrorUsing)
        {
            // หาเวกเตอร์ทิศทางจากตัวผู้เล่น ส่องมาหาป้อมปืน
            Vector3 directionToMe = (transform.position - player.position).normalized;
            directionToMe.y = 0f;

            Vector3 playerForward = player.transform.forward;
            playerForward.y = 0f;

            // คำนวณความตรงสายตา (Dot Product)
            float lookMatch = Vector3.Dot(playerForward.normalized, directionToMe.normalized);

            // ถ้าผู้เล่นหันหน้าส่องกระจกตรงเข้าหาป้อมปืนในระยะโจมตี
            if (lookMatch >= visionAccuracy && distanceToPlayer <= attackRange)
            {
                if (!isBlindedByMirror)
                {
                    isBlindedByMirror = true;
                    Debug.Log($"🔮 [{gameObject.name}] ว๊ากกก! แสบตา! โดนกระจกผู้เล่นส่องหน้า ยิงไม่ได้!");
                    
                    // ถ้ากำลังชาร์จยิงอยู่ ให้ยกเลิกนัดนั้นทันที
                    if (isAttacking)
                    {
                        StopAllCoroutines();
                        isAttacking = false;
                    }
                }
                return; // 🚨 สั่ง Return ตัดจบตรงนี้เลย เพื่อล็อกให้ป้อมปืนยืนเอ๋อ ไม่ทำการเล็งหรือยิงใดๆ ทั้งสิ้น!
            }
        }

        // ถ้าผู้เล่นเลิกส่องกระจก ให้ปลดล็อกสถานะตาบอด
        isBlindedByMirror = false;

        // ==========================================================
        // 🎯 2. ลอจิกการล็อกเป้าและระดมยิง (ทำงานเมื่อผู้เล่นเข้ามาในระยะ)
        // ==========================================================
        if (distanceToPlayer <= attackRange)
        {
            // 🔄 หมุนเฉพาะ "หน้าตา" ของป้อมปืนให้หันไปจ้องมองผู้เล่นตลอดเวลา (ไม่ขยับขาเดิน)
            Vector3 lookDirection = player.position - transform.position;
            lookDirection.y = 0f; // ล็อกแกน Y ไว้ไม่ให้ป้อมเงยหน้าลอยฟ้าหรือทิ่มลงดิน
            
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }

            // ถ้าคูลดาวน์เสร็จแล้ว ยิงนัดต่อไปทันที!
            if (!isAttacking)
            {
                StartCoroutine(StationaryShootCoroutine());
            }
        }
    }

    IEnumerator StationaryShootCoroutine()
    {
        isAttacking = true;

        Debug.Log($"💥 [{gameObject.name}] ป้อมปืนสับกระสุนโป้ง!");

        // กำหนดจุดเกิดกระสุน ถ้าไม่ใส่จะเกิดกลางตัวป้อมปืนพอดี
        Transform spawnPoint = shootPoint != null ? shootPoint : transform;
        
        if (bulletPrefab != null)
        {
            // ทำการสร้างไอเทมกระสุนขึ้นมาบนโลกเกม
            GameObject spawnedBullet = Instantiate(bulletPrefab, spawnPoint.position, Quaternion.identity);
            
            // คำนวณทิศทางพุ่งจากปากกระบอกปืน วิ่งไปหาพิกัดตัวผู้เล่น
            Vector3 fireDirection = (player.position - spawnPoint.position).normalized;

            // สั่งให้สคริปต์กระสุนพุ่งออกไปตามทิศทางที่คำนวณไว้
            EnemyBullet bulletScript = spawnedBullet.GetComponent<EnemyBullet>();
            if (bulletScript != null)
            {
                bulletScript.Launch(fireDirection);
            }
        }
        else
        {
            Debug.LogError($"❌ พี่ลืมลาก Prefab ลูกกระสุนมาใส่ในช่อง Bullet Prefab ของ {gameObject.name} ครับ!");
        }

        // นั่งรอเวลาคูลดาวน์ (Fire Rate) ก่อนจะอนุญาตให้ยิงนัดถัดไป
        yield return new WaitForSeconds(fireRate);

        isAttacking = false;
    }

    // วาดวงกลมพิกัดระยะยิงในหน้าตาสร้างด่าน (Scene Editor) จะได้รู้ว่าป้อมคุมพื้นที่ถึงไหน
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan; // สีฟ้า: วงรัศมีขอบเขตการยิงถล่มของป้อมปืนตัวนี้
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}