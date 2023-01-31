using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System;
using System.IO;

public class CarAgent : Agent
{

    public AIEngine drivingEngine;
    private bool debugSavePictures = false;

    private ImageRecognitionPipeline imagePreprocess;
    private Camera cam;
    private SpawnManager spawnManager;
    private int resWidth = 256;
    private int resHeight = 64;
    private GameObject finishLine;
    private GameManager gameManager;

    public override void Initialize()
    {
        this.imagePreprocess = new ImageRecognitionPipeline();
        this.cam = gameObject.GetComponentInChildren<Camera>();
        this.finishLine = GameObject.FindGameObjectWithTag("FinishCheckpoint");
        this.gameManager = transform.parent.gameObject.GetComponentInChildren<GameManager>();
        //get spawn manager
        this.spawnManager = transform.parent.gameObject.GetComponentInChildren<SpawnManager>();
        //this.gameManager.InitializeMapWithObstacles();

    }

    public override void OnEpisodeBegin()
    {
        //on every restart clear map and load map again
        this.gameManager.DestroyObstaclesOnMap();
        this.gameManager.InitializeMapWithObstacles();
        this.Respawn();
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        var actionInputFromNN = actions.ContinuousActions;
        List<float> input = new List<float>() { actionInputFromNN[0], actionInputFromNN[1] };

        //print("NN Input: [" + input[0] + ", " + input[1] + "]");

        this.drivingEngine.SetInput(input);
        //var debugObservations = this.GetObservations();

        // after very action add 1 / distance to finishline as reward
        float distance = Vector3.Distance(this.gameObject.transform.localPosition, this.finishLine.transform.localPosition);
        //print("Distance: " + distance);
        this.AddReward(-1 * (distance/1000));


        //print(debugObservations);
    }

    //Collecting extra Information that isn't picked up by the RaycastSensors
    public override void CollectObservations(VectorSensor sensor)
    {

        Byte[] cameraPicture = this.GetCameraInput();

        List<List<Vector3>> obstacePositions = this.imagePreprocess.GetCooridnates10ClosestObstacles(this.transform.position, cameraPicture);

        // add speed input
        sensor.AddObservation(this.drivingEngine.getCarVelocity());

        //add actual steering
        sensor.AddObservation(this.drivingEngine.getSteeringAngle());

       // add sensor for 3 nearest recognized blue obstacles

       sensor.AddObservation(obstacePositions[0][0]);
       sensor.AddObservation(obstacePositions[0][1]);
       sensor.AddObservation(obstacePositions[0][2]);

       // // add sensor for 3 nearest recognized red obstacles
       sensor.AddObservation(obstacePositions[1][0]);
       sensor.AddObservation(obstacePositions[1][1]);
       sensor.AddObservation(obstacePositions[1][2]);

       // // add sensor for 3 nearest recognized yellow obstacles
       sensor.AddObservation(obstacePositions[2][0]);
       sensor.AddObservation(obstacePositions[2][1]);
       sensor.AddObservation(obstacePositions[2][2]);

    }

    //Get the AI vehicles camera input encode as byte array
    public Byte[] GetCameraInput ()
    {
        RenderTexture rt = new RenderTexture(this.resWidth, this.resHeight, 24);
        this.cam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(this.resWidth, this.resHeight, TextureFormat.RGB24, false);
        this.cam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
       
        Byte[] pictureInBytes = screenShot.EncodeToPNG();
        this.cam.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        Destroy(screenShot);

        if (this.debugSavePictures) {
            File.WriteAllBytes("PicturesOfAi/cameraAiCloneInput.png", pictureInBytes);
        }
        return pictureInBytes;
    }

    public void Respawn()
    {
        Rigidbody theRB = this.gameObject.GetComponentInChildren<Rigidbody>();
        Vector3 pos = spawnManager.SelectRandomSpawnpoint().localPosition;
        Quaternion spawnRotation = spawnManager.SelectRandomSpawnpoint().localRotation;
        theRB.MovePosition(pos);
        theRB.MoveRotation(spawnRotation);
        transform.localRotation = spawnRotation;
        transform.localPosition = pos - new Vector3(0, 0.4f, 0);
    }


    //For manual testing with human input, the actionsOut defined here will be sent to OnActionRecieved
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var action = actionsOut.ContinuousActions;
        action.Clear();

        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        //print("Heuristic Input: [" + action[0] + ", " + action[1] + "]");
    }


}
