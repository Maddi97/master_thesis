using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";

    private float horizontalInput;
    private float verticalInput;
    private float currentSteerAngle;

    [SerializeField] private float motorForce;
    [SerializeField] private float maxSteerAngle;

    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheeTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;
    

	[SerializeField] private Transform carBody;
	[SerializeField] public int count;
    

    private void FixedUpdate()
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }


    private void GetInput()
    {
        horizontalInput = Input.GetAxis(HORIZONTAL);
        verticalInput = Input.GetAxis(VERTICAL);
    }

    private void HandleMotor()
    {
        //resistance velocity depanend + (or if backwards -) konstant to break when not accelerating
        float resistance = this.getCarVelocity() * 10f + Math.Sign(this.getCarVelocity()) * 15f;
        

        frontLeftWheelCollider.motorTorque = (verticalInput * motorForce) - resistance;
        frontRightWheelCollider.motorTorque = (verticalInput * motorForce)- resistance;

    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheeTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
        // Debug.Log(pos);
        // (25, 87) , (-30, 90) (-30, (-25) , (24, -27)

    }

    private void OnTriggerEnter(Collider other)
    {
	    if (other.tag == "red" && !(isleft(other.transform.position))){
			
		   count++;
		}else if(other.tag == "blue" && isleft(other.transform.position)){
			count++;
		}else{
			Debug.Log("Tag Problem:"+other.tag);
		}
		//text.text = b.ToString();
	    
    }
    
    private bool isleft(Vector3 position){
        Debug.Log("is left function triggered");
		Vector3 Dir = position - carBody.position;
		Dir = Quaternion.Inverse(carBody.rotation) * Dir;
		return (Dir.x>0);
	}

    private float getCarVelocity()
    {
        // transform objects that velocity on z axis always indicates the direction -> getting the Sign givs the direction
        float direction = Math.Sign(carBody.InverseTransformDirection(frontLeftWheelCollider.attachedRigidbody.velocity).z);

        // signed speed (foreward and backward speed)
        float velocity = direction * frontLeftWheelCollider.attachedRigidbody.velocity.magnitude;

        return velocity;

    }

}
