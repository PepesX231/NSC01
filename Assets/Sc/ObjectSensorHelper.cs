using UnityEngine;

public class ObjectSensorHelper : MonoBehaviour
{
    private ObjectSightCheck mainScript;

    void Start()
    {
        // วิ่งไปดึงสคริปต์หลักที่อยู่บนตัวพ่อมาเก็บไว้
        mainScript = GetComponentInParent<ObjectSightCheck>();
    }

    void OnTriggerEnter(Collider other) { if(mainScript != null) mainScript.SendMessage("OnTriggerEnter", other); }
    void OnTriggerStay(Collider other)  { if(mainScript != null) mainScript.SendMessage("OnTriggerStay", other); }
    void OnTriggerExit(Collider other)  { if(mainScript != null) mainScript.SendMessage("OnTriggerExit", other); }
}