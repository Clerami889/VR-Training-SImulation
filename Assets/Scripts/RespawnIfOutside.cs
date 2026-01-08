using UnityEngine;

public class RespawnIfOutside : MonoBehaviour
{
    public float outsideTimeToRespawn = 3f;
    public Transform respawnPoint;

    float outsideTimer = 0f;
    bool isOutside = false;

    void Update()
    {
        if (isOutside)
        {
            outsideTimer += Time.deltaTime;
            if (outsideTimer >= outsideTimeToRespawn)
            {
                DoRespawn();
            }
        }
    }

    // Called when this object's collider exits a trigger (the Play Area)
    void OnTriggerExit(Collider other)
    {
        // Accept either tag "PlayArea" or an object named "PlayArea" if tag not set
        if (other.CompareTag("PlayArea") || other.name == "PlayArea")
        {
            isOutside = true;
            outsideTimer = 0f;

        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayArea") || other.name == "PlayArea")
        {
            isOutside = false;
            outsideTimer = 0f;

        }
    }

    public void DoRespawn()
    {
        Vector3 targetPos = respawnPoint != null ? respawnPoint.position : Vector3.zero;
        Quaternion targetRot = respawnPoint != null ? respawnPoint.rotation : Quaternion.identity;

        var rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            if (rb.isKinematic)
            {
                // Move kinematic bodies by setting transform
                transform.position = targetPos;
                transform.rotation = targetRot;
            }
            else
            {
                // For dynamic bodies use Rigidbody methods and clear velocities
                rb.position = targetPos;
                rb.rotation = targetRot;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.Sleep();
            }
        }
        else
        {
            transform.position = targetPos;
            transform.rotation = targetRot;
        }

        isOutside = false;
        outsideTimer = 0f;
        Debug.Log($"{name} respawned to {targetPos}");
    }
}