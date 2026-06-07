using UnityEngine;
using System.Collections;

public class EnemyUndergroundAttack : MonoBehaviour
{
    private Transform player;
    private bool isAttacking = false;
    private bool isDamageActive = false; // เช็กว่าช่วงเฟรมที่ทุบ ดาเมจทำงานอยู่ไหม

    [Header("Trap Detection Settings")]
    [Tooltip("รัศมีวงกลมรอบกับดัก ถ้าผู้เล่นเดินเข้ามาในระยะนี้ กับดักจะเริ่มทำงานสับสวิตช์ขึ้นพื้นแดงทันที")]
    public float detectionRadius = 4f; 

    [Header("Attack Objects (วัตถุลูกที่สั่งเปิด/ปิดตามจังหวะ)")]
    [Tooltip("ตัวโมเดลกล่อง/ร่างของศัตรู (ตอนแรกเอาติ๊กถูกออก พอโจมตีจะโผล่มาพร้อมดาเมจ)")]
    public GameObject enemyBodyModel;

    [Tooltip("ลาก Object พื้นหลังสีแดงใบ้เตือนบนพื้นมาใส่ช่องนี้ (DangerZone)")]
    public GameObject dangerZoneModel; 

    [Tooltip("ลาก Object ตัวชนจริง 'DamageCollider_Empty' มาใส่ช่องนี้")]
    public GameObject damageColliderObject; 

    [Header("Timing Settings")]
    [Tooltip("ระยะเวลาเตือน (วินาที) หลังจากพื้นแดงโผล่ ก่อนจะโผล่มาทุบจริง")]
    public float delayBeforeAttack = 1f;

    [Tooltip("ระยะเวลาที่เปิดพิกัดตัวชนจริงและโมเดลค้างไว้คาสนาม (วินาที)")]
    public float attackDuration = 0.3f; 
    
    [Tooltip("ระยะเวลาคูลดาวน์ (วินาที) ของกับดักหลังซ่อนตัวลงไป ก่อนจะตรวจจับผู้เล่นเพื่อเปิดใช้งานได้อีกครั้ง")]
    public float attackCooldown = 1.2f; 

    [Header("Special Distance Kill")]
    [Tooltip("รัศมีความกว้างในการฆ่า (เมตร) ถ้าผู้เล่นอยู่ในระยะนี้ตอนทุบจะตายทันที")]
    public float killRadius = 2.5f;

    void Start()
    {
        // 🚨 [ตัดระบบ NavMeshAgent ออกทั้งหมด] 
        // ตามหาผู้เล่นในสนามเพื่อเอาไว้เช็กพิกัดระยะห่าง (Distance)
        CubeMovement playerMovement = GameObject.FindAnyObjectByType<CubeMovement>();
        if (playerMovement != null)
        {
            player = playerMovement.transform;
        }
        
        // [ตั้งค่าเริ่มต้น] เอาติ๊กถูกออกให้หมดเกลี้ยง เพื่อซ่อนกับดักให้เนียนไปกับพื้นสนาม
        if (enemyBodyModel != null) enemyBodyModel.SetActive(false);
        if (dangerZoneModel != null) dangerZoneModel.SetActive(false);
        if (damageColliderObject != null) damageColliderObject.SetActive(false);
    }

    void Update()
    {
        if (player == null) return; 

        // 1. ระบบเช็กขอบเขตดาเมจฆ่าผู้เล่น (จะทำงานเฉพาะช่วงเฟรมที่ตัวศัตรูสับหน้าโผล่ขึ้นมาเท่านั้น)
        if (isDamageActive && damageColliderObject != null)
        {
            float distanceToPlayer = Vector3.Distance(damageColliderObject.transform.position, player.position);
            if (distanceToPlayer <= killRadius)
            {
                KillPlayer();
            }
        }

        // ถ้ากับดักกำลังชาร์จเตือนภัย หรือกำลังทุบคาคูลดาวน์อยู่ ให้ล็อกไว้ข้ามลูปตรวจจับไปก่อน
        if (isAttacking) return; 

        // 2. วัดระยะห่างระหว่างจุดตั้งกับดัก กับตัวผู้เล่น
        float distance = Vector3.Distance(transform.position, player.position);

        // ถ้าผู้เล่นเดินทะเล่อทะล่าเข้ามาในรัศมีตรวจจับของกับดัก สั่งสับสวิตช์ทำงานทันที!
        if (distance <= detectionRadius)
        {
            StartCoroutine(TrapAttackSequence());
        }
    }

    // ลอจิกขบวนการกับดัก: ผู้เล่นเหยียบ -> ขึ้นพื้นแดงเตือน -> ลบพื้นแดง -> ตัวศัตรู+คอลไลเดอร์โผล่ตูมพร้อมกัน -> ซ่อนตัวลงดินรอรีเซ็ตลูปใหม่
    IEnumerator TrapAttackSequence()
    {
        isAttacking = true;
        Debug.Log($"🪤 {gameObject.name} กับดักทำงาน! แจ้งเตือนแผ่นแดงบนพื้น...");

        // 1. เปิดแผ่นแจ้งเตือนสีแดง (DangerZone) ขึ้นมาเตือนภัยบนพื้นผิว
        if (dangerZoneModel != null) dangerZoneModel.SetActive(true);

        // ชาร์จแผ่นแดงค้างไว้ให้ผู้เล่นตาไวรีบกดแดช/สไลด์หลบออกไป
        yield return new WaitForSeconds(delayBeforeAttack);

        // 2. 🔥 [ทำตามสเปกเป๊ะ] เอาติ๊กถูกออกจากแผ่นแจ้งเตือนภัยสีแดง (DangerZone ปิดตัวลง)
        if (dangerZoneModel != null) dangerZoneModel.SetActive(false);

        // 3. 🔥 [ทำตามสเปกเป๊ะ] เปิดติ๊กถูกให้โมเดล Enemy และกล่องดาเมจ โผล่มาแทนที่พร้อมกันทันที!
        if (enemyBodyModel != null) enemyBodyModel.SetActive(true);
        if (damageColliderObject != null) damageColliderObject.SetActive(true);
        isDamageActive = true; 
        Debug.Log("💥 BOOM! กับดักทำงานเต็มตัว โมเดลศัตรูและ DamageCollider โผล่ขึ้นมาสับหน้าพร้อมกันแล้ว!");

        // เปิดแช่ร่างศัตรูค้างไว้ฟาดผู้เล่นแวบนึง
        yield return new WaitForSeconds(attackDuration);

        // 4. โจมตีเสร็จเรียบร้อย: ปิดระบบทำดาเมจ และซ่อนตัวโมเดลรวมถึงกล่องดาเมจกลับไปล่องหนตามเดิม
        isDamageActive = false;
        if (damageColliderObject != null) damageColliderObject.SetActive(false);
        if (enemyBodyModel != null) enemyBodyModel.SetActive(false);
        Debug.Log($"🕳️ กับดักมุดดินซ่อนตัวกลับไปสถานะเริ่มต้น...");

        // 5. พักเวลาระบบคูลดาวน์ของกับดัก ก่อนจะอนุญาตให้เช็กระยะเหยียบรอบใหม่ได้
        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }

    private void KillPlayer()
    {
        Debug.Log("💀 ผู้เล่นหลบไม่พ้น โดนกับดักสอยร่วงระเบิดตู้ม!");
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    // วาดวงกลมจำลองให้พี่เห็นระยะในหน้า Scene Editor เวลาจัดวางตำแหน่งกับดักได้ง่ายขึ้น
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan; // สีฟ้า: รัศมีวงกลมที่ผู้เล่นเดินมาเหยียบแล้วกับดักจะเริ่มทำงาน
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        Gizmos.color = Color.red;  // สีแดง: ขอบเขตรัศมีการทำลายล้างที่ฆ่าผู้เล่นตายจริง
        Gizmos.DrawWireSphere(transform.position, killRadius);
    }
}