using UnityEngine;

public class ColliderIgnore : MonoBehaviour
{
    [SerializeField] private Collider mugCollider;
    [SerializeField] private Collider playerBodyCollider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
         Physics.IgnoreCollision(mugCollider, playerBodyCollider, true);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
