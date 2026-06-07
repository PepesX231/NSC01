using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuButton : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneToLoad; 

    [Header("Hover Visual Settings")]
    public GameObject hoverImage; 

    // เปลี่ยนจาก Outline ธรรมดา เป็น UnityEngine.UI.Outline เพื่อป้องกันไม่ให้มันไปดึงสคริปต์ 3D มาผิดอันครับ
    private UnityEngine.UI.Outline uiOutline;

    void Start()
    {
        // บังคับเจาะจงดึงเฉพาะ Outline 2D ของระบบ UI เท่านั้น
        uiOutline = GetComponent<UnityEngine.UI.Outline>();
        
        // สั่งปิด Outline และภาพพรีวิวไว้ก่อนตอนเริ่มเกม
        if (uiOutline != null) uiOutline.enabled = false;
        if (hoverImage != null) hoverImage.SetActive(false);
    }

    // เมาส์ชี้เข้า -> เปิดเส้นขอบ และเปิดภาพพรีวิว
    public void OnMouseHoverEnter()
    {
        if (uiOutline != null) uiOutline.enabled = true;
        if (hoverImage != null) hoverImage.SetActive(true);
    }

    // เมาส์เลื่อนออก -> ปิดเส้นขอบ และปิดภาพพรีวิว
    public void OnMouseHoverExit()
    {
        if (uiOutline != null) uiOutline.enabled = false;
        if (hoverImage != null) hoverImage.SetActive(false);
    }

    // คลิกปุ่ม -> โหลดฉากสลับ Scene
    public void OnButtonClick()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log("กำลังโหลดเข้าสู่ Scene: " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("พี่ลืมใส่ชื่อ Scene ในช่อง Scene To Load หรือเปล่าครับ!");
        }
    }
}