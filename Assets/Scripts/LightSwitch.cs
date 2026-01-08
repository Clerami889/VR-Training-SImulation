using UnityEngine;
using UnityEngine.XR;

public class LightSwitch : MonoBehaviour
{
    [SerializeField] private GameObject LightBulb;
    [SerializeField] private Transform leftControllerTransform;
    [SerializeField] private Transform rightControllerTransform;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private GameObject SwitchPart;
    private bool onToggle;
    private bool prevLeftY;
    private bool prevRightY;
   

    void Start()
    {
        if (LightBulb == null)
            LightBulb = GameObject.Find("LampuAtas");

        onToggle = LightBulb != null && LightBulb.activeSelf;
        prevLeftY = false;
        prevRightY = false;
       
    }

    void Update()
    {
        // Fetch both devices
        var leftDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        var rightDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        // Read Y-buttons
        bool leftY = leftDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool l) && l;
        bool rightY = rightDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool r) && r;

        // Rising-edge on left hand
        if (leftY && !prevLeftY)
            TryToggle(leftControllerTransform);

        // Rising-edge on right hand
        if (rightY && !prevRightY)
            TryToggle(rightControllerTransform);

        prevLeftY = leftY;
        prevRightY = rightY;

        
    }

    private void TryToggle(Transform controller)
    {
        if (Physics.Raycast(controller.position, controller.forward, out var hit, maxDistance))
        {
            if (hit.collider.gameObject == gameObject)
            {
                onToggle = !onToggle;
                LightBulb.SetActive(onToggle);
            }
        }
        float y = SwitchPart.transform.eulerAngles.y;
        float z = SwitchPart.transform.eulerAngles.z;
        float x = onToggle ? 150f : 150f;
        SwitchPart.transform.rotation = Quaternion.Euler(x, y, z);

    }


}