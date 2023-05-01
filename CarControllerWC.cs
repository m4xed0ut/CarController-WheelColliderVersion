using System.Collections;
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
    public float maxTorque;
    public float maxSteeringAngle;

    Vector3 position;
    Quaternion rotation;

    public GameObject brakes;
    public GameObject smoke;
    public GameObject reverseLight;

    Vector3 com;
    Rigidbody rb;

    public AudioSource engine;
    public float gearRatio = 30;
    public float maxRpm = 3000;
    public float idle = 400;
    float currentGear = 1;

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

        float gamepadControl = maxTorque * Gamepad.current.rightTrigger.ReadValue();

        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");

        engine.pitch = Mathf.Clamp(rb.velocity.sqrMagnitude, idle, maxRpm) * Time.deltaTime / gearRatio;

        foreach (AxleInfo axleInfo in axleInfos)
        {

            if (Keyboard.current != null && Keyboard.current.downArrowKey.IsPressed(1))
            {
                axleInfo.leftWheel.brakeTorque = 3000;
                axleInfo.rightWheel.brakeTorque = 3000;
                brakes.SetActive(true);
            }
            else
            {
                axleInfo.leftWheel.brakeTorque = 0;
                axleInfo.rightWheel.brakeTorque = 0;
                brakes.SetActive(false);
            }

            if (Gamepad.current != null && Gamepad.current.leftTrigger.IsPressed(1))
            {
                axleInfo.leftWheel.brakeTorque = 3000;
                axleInfo.rightWheel.brakeTorque = 3000;
                brakes.SetActive(true);
            }
            else
            {
                axleInfo.leftWheel.brakeTorque = 0;
                axleInfo.rightWheel.brakeTorque = 0;
                brakes.SetActive(false);
            }

            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;

            }

            if (axleInfo.motor && Keyboard.current != null && Gamepad.current != null)
            {
                axleInfo.leftWheel.motorTorque = forward;
                axleInfo.rightWheel.motorTorque = forward;

                axleInfo.leftWheel.motorTorque = gamepadControl;
                axleInfo.rightWheel.motorTorque = gamepadControl;
            }

            if (axleInfo.leftWheel.isGrounded || axleInfo.rightWheel.isGrounded)
            {
                smoke.SetActive(true);
            }
            else
            {
                smoke.SetActive(false);
            }

            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }
    }

    void Gearbox()
    {
        Debug.Log(currentGear);

        if (engine.pitch == 2.0)
        {
            rb.drag = 0.5f;
        }
        else
        {
            rb.drag = 0;
        }

        if (Keyboard.current != null && Keyboard.current.aKey.wasPressedThisFrame && currentGear != 7)
        {
            gearRatio += 15;
            maxRpm += 1500;
            idle += 150;
            maxTorque -= 150;
            rb.drag = 0;
            currentGear++;
        }

        if (Keyboard.current != null && Keyboard.current.zKey.wasPressedThisFrame && currentGear != -1)
        {
            gearRatio -= 15;
            maxRpm -= 1500;
            idle -= 150;
            maxTorque += 150;
            rb.drag = 0.5f;
            currentGear--;
        }

        if (currentGear == 0)
        {
            reverseLight.SetActive(false);
            maxTorque = 0;
        }


        if (currentGear >= 7)
        {
            currentGear = 7;
        }


        if (currentGear == 1)
        {
            maxRpm = 3000;
            idle = 400;
            gearRatio = 30;
            maxTorque = 1150;
        }

        if (currentGear <= -1)
        {
            currentGear = -1;
        }

        if (currentGear == -1)
        {
            reverseLight.SetActive(true);
            maxRpm = 3000;
            idle = 400;
            gearRatio = 30;
            maxTorque = -1150;
        }

        if (Gamepad.current != null && Gamepad.current.aButton.wasPressedThisFrame && currentGear != 7)
        {
            gearRatio += 15;
            maxRpm += 1500;
            idle += 150;
            maxTorque -= 150;
            rb.drag = 0;
            currentGear++;
        }

        if (Gamepad.current != null && Gamepad.current.xButton.wasPressedThisFrame && currentGear != -1)
        {
            gearRatio -= 15;
            maxRpm -= 1500;
            idle -= 150;
            maxTorque += 150;
            rb.drag = 0.5f;
            currentGear--;
        }


    }

    private void Update()
    {
        Gearbox();

        foreach (AxleInfo axleInfo in axleInfos)
        {

            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                maxSteeringAngle = 25;

                WheelFrictionCurve grip;
                grip = axleInfo.leftWheel.sidewaysFriction;
                grip.extremumSlip = 0.8f;
                axleInfo.leftWheel.sidewaysFriction = grip;

                grip = axleInfo.rightWheel.sidewaysFriction;
                grip.extremumSlip = 0.8f;
                axleInfo.rightWheel.sidewaysFriction = grip;
            }

            if (Keyboard.current != null && Keyboard.current.upArrowKey.wasReleasedThisFrame)
            {
                maxSteeringAngle = 10;

                WheelFrictionCurve grip;
                grip = axleInfo.leftWheel.sidewaysFriction;
                grip.extremumSlip = 0.2f;
                axleInfo.leftWheel.sidewaysFriction = grip;

                grip = axleInfo.rightWheel.sidewaysFriction;
                grip.extremumSlip = 0.2f;
                axleInfo.rightWheel.sidewaysFriction = grip;
            }

            if (Gamepad.current != null && Gamepad.current.bButton.wasPressedThisFrame)
            {
                maxSteeringAngle = 25;

                WheelFrictionCurve grip;
                grip = axleInfo.leftWheel.sidewaysFriction;
                grip.extremumSlip = 0.8f;
                axleInfo.leftWheel.sidewaysFriction = grip;

                grip = axleInfo.rightWheel.sidewaysFriction;
                grip.extremumSlip = 0.8f;
                axleInfo.rightWheel.sidewaysFriction = grip;
            }

            if (Gamepad.current != null && Gamepad.current.rightTrigger.wasReleasedThisFrame)
            {
                maxSteeringAngle = 10;

                WheelFrictionCurve grip;
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
