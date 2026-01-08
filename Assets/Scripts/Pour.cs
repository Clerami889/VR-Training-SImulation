using NUnit.Framework;
using UnityEngine;

public class Pour : MonoBehaviour
{
    [SerializeField] private ParticleSystem waterStream;
    [SerializeField] private Transform pourDirection;
    [SerializeField] private float pourThreshold = 20f;
    [SerializeField] private GameObject pourTip;

    [SerializeField] AudioSource audioSource;

    [SerializeField] AudioClip tuang;

    [Header("What this bottle contains")]
    public LiquidType liquid = LiquidType.Tea;

    private bool isPouring;
    public bool IsPouring => isPouring;


    void Start()
    {
        waterStream?.Stop();
        if (pourTip) pourTip.SetActive(false);
    }

    void Update()
    {
        pour();


    }

    void pour()
    {
        float angle = Vector3.Angle(pourDirection.forward, Vector3.down);

        if (angle < pourThreshold && !isPouring)
        {
            waterStream.Play();
            if (pourTip) pourTip.SetActive(true);
            isPouring = true;
            // Assign the clip and enable looping
            audioSource.clip = tuang;
            audioSource.loop = true;
            audioSource.Play();


        }
        else if (angle >= pourThreshold && isPouring)
        {
            waterStream.Stop();
            if (pourTip) pourTip.SetActive(false);
            isPouring = false;

            audioSource.Stop();
            audioSource.loop = false;
        }
    }

}