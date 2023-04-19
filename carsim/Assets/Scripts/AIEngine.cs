using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIEngine : MonoBehaviour
{

    private float inputAccelerationLeft;
    private float inputAccelerationRight;
    private float currentSteerAngle;

    public float maxTorque = 100f;
    public float maxSteeringAngle = 45f;


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
        this.UpdateWheels();
    }
    public void SetInput(List<float> input)
    {
        //TODO vertauscht
        this.inputAccelerationLeft = input[1];
        this.inputAccelerationRight = input[0];
    }

    public void HandleMotor()
    {
        //resistance slows down car when not accelarating
        // grows with velocity + (signed in direction of vel) constant

        float resistance = (this.getCarVelocity() * 10f) + Math.Sign(this.getCarVelocity()) * 15f;


        //frontLeftWheelCollider.motorTorque = (inputAccelerationLeft * motorForce) - resistance;
        //frontRightWheelCollider.motorTorque = (inputAccelerationRight * motorForce) - resistance;


        // Calculate steering angle for each wheel based on difference in acceleration
        float accelerationDiff = Math.Abs(this.inputAccelerationRight) - Math.Abs( this.inputAccelerationLeft);
        float steeringAngle = maxSteeringAngle * accelerationDiff;
 
        // Apply differential torque to the wheels based on steering angle
        //leftTorque *= 1 - differentialFactor * Mathf.Abs(steeringAngle);
        //rightTorque *= 1 + differentialFactor * Mathf.Abs(steeringAngle);

        // Apply torque and steering angle to the left wheel
        frontLeftWheelCollider.motorTorque = (inputAccelerationLeft * this.maxTorque) - resistance; ;
        frontLeftWheelCollider.steerAngle = steeringAngle;

        // Apply torque and steering angle to the right wheel
        frontRightWheelCollider.motorTorque = (inputAccelerationRight * this.maxTorque) - resistance;
        frontRightWheelCollider.steerAngle = steeringAngle;

    }

    public void ResetMotor()
    {

        this.inputAccelerationLeft = 0;
        this.inputAccelerationRight = 0;
        frontLeftWheelCollider.steerAngle = 0;
        frontRightWheelCollider.steerAngle = 0;

        frontLeftWheelCollider.motorTorque = 0;
        frontRightWheelCollider.motorTorque = 0;
        frontLeftWheelCollider.attachedRigidbody.velocity = Vector3.zero;
        frontRightWheelCollider.attachedRigidbody.angularVelocity = Vector3.zero;



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

    public float getSteeringAngle()
    {
        //TODO check if correct angle gives back 90° should have 0
        return this.carBody.eulerAngles.y;
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
