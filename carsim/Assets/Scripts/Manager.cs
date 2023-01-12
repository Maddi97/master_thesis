using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class Manager : MonoBehaviour
{
	// arena coordinates: (0, 0) (0, 10) (20,10) (20, 0)


	//
	public int idOfCurrentRun;

	// Game objects
	public GameObject vehicleAI;//holds bot vehicle
	public GameObject vehicleControlled;

	// initialize obstacle Map
	public ObstacleMap obstacleMap = new();


	// generate map
	public MapType mapTypeGenerateMap = MapType.twoGoalLanes;


	// load obstacle Map
	private bool loadObstacles = false;
	public string loadObstacleMapFilePath = ".";

	// save map if generated
	private bool saveObstacles = false;
	public string saveObstacleMapFilePath = ".";

	private SpawnManager spawnManager;

	//to store result
	private int result;

	// 
	public bool humanPilot = true;

	
	private List<Bot> botList;

    // Start is called before the first frame update
    void Start()
    {
		// load obstacles
		InitializeMapWithObstacles();

		//get Spawn Manager
		this.spawnManager = FindObjectOfType<SpawnManager>();

		if (this.humanPilot)
		{
			SpawnControlledCar();
		}

		else
		{
			print("TODO Bot");
		}
    }

	void FixedUpdate()//FixedUpdate is called at a constant interval
    {
		
	}

    // Update is called once per frame
    void Update()
    {

    }
      
    
    void InitializeMapWithObstacles(){
		ObstacleList obstacleList;

		// load a already generated map
		if (loadObstacles)
		{
            obstacleList = this.obstacleMap.LoadObastacleMap(this.loadObstacleMapFilePath, this.idOfCurrentRun);
		}
		else
		{

			// generate a new map with new obstacle, decide which type of map should be generated
			obstacleList = this.obstacleMap.GenerateObstacleMap(this.mapTypeGenerateMap, this.idOfCurrentRun);

			if (this.saveObstacles)
            {
				this.obstacleMap.SaveObstacleMap(this.saveObstacleMapFilePath,
					this.idOfCurrentRun, obstacleList);

			}
		}

		// intantiate real objects in unity
		this.obstacleMap.IntantiateObstacles(obstacleList);

		idOfCurrentRun ++;

	}
    
    void getResult(){
		
	}
    
    void destroybots(){
		
	}
    
    void SpawnControlledCar()
    {
		Vector3 spawnPosition = spawnManager.SelectRandomSpawnpoint();
		GameObject controlledCar = Instantiate(vehicleControlled, spawnPosition, new Quaternion(0, 1, 0, 0));

	}

	void InitializeBots()
    {
		
	}
	
}
