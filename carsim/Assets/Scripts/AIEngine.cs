using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIEngine : MonoBehaviour
{

    private float inputAcceleration;
    private float inputSteering;
    private float currentSteerAngle;

    public float motorForce;
    public float maxSteerAngle;

    public WheelCollider frontLeftWheelCollider;
    public WheelCollider frontRightWheelCollider;
    public WheelCollider rearLeftWheelCollider;
    public WheelCollider rearRightWheelCollider;

    public Transform frontLeftWheelTransform;
    public Transform frontRightWheeTransform;
    public Transform rearLeftWheelTransform;
    public Transform rearRightWheelTransform;


    public Transform carBody;


    public void FixedUpdate()
    {
        this.HandleMotor();
        this.HandleSteering();
        this.UpdateWheels();
    }
    public void SetInput(List<float> input)
    {
        this.inputAcceleration = input[0];
        this.inputSteering = input[1];
    }

    public void HandleMotor()
    {
        //resistance slows down car when not accelarating
        // grows with velocity + (signed in direction of vel) constant

        float resistance = (this.getCarVelocity() * 10f) + Math.Sign(this.getCarVelocity()) * 15f;


        frontLeftWheelCollider.motorTorque = (inputSteering * motorForce) - resistance;
        frontRightWheelCollider.motorTorque = (inputSteering * motorForce) - resistance;

    }

    public void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * inputAcceleration;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    public void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheeTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    public void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
        // Debug.Log(pos);
        // (25, 87) , (-30, 90) (-30, (-25) , (24, -27)

    }


    public float getCarVelocity()
    {
        // transform objects that velocity on z axis always indicates the direction -> getting the Sign givs the direction
        float direction = Math.Sign(this.carBody.InverseTransformDirection(this.frontLeftWheelCollider.attachedRigidbody.velocity).z);

        // signed speed (foreward and backward speed)
        float velocity = direction * frontLeftWheelCollider.attachedRigidbody.velocity.magnitude;

        return velocity;

    }

}
