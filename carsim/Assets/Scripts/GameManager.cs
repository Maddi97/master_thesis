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

	public GameObject JetBot;

	// need to load the prefabs of the obstacles in unity here
	public GameObject obstacleBlue;
	public GameObject obstacleRed;
	public GameObject goalPassedWallCheckpoint;
	public GameObject goalMissedWallCheckpoint;
	public GameObject FinishLineCheckpoint;

	//spawn jetbot random on map if trainnig
	public Boolean isTrainingSpawnRandom = true;

	// has the last goal the finish line?
	public Boolean isFinishLineLastGoal = true;

	// initialize obstacle Map Generator
	private ObstacleMapManager obstacleMapManager;
	private ObstacleList obstacleList;

	// generate map
	//could be selected in unity in the GameManager game object 
	public MapType mapTypeGeneratedMap = MapType.twoGoalLanesBlueFirstLeft;


	// load obstacle Map
	public bool loadObstacles = false;
	public string loadObstacleMapFilePath = ".";

	// save map if generated
	public bool saveObstacles = false;
	public string saveObstacleMapFilePath = ".";


	//to store result
	private int result;

	//  
	public bool humanPilot = true;

	
    // Start is called before the first frame update
    void Start()
    {
		// load obstacles
	
		this.obstacleMapManager = new ObstacleMapManager(this.transform, obstacleBlue, obstacleRed, goalPassedWallCheckpoint, goalMissedWallCheckpoint, this.FinishLineCheckpoint, this.isFinishLineLastGoal, this.JetBot, this.isTrainingSpawnRandom);
		this.obstacleMapManager.SpawnJetBot();
		InitializeMapWithObstacles();
    }

	void FixedUpdate()//FixedUpdate is called at a constant interval
    {

	}

    // Update is called once per frame
    void Update()
    {

    }
      
    
    public void InitializeMapWithObstacles(){

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

	//retuns coords at beginning of the map (start point)
	public Vector3 GetStartSpawnPosition()
    {
		return this.obstacleMapManager.GetJetBotSpawnCoords();
    }

	//returns random spawn position on map
	public Vector3 GetRandomSpawnPosition()
    {
		return this.obstacleMapManager.GetJetBotRandomCoords();

	}

	public Quaternion GetRandomSpawnRotation()
    {
		return this.obstacleMapManager.JetBotRandomRotation();
    }

	public void DestroyObstaclesOnMap()
    {
		this.obstacleMapManager.DestroyMap();
    }
	public Boolean GetIsTrainingSpawnRandom()
    {
		return this.isTrainingSpawnRandom;

	}
}
