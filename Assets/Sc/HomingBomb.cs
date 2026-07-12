using UnityEngine;

public class HomingBomb : MonoBehaviour
{
    public float speed = 5f;
    public float detectionRange = 10f; // ระยะที่เริ่มติดตาม
    public float lifeTime = 5f;        // เวลาที่อยู่ได้ก่อนหายไปเอง
    
    private Transform player;
    private bool isTracking = false;   // สถานะว่าเริ่มบินตามหรือยัง

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // เริ่มนับถอยหลังทำลายตัวเองทันทีที่เกิดมา
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 1. ถ้ายังไม่ตาม และ Player เข้ามาในระยะ ถึงจะเริ่มติดตาม
        if (!isTracking && distanceToPlayer <= detectionRange)
        {
            isTracking = true;
        }

        // 2. ถ้าอยู่ในสถานะ Tracking ให้บินเข้าหา
        if (isTracking)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // เช็คว่า Player มีสคริปต์ CubeMovement ไหมก่อนเรียกใช้
            CubeMovement pScript = other.GetComponent<CubeMovement>();
            if (pScript != null)
            {
                pScript.Die();
            }
            Destroy(gameObject); 
        }
    }
}