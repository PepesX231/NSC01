using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    public Vector3 moveDirection = Vector3.right; // ทิศทางที่ต้องการให้เลื่อน (เช่น ขวา)
    public float distance = 5f;                   // ระยะที่เลื่อนไป (เมตร)
    public float speed = 2f;                      // ความเร็วในการเลื่อน

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position; // จำตำแหน่งเริ่มต้นไว้
    }

    void Update()
    {
        // ใช้ PingPong คำนวณตำแหน่งโดยอิงจากเวลา
        // ผลลัพธ์จะเป็นการเลื่อนไปและกลับอย่างต่อเนื่อง
        float offset = Mathf.PingPong(Time.time * speed, distance);
        transform.position = startPosition + (moveDirection.normalized * offset);
    }
}