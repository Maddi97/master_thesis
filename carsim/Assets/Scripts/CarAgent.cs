using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class CarAgent : Agent
{

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

    private DrivingEngine drivingEngine;

    public override void Initialize()
    {
        this.drivingEngine = new DrivingEngine(this.motorForce, this.maxSteerAngle, this.carBody, this.frontLeftWheelCollider, this.frontRightWheelCollider, this.rearLeftWheelCollider, this.rearRightWheelCollider,
           this.frontLeftWheelTransform, this.frontRightWheeTransform, this.rearLeftWheelTransform, this.rearRightWheelTransform);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        //if (this.transform.up.y > 0.1f)
        //{
          //  return;
       // }

        var actionInputFromNN = actions.ContinuousActions;
        List<float> input = new List<float>() { actionInputFromNN[0], actionInputFromNN[1] };



        this.drivingEngine.SetInput(input);
        this.drivingEngine.HandleMotor();
        this.drivingEngine.HandleSteering();
        this.drivingEngine.UpdateWheels();
    }

    public override void CollectObservations(VectorSensor sensor)
    {

        //TODO
        sensor.AddObservation( 20f);


    }

    //For manual testing with human input, the actionsOut defined here will be sent to OnActionRecieved
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var action = actionsOut.ContinuousActions;
        action.Clear();

        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
    }


}
