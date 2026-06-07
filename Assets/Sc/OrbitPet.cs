using UnityEngine;

public class OrbitPet : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;           // ตัวผู้เล่น (Cube)
    
    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0f, 1.5f, -1.5f); // ระยะห่างจากตัวผู้เล่น (X, Y=ความสูง, Z=อยู่ข้างหลัง)
    public float followSpeed = 3f;     // ความเร็วในการลอยตาม (ยิ่งเยอะยิ่งตามไว)
    
    [Header("Hover Settings")]
    public float bobbingSpeed = 2f;    // ความเร็วในการลอยกระเพื่อมขึ้น-ลง (ให้ดูเหมือนลอยได้จริงๆ)
    public float bobbingAmount = 0.15f; // ความสูงในการลอยกระเพื่อม

    void LateUpdate()
    {
        if (target == null) return;

        // 1. คำนวณตำแหน่งเป้าหมายที่ควรจะอยู่ โดยอิงตามทิศทางและตำแหน่งของตัวผู้เล่น
        // target.TransformPoint จะช่วยคำนวณให้ตำแหน่ง offset อยู่ "ด้านหลัง" ตัวผู้เล่นเสมอ ไม่ว่าผู้เล่นจะหันหน้าไปทางไหน
        Vector3 targetPosition = target.TransformPoint(offset);

        // 2. ใส่เอฟเฟกต์ลอยกระเพื่อมขึ้น-ลง (Bobbing) เบาๆ ให้ดูนุ่มนวลเหมือนสิ่งมีชีวิตเวทมนตร์
        targetPosition.y += Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmount;

        // 3. ใช้ Vector3.Lerp เพื่อให้ก้อนกลมค่อยๆ ลอยเคลื่อนที่ตามหลังผู้เล่นมาแบบสมูทๆ ไม่แข็งกระด้าง
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // 4. หันหน้าเจ้าก้อนกลมตามทิศทางของผู้เล่น (เผื่อในอนาคตพี่เปลี่ยนโมเดลเป็ดหรือหุ่นยนต์ที่มีหน้าตา)
        transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, followSpeed * Time.deltaTime);
    }
}