using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	/*
	 *This class controls the Game settings. In Unity & in this class you can choose which type pf map shoudl be
	 *generated and if there car should be controlled by a human or AI.
	 */


	// Corner coordinates of the arena: (0, 0) (0, 10) (20,10) (20, 0)


	public int idOfCurrentRun;

	// Game objects
	public GameObject vehicleAI; //holds bot vehicle gameobject
	public GameObject vehicleControlled; // vehicle object that is controlled by an human

	// need to load the prefabs of the obstacles in unity here
	public GameObject obstacleBlue;
	public GameObject obstacleRed;
	public GameObject goalPassedWallCheckpoint;
	public GameObject goalMissedWallCheckpoint;


	// initialize obstacle Map Generator
	private ObstacleMapManager obstacleMapManager;


	// generate map
	//could be selected in unity in the GameManager game object 
	public MapType mapTypeGeneratedMap = MapType.twoGoalLanes;


	// load obstacle Map
	public bool loadObstacles = false;
	public string loadObstacleMapFilePath = ".";

	// save map if generated
	public bool saveObstacles = false;
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
		this.obstacleMapManager = new ObstacleMapManager(obstacleBlue, obstacleRed, goalPassedWallCheckpoint, goalMissedWallCheckpoint);
		InitializeMapWithObstacles();

		//get Spawn Manager
		this.spawnManager = FindObjectOfType<SpawnManager>();

		if (this.humanPilot)
		{
			print("Spawn Cars");
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
            obstacleList = this.obstacleMapManager.LoadObastacleMap(this.loadObstacleMapFilePath, this.idOfCurrentRun);
		}
		else
		{

			// generate a new map with new obstacle, decide which type of map should be generated
			obstacleList = this.obstacleMapManager.GenerateObstacleMap(this.mapTypeGeneratedMap, this.idOfCurrentRun);

			if (this.saveObstacles)
            {
				this.obstacleMapManager.SaveObstacleMap(this.saveObstacleMapFilePath,
					this.idOfCurrentRun, obstacleList);

			}
		}

		// intantiate real objects in unity
		this.obstacleMapManager.IntantiateObstacles(obstacleList);

		idOfCurrentRun ++;

	}
    
    void getResult(){
		
	}
    
    void destroybots(){
		
	}
    
    void SpawnControlledCar()
    {

		Vector3 spawnPosition = spawnManager.SelectRandomSpawnpoint();
		GameObject controlledCar = Instantiate(vehicleControlled, spawnPosition, new Quaternion(0, 1, 0, 1));

	}

	void InitializeBots()
    {
		
	}
	
}
