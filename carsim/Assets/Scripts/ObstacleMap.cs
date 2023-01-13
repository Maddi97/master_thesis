using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Json.Net;
using UnityEngine;

static class Constants
{
    public const int MIN_X = 4;
    public const int MAX_X = 18;
    public const float LEFTMOST_Z = 10;
    public const float RIGHTMOST_Z = 0.5f;
    public const float SPAWNHEIGHT_Y = 1.2f;
    public const float MINWIDTH_GOAL = 2;
    public const float MAXWIDTH_GOAL = 4;
    public const int MINXDISTANCEGOALS = 2;

}


public enum MapType
{
    random,
    easyGoalLaneMiddle,
    twoGoalLanes,

}
public class Goal
{
    public GameObject ObstacleGO;
    public Vector3[] Coords;

    public Goal(GameObject obstacle, Vector3[] coords)
    {
        ObstacleGO = obstacle;
        Coords = coords;

    }

}

public class ObstacleList
{
    public int listId;
    public Goal[] obstacles;
}


public class ObstacleMapManager : MonoBehaviour
{
    public List<UnityEngine.Object> obstacles = new List<UnityEngine.Object>();

    private GameObject obstacleBlue;
    private GameObject obstacleRed;

    public ObstacleMapManager(GameObject obstacleBlue, GameObject obstacleRed)
    {
        this.obstacleBlue = obstacleBlue;
        this.obstacleRed = obstacleRed;
    }


    public void IntantiateObstacles(ObstacleList obstacleList)
    {
        Quaternion obstacleForm = new Quaternion(0, 0, 1, 0);

        foreach (Goal obs in obstacleList.obstacles)
        {
            this.obstacles.Add(GameObject.Instantiate(obs.ObstacleGO, obs.Coords[0], obstacleForm));
            this.obstacles.Add(GameObject.Instantiate(obs.ObstacleGO, obs.Coords[1], obstacleForm));


        }
    }



    public void DestroyObstacles()
    {
        for (int i = 0; i < this.obstacles.Count; i++)
        {
            GameObject.Destroy(this.obstacles[i]);
        }
    }


    public ObstacleList LoadObastacleMap(string filepath, float id)
    {
        string fullPath = filepath + id.ToString() + ".json";
        if (File.Exists(fullPath))
        {
            string content = File.ReadAllText(fullPath);
            ObstacleList obstacleList = JsonUtility.FromJson<ObstacleList>(content);
            return obstacleList;

        }
        else
            throw new FileNotFoundException(
                "File not found.");
    }

    public void SaveObstacleMap(string filepath, float id, ObstacleList obstacleList)
    {
        string fullPath = filepath + id.ToString() + ".json";
        string json = JsonNet.Serialize(obstacleList);
        File.WriteAllText(fullPath, json);

    }


    // generate maps with different placement of obstacles
    public ObstacleList GenerateObstacleMap(MapType mapType, int id)
    {
        Goal[] obstacles = new Goal[0];

        switch (mapType)
            { 
            case MapType.random:
                obstacles = this.GenerateRandomObstacleMap();
                Debug.Log("Random Map generated");
                break;
            case MapType.easyGoalLaneMiddle:
                obstacles = this.GenerateEasyGoalLaneMiddleMap();
                Debug.Log("Easy middle lane map generated");
                break;
            case MapType.twoGoalLanes:
                obstacles = this.GenerateTwoGoalLanesMap();
                Debug.Log("Two lanes map generated");
                break;
            }

        ObstacleList obstacleList = new ObstacleList { listId= id, obstacles = obstacles };
             
        return obstacleList;

    }

    private Goal[] GenerateRandomObstacleMap()
    {
        Goal[] obstacles = new Goal[0];
        return obstacles;
    }

    private Goal[] GenerateEasyGoalLaneMiddleMap()
    {
        List<Goal> obstacles = new List<Goal>();
        // calculate how many goals

        int zLeftRow = 3;

        GameObject actualColorObject = obstacleBlue;

        int numberOfGoals = (int)((Constants.MAX_X - Constants.MIN_X) / Constants.MINXDISTANCEGOALS);

        for (int x = Constants.MIN_X; x < Constants.MAX_X ; x += Constants.MINXDISTANCEGOALS)
        {

            // left obstacles of goals
            Vector3 coordLeft = new Vector3(x, Constants.SPAWNHEIGHT_Y, zLeftRow);

            Vector3 coordRight = new Vector3(x, Constants.SPAWNHEIGHT_Y, zLeftRow + Constants.MAXWIDTH_GOAL );
            Vector3[] coordsGoal = { coordLeft, coordRight };

            Goal goal = new Goal(actualColorObject, coordsGoal);
            obstacles.Add(goal);

            actualColorObject = actualColorObject == obstacleBlue ? obstacleRed : obstacleBlue;

        }



        return obstacles.ToArray();


    }

    private Goal[] GenerateTwoGoalLanesMap()
    {

        List<Goal> obstacles = new List<Goal>();


        return obstacles.ToArray() ;
    }
}
