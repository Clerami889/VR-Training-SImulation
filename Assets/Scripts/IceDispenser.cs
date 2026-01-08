using UnityEngine;

public class IceDispenser : MonoBehaviour
{
    [Tooltip("Optional ice prefab the dispenser can give to the mug. If empty, the mug must have its own prefab configured.")]
    [SerializeField] private GameObject icePrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Mug")) return;

        MugContent mugContent = other.GetComponentInParent<MugContent>();
        if (mugContent == null) return;

        if (!mugContent.HasIce)
        {
            // If dispenser provides a prefab, pass it to the mug
            if (icePrefab != null)
                mugContent.SetIcePrefabFromDispenser(icePrefab);

            mugContent.SetHasIce(true);
            Debug.Log($"IceDispenser: gave ice to {mugContent.name}");
        }
    }
}