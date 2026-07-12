using UnityEngine;

public class Item : MonoBehaviour
{
    public enum ItemType { Damage, Score, Heal, Speed, Invincible }
    public ItemType type;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // ส่งค่าไปบอกระบบคุมเกม
            GameManager.instance.ApplyEffect(type);
            Destroy(gameObject);
        }
    }
}