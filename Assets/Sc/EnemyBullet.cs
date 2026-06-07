using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 12f;
    public float lifeTime = 5f; // กระสุนจะทำลายตัวเองใน 5 วิถ้าไม่ชนอะไรเลย กันรกแมพ

    private Vector3 travelDirection;

    // ฟังก์ชันยัดทิศทางให้กระสุนพุ่งจากสคริปต์ศัตรู
    public void Launch(Vector3 direction)
    {
        travelDirection = direction.normalized;
        // หมุนหน้ากระสุนให้หันไปตามทิศที่พุ่งไปด้วย
        if (travelDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(travelDirection);
        }
        // สั่งทำลายตัวเองตามเวลาที่ตั้งไว้
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // ขยับกระสุนให้พุ่งไปข้างหน้าทุกเฟรม
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    // ระบบดักชนผู้เล่น: ถ้าหัวกระสุนทุบเข้าที่ตัว Cube ของเรา... ตายทันที!
    void OnTriggerEnter(Collider other)
    {
        // ดึงสคริปต์ CubeMovement จากวัตถุที่มันบินชน
        CubeMovement player = other.GetComponent<CubeMovement>();
        
        if (player != null)
        {
            Debug.Log($"💀 โดนกระสุนของ {gameObject.name} สอยร่วงเต็มเปา!");
            player.Die(); // เรียกใช้ฟังก์ชัน Die() จากสคริปต์ผู้เล่นของพี่ทันที
            Destroy(gameObject); // ทำลายกระสุนทิ้ง
        }
    }
}