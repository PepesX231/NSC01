using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("References")]
    public Transform player; // ลากตัว Cube มาใส่
    public GameObject warningIndicator; // Prefab แผ่นสีแดง
    public GameObject meteorPrefab;     // Prefab หิน
    public GameObject homingBombPrefab; // Prefab ระเบิดติดตาม

    [Header("Settings")]
    public float skillCooldown = 3f;
    public float detectRange = 15f; // ระยะที่บอสเริ่มโจมตี

    private bool isPlayerInRange = false;

    void Start() 
    { 
        if (player != null) StartCoroutine(BossLoop()); 
    }

    void Update()
    {
        // ตรวจสอบระยะห่างจาก Player ตลอดเวลา
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            isPlayerInRange = distance <= detectRange;
        }
    }

    IEnumerator BossLoop()
    {
        while (true)
        {
            if (isPlayerInRange)
            {
                // สุ่มสกิล 0 หรือ 1
                int skill = Random.Range(0, 2); 

                if (skill == 0) yield return StartCoroutine(SkillMeteor());
                else yield return StartCoroutine(SkillHomingBomb());

                yield return new WaitForSeconds(skillCooldown);
            }
            else
            {
                // ถ้านอกระยะ ให้รอเช็คเรื่อยๆ
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    IEnumerator SkillMeteor()
    {
        // 1. วางที่เตือน
        Vector3 targetPos = player.position;
        GameObject warning = Instantiate(warningIndicator, targetPos + Vector3.up * 0.1f, Quaternion.identity);
        
        // 2. รอ 1.5 วิ
        yield return new WaitForSeconds(1.5f);
        
        // 3. ทำลายที่เตือนและเสกหิน
        Destroy(warning);
        Instantiate(meteorPrefab, targetPos + Vector3.up * 15, Quaternion.identity);
    }

    IEnumerator SkillHomingBomb()
    {
        // เสกระเบิดติดตาม (ระเบิดต้องมีสคริปต์ Homing ในตัวมันเอง)
        Instantiate(homingBombPrefab, transform.position, Quaternion.identity);
        yield return null;
    }

    // วาดวงกลมระยะตรวจจับใน Editor ให้เห็นชัดๆ
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}