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

    // for debugging its public, then you can lift the TimeLimit in unity
    public float TimeLeft = 10f;
    public float t = 0f;

    private ImageRecognitionPipeline imagePreprocess;
    private Camera cam;
    private int resWidth = 512;
    private int resHeight = 256;

    private GameObject finishLine;
    private GameManager gameManager;
    private int numberOfObstaclesPerType = 4;
    private int numberMemoryTraces = 5;
    private List<List<List<Vector4>>> rememberObstaclePositions;
    
    public override void Initialize()
    {
        this.imagePreprocess = new ImageRecognitionPipeline();
        this.cam = gameObject.GetComponentInChildren<Camera>();
        this.gameManager = transform.parent.gameObject.GetComponentInChildren<GameManager>();
        //get spawn manager
        //this.gameManager.InitializeMapWithObstacles();
        this.rememberObstaclePositions = this.InitializeObstacleMemory();
    }

    public override void OnEpisodeBegin()
    {
        this.t = 0f;

        //if heuristic
        //this.gameManager = transform.parent.gameObject.GetComponentInChildren<GameManager>();

        //on every restart clear map and load map again

        this.gameManager.DestroyObstaclesOnMap();
        this.Respawn();

        this.gameManager.InitializeMapWithObstacles();

        //Instead destroy verschieben -> Car Agent hat alle wichtigen sachen kann nicht destroyd werden
        //this.gameManager.SpawnJetBot();
        this.rememberObstaclePositions = this.InitializeObstacleMemory();
    }

    public void FixedUpdate()
    {
        //small reward for speed
        // Debug.Log(this.drivingEngine.getCarVelocity() / 100f);
        
        this.t += Time.deltaTime;
        //float distance = Vector3.Distance(this.gameObject.transform.localPosition, this.finishLine.transform.localPosition);
        //float distanceReward = 1 / distance;

        float velo = this.drivingEngine.getCarVelocity();

        //Debug.Log(velo);
       // this.AddReward(distanceReward * Time.deltaTime);
        if (velo > 0)
        {
            this.AddReward((velo / 10f) * Time.deltaTime);

        }
        //Debug.Log(this.t);
        //this.AddReward(-0.01f * Time.deltaTime);
        if (this.t >= this.TimeLeft)
        {
            this.AddReward(-1f);
            this.EndEpisode();
            //UnityEngine.Debug.Log("Ended episode because of time");

        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var actionInputFromNN = actions.ContinuousActions;
        List<float> input = new List<float>() { actionInputFromNN[0], actionInputFromNN[1] };

        //print("NN Input: [" + input[0] + ", " + input[1] + "]");

        this.drivingEngine.SetInput(input);
        //var debugObservations = this.GetObservations();

        // after very action add 1 / distance to finishline as reward
        if(this.finishLine != null) {
            float distance = Vector3.Distance(this.gameObject.transform.localPosition, this.finishLine.transform.localPosition);
        }
        //print("Distance: " + distance);
        //this.AddReward(-1 * (distance/100));

        //print(debugObservations);
    }

    //Collecting extra Information that isn't picked up by the RaycastSensors
    public override void CollectObservations(VectorSensor sensor)
    {

        Byte[] cameraPicture = this.GetCameraInput();
        //this.imagePreprocess.saveImageToPath(cameraPicture, "camPic.png");
        List<List<Vector4>> obstaclePositions = this.imagePreprocess.GetCooridnatesNClosestObstacles(this.transform.position, cameraPicture, n: numberOfObstaclesPerType);

        this.rememberObstaclePositions = this.imagePreprocess.TraceObstcalePosition(obstaclePositions, this.rememberObstaclePositions);
        // add speed input
       // sensor.AddObservation(this.drivingEngine.getCarVelocity());

        //add actual rotation of object x is up and down

        //TODO check if local rotation true for every car
       // sensor.AddObservation(this.transform.localEulerAngles.y);
        //sensor.AddObservation(this.transform.localEulerAngles.z);


        //add actual steering
        //sensor.AddObservation(this.drivingEngine.getSteeringAngle());

        //only add current observed obstacles without memory
        this.AddObstacleObservationWithoutMemory(sensor, obstaclePositions);

        // add with memory
        //this.AddObstaclePositionsWithMemory(sensor, this.rememberObstaclePositions);
       
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
        // if not spawn random, vehicle is respawnd at start of the map
        if (this.gameManager.GetIsTrainingSpawnRandom() == false)
        {
            Rigidbody theRB = this.gameObject.GetComponentInChildren<Rigidbody>();
            Vector3 pos = this.gameManager.GetStartSpawnPosition();
            Quaternion spawnRotation = Quaternion.Euler(0, 90, 0);
            theRB.MovePosition(pos);
            theRB.MoveRotation(spawnRotation);
            transform.localRotation = spawnRotation;
            transform.position = pos;
            this.drivingEngine.ResetMotor();
        }
        //else vehicle is randomly spawn
        else
        {
            Rigidbody theRB = this.gameObject.GetComponentInChildren<Rigidbody>();
            Vector3 pos = this.gameManager.GetRandomSpawnPosition();
            // Quaternion spawnRotation = spawnManager.SelectRandomSpawnpoint().localRotation;
            Quaternion spawnRotation = this.gameManager.GetRandomSpawnRotation();
            theRB.MovePosition(pos);
            theRB.MoveRotation(spawnRotation);
            transform.localRotation = spawnRotation;
            transform.position = pos;
            this.drivingEngine.ResetMotor();
        }
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

    public void AddTime(float time)
    {
        this.t -= time;
    }


    public void AddObstaclePositionsWithMemory(VectorSensor sensor, List<List<List<Vector4>>> obstaclePositionsMemory)
    {
        foreach(var obstaclePositions in obstaclePositionsMemory)
        {
            this.AddObstacleObservationWithoutMemory(sensor, obstaclePositions);
        }
    }

    public void AddObstacleObservationWithoutMemory(VectorSensor sensor, List<List<Vector4>> obstaclePositions)
    {
        // add sensor for 3 nearest recognized red obstacles
        foreach (Vector4 rect in obstaclePositions[0])
        {
            sensor.AddObservation(rect.x);
            sensor.AddObservation(rect.y);
            sensor.AddObservation(rect.w);
            sensor.AddObservation(rect.z);

        }

        // add sensor for 3 nearest recognized blue obstacles
        foreach (Vector4 rect in obstaclePositions[1])
        {
            sensor.AddObservation(rect.x);
            sensor.AddObservation(rect.y);
            sensor.AddObservation(rect.w);
            sensor.AddObservation(rect.z);

        }

        // add sensor for 3 nearest recognized yellow obstacles
        foreach (Vector4 rect in obstaclePositions[2])
        {
            sensor.AddObservation(rect.x);
            sensor.AddObservation(rect.y);
            sensor.AddObservation(rect.w);
            sensor.AddObservation(rect.z);

        }
    }

    public List<List<List<Vector4>>> InitializeObstacleMemory()
    {
        var outerList = new List<List<List<Vector4>>>();
        for (int i = 0; i < this.numberMemoryTraces; i++)
        {
            //blue red and wall
            var middleList = new List<List<Vector4>>();
            for (int j = 0; j < 3; j++)
            {
                var innerList = new List<Vector4>();
                for (int k = 0; k < this.numberOfObstaclesPerType; k++)
                {
                    innerList.Add(new Vector4());
                }
                middleList.Add(innerList);
            }
            outerList.Add(middleList);
        }
        return outerList;
    }

    public void ResetMotor()
    {
        this.drivingEngine.ResetMotor();
    }

    public float getTime()
    {
        return this.t;
    }
}
