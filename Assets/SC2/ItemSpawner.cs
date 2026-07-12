using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public GameObject[] itemPrefabs;
    public Transform playerTransform; // ลากตัว Cube มาใส่ตรงนี้ใน Inspector
    public float spawnInterval = 1.0f;
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnItem();
            timer = 0;
        }
    }

    void SpawnItem()
    {
        if (playerTransform == null) return;

        // ให้ X และ Z เกิดตาม Player แต่บวก/ลบค่าเล็กน้อยเพื่อสุ่มตำแหน่งใกล้ๆ
        float randomX = Random.Range(-2f, 2f); // ปรับเลขนี้ให้แคบลงถ้าอยากให้ตกใกล้ตัวมาก
        float randomZ = Random.Range(-2f, 2f);
        
        Vector3 spawnPosition = new Vector3(
            playerTransform.position.x + randomX, 
            10f, // ให้สูงจากพื้นขึ้นไป 10 หน่วย
            playerTransform.position.z + randomZ
        );
        
        int randomIndex = Random.Range(0, itemPrefabs.Length);
        Instantiate(itemPrefabs[randomIndex], spawnPosition, Quaternion.identity);
    }
}