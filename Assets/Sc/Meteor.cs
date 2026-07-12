using UnityEngine;

public class Meteor : MonoBehaviour
{
    public float destroyTime = 5f; // ให้หินหายไปเองถ้าไม่โดนอะไรเลย

    void Start()
    {
        // ทำลายหินตัวเองอัตโนมัติหลังจากผ่านไป 5 วินาที เพื่อไม่ให้เปลือง Memory
        Destroy(gameObject, destroyTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // 1. เช็ค Tag ต้องเป็น "Player" เท่านั้น
        if (other.CompareTag("Player"))
        {
            // 2. พยายามเข้าถึงสคริปต์ CubeMovement
            CubeMovement player = other.GetComponent<CubeMovement>();
            
            if (player != null)
            {
                Debug.Log("หินโดน Player แล้ว! สั่ง Die");
                player.Die();
                Destroy(gameObject); // ชนแล้วหินหายไป
            }
        }
    }
}