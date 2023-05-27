using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class AxleInfo
{
    public bool motor;
    public bool steering;
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public Transform lWheel;
    public Transform rWheel;
}


public class CarControllerWC : MonoBehaviour
{
    public List<AxleInfo> axleInfos;

    Vector3 position;
    Quaternion rotation;
    Vector3 com;
    Rigidbody rb;

    [Header("Steering Settings")]
    public float maxSteeringAngle;

    [Header("VFX and SFX")]
    public GameObject brakes;
    public GameObject smoke;
    public GameObject reverseLight;
    public AudioSource engine;
    public GameObject snow;
    public GameObject tireScreech;

    [Header("Engine Settings")]
    public float maxTorque;
    public float gearRatio = 30;
    public float maxRpm = 3000;
    public float idle = 400;
    public float currentGear = 1;
    bool moving = false;
    bool reverse = false;

    [Header("Raycast")]
    public LayerMask terrain;
    public Transform raycastTarget;
    public float raycastDistance = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = com;
    }

    public void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0)
        {
            return;
        }

        Transform visualWheel = collider.transform.GetChild(0);

        collider.GetWorldPose(out position, out rotation);

        rotation = rotation * Quaternion.Euler(new Vector3(0, 0, 90));
        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }

    void FixedUpdate()
    {
        float forward = maxTorque * Keyboard.current.upArrowKey.ReadValue();

        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");

        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (moving)
            {
                engine.pitch = Mathf.Clamp(rb.velocity.sqrMagnitude, idle, maxRpm) * Time.deltaTime / gearRatio;
            }

            if (reverse)
            {
                engine.pitch = Mathf.Clamp(rb.velocity.sqrMagnitude, idle, maxRpm) * Time.deltaTime / gearRatio;
            }

            if (engine.pitch == 2 && axleInfo.leftWheel.isGrounded)
            {
                rb.drag = 1;
            }


            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }

            if (axleInfo.motor && Keyboard.current != null)
            {
                axleInfo.leftWheel.motorTorque = forward;
                axleInfo.rightWheel.motorTorque = forward;
            }

            if (axleInfo.leftWheel.isGrounded || axleInfo.rightWheel.isGrounded)
            {
                smoke.SetActive(true);
            }
            else
            {
                rb.AddForce(Vector3.down * 600 * 10);
                smoke.SetActive(false);
                engine.pitch = Mathf.Clamp(axleInfo.leftWheel.rpm + axleInfo.rightWheel.rpm, idle, maxRpm) * Time.deltaTime / gearRatio;
            }

            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }
    }

    void Gearbox()
    {

        // gearbox logic
        if (currentGear == -1)
        {
            reverseLight.SetActive(true);
            maxRpm = 3000;
            idle = 400;
            gearRatio = 30;
            maxTorque = -1300;
            reverse = true;
        }

        if (currentGear == 0)
        {
            reverse = false;
            reverseLight.SetActive(false);
            maxTorque = 0;
            moving = false;

            engine.pitch = Mathf.Lerp(idle, maxRpm, Input.GetAxis("Vertical")) / 1500;
        }

        if (currentGear == 1)
        {
            moving = true;
            maxRpm = 3000;
            idle = 400;
            gearRatio = 30;
            maxTorque = 1300;
        }

        if (currentGear == 2)
        {
            maxTorque = 1150;
            gearRatio = 45;
            maxRpm = 4500;
            idle = 550;
        }

        if (currentGear == 3)
        {
            maxTorque = 1000;
            gearRatio = 60;
            maxRpm = 6000;
            idle = 700;
        }

        if (currentGear == 4)
        {
            maxTorque = 850;
            gearRatio = 75;
            maxRpm = 7500;
            idle = 850;
        }

        if (currentGear == 5)
        {
            maxTorque = 700;
            gearRatio = 90;
            maxRpm = 9000;
            idle = 1000;
        }

        if (moving)
        {
            // automatic gearbox

            // 2nd gear

            if (rb.velocity.magnitude <= 54 && engine.pitch >= 1.6f)
            {
                currentGear = 2;
            }

            if (rb.velocity.magnitude <= 52 && rb.velocity.magnitude >= 0)
            {
                currentGear = 1;
            }

            // 3rd gear

            if (rb.velocity.magnitude >= 65)
            {
                currentGear = 3;
            }

            if (rb.velocity.magnitude <= 65 && rb.velocity.magnitude >= 63)
            {
                currentGear = 2;
            }

            // 4th gear

            if (rb.velocity.magnitude >= 75)
            {
                currentGear = 4;
            }

            if (rb.velocity.magnitude <= 75 && rb.velocity.magnitude >= 73)
            {
                currentGear = 3;
            }

            // 5th gear

            if (rb.velocity.magnitude >= 84)
            {
                currentGear = 5;
            }

            if (rb.velocity.magnitude <= 84 && rb.velocity.magnitude >= 82)
            {
                currentGear = 4;
            }
        }

        
        if (Keyboard.current != null && Keyboard.current.aKey.wasPressedThisFrame && currentGear != 6)
        {
            currentGear++;
            rb.drag = 0;
        }
        

        if (Keyboard.current != null && Keyboard.current.zKey.wasPressedThisFrame && currentGear != -1)
        {
            currentGear--;
            rb.drag = 0;
        }

        if (currentGear >= 6)
        {
            currentGear = 6;
        }

        if (currentGear <= -1)
        {
            currentGear = -1;
        }
    }

    private void Update()
    {
        Gearbox();
        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (Keyboard.current != null && Keyboard.current.downArrowKey.IsPressed(1) && axleInfo.leftWheel.isGrounded)
            {
                rb.drag = 1.5f;
                brakes.SetActive(true);
            }
            else
            {
                rb.drag = 0;
                brakes.SetActive(false);
            }

            RaycastHit groundHit;
            if (Physics.Raycast(raycastTarget.position, -raycastTarget.up, out groundHit, raycastDistance, terrain))
            {
                maxSteeringAngle = 30;
                WheelFrictionCurve grip;
                grip = axleInfo.leftWheel.sidewaysFriction;
                grip.extremumSlip = 1.5f;
                axleInfo.leftWheel.sidewaysFriction = grip;

                grip = axleInfo.rightWheel.sidewaysFriction;
                grip.extremumSlip = 1.5f;
                axleInfo.rightWheel.sidewaysFriction = grip;

                if (rb.velocity.sqrMagnitude >= 2)
                {
                    snow.SetActive(true);
                }
                else
                {
                    snow.SetActive(false);
                }

            }
            else
            {
                maxSteeringAngle = 15;
                snow.SetActive(false);
                WheelFrictionCurve grip;
                grip = axleInfo.leftWheel.sidewaysFriction;
                grip.extremumSlip = 0.2f;
                axleInfo.leftWheel.sidewaysFriction = grip;

                grip = axleInfo.rightWheel.sidewaysFriction;
                grip.extremumSlip = 0.2f;
                axleInfo.rightWheel.sidewaysFriction = grip;


                if (Keyboard.current != null && Keyboard.current.spaceKey.IsPressed(1))
                {
                    grip = axleInfo.leftWheel.sidewaysFriction;
                    grip.extremumSlip = 0.8f;
                    axleInfo.leftWheel.sidewaysFriction = grip;

                    grip = axleInfo.rightWheel.sidewaysFriction;
                    grip.extremumSlip = 0.8f;
                    axleInfo.rightWheel.sidewaysFriction = grip;
                }
                else
                {
                    grip = axleInfo.leftWheel.sidewaysFriction;
                    grip.extremumSlip = 0.2f;
                    axleInfo.leftWheel.sidewaysFriction = grip;

                    grip = axleInfo.rightWheel.sidewaysFriction;
                    grip.extremumSlip = 0.2f;
                    axleInfo.rightWheel.sidewaysFriction = grip;
                }
            }
        }
    }
}
