using UnityEngine;

public class OutlineProximityTrigger : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;   // ตัวผู้เล่น (Cube)
    
    // ตรงนี้เราดึงคลาส "Outline" จากสคริปต์ในรูปของพี่มาใช้ตรงๆ เลย
    public Outline outlineScript;       // ช่องสำหรับลากสคริปต์ Outline ตัวเดิมมาใส่

    [Header("Distance Settings")]
    public float activationDistance = 4f; // ระยะห่างที่เดินเข้ามาแล้ว Outline จะติด (เมตร)

    void Start()
    {
        // เริ่มเกมมา สั่งปิดไม่ให้เส้นขอบแสดงผลไว้ก่อน
        if (outlineScript != null)
        {
            outlineScript.enabled = false;
        }
    }

    void Update()
    {
        if (playerTransform == null || outlineScript == null) return;

        // คำนวณระยะห่างระหว่าง ไอเทมชิ้นนี้ กับ ตัวผู้เล่น (Cube)
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // ถ้าเดินเข้ามาใกล้กว่าระยะที่ตั้งไว้ (เช่น ต่ำกว่า 4 เมตร)
        if (distance <= activationDistance)
        {
            // เปิดให้สคริปต์ Outline ทำงาน (เส้นขึ้นพรึ่บ)
            if (!outlineScript.enabled)
            {
                outlineScript.enabled = true;
            }
        }
        else
        {
            // ถ้าเดินถอยห่างออกไปเกินระยะ = ปิดสคริปต์ Outline (เส้นหายไป)
            if (outlineScript.enabled)
            {
                outlineScript.enabled = false;
            }
        }
    }
}