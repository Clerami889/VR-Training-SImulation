using UnityEngine;

public class VRBoundary : MonoBehaviour
{
    [SerializeField] private Transform safePosition; // where to return player
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private GameObject warningWall; // optional visual

    private void OnTriggerExit(Collider ot)
    {
        if (ot.CompareTag(playerTag))
        {
            // Show warning wall or fade effect
            if (warningWall != null) warningWall.SetActive(true);
            
            // Teleport player back after short delay
            StartCoroutine(ReturnPlayer(ot.transform));
        }
    }

    private System.Collections.IEnumerator ReturnPlayer(Transform player)
    {
        yield return new WaitForSeconds(1f); // give them a moment
        player.position = safePosition.position;
        if (warningWall != null) warningWall.SetActive(false);
    }
}