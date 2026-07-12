using UnityEngine;
using UnityEngine.SceneManagement; // เพิ่มส่วนนี้เพื่อสั่งรีโหลดฉาก

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int score = 0;

    void Awake() { instance = this; }

    public void ApplyEffect(Item.ItemType type)
    {
        switch (type)
        {
            case Item.ItemType.Damage:
                Debug.Log("Game Over!");
                RestartGame(); // ตายทันที
                break;

            case Item.ItemType.Score:
                score += 10;
                Debug.Log("Score: " + score);
                if (score >= 100) Debug.Log("คุณชนะแล้ว!");
                break;
                
            // เพิ่ม Case อื่นๆ ได้ตามต้องการ
        }
    }

    void RestartGame()
    {
        // รีโหลดฉากปัจจุบันใหม่
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}