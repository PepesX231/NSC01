using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonSimple : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneToLoad; 

    [Header("UI Elements")]
    public GameObject previewImage; // ลาก PreviewImage มาใส่ช่องนี้
    public GameObject outlineObject; // ลากตัวมันเอง (แฟ้มเหลือง) มาใส่ช่องนี้

    private UnityEngine.UI.Outline uiOutline;

    void Start()
    {
        // ดึง Outline ออกมาซ่อนไว้ก่อนตอนเริ่มเกม
        if (outlineObject != null)
        {
            uiOutline = outlineObject.GetComponent<UnityEngine.UI.Outline>();
            if (uiOutline != null) uiOutline.enabled = false;
        }
        
        if (previewImage != null) previewImage.SetActive(false);
    }

    // ฟังก์ชันตอนเมาส์ชี้เข้า (เราจะให้ Button เป็นคนเรียกใช้)
    public void OnHoverEnter()
    {
        if (uiOutline != null) uiOutline.enabled = true;
        if (previewImage != null) previewImage.SetActive(true);
    }

    // ฟังก์ชันตอนเมาส์เลื่อนออก (เราจะให้ Button เป็นคนเรียกใช้)
    public void OnHoverExit()
    {
        if (uiOutline != null) uiOutline.enabled = false;
        if (previewImage != null) previewImage.SetActive(false);
    }

    // ฟังก์ชันตอนคลิกปุ่ม
    public void OnClickButton()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}