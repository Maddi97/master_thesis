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
    public const float RIGHTMOST_Z = 0f;
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


    public Goal(GameObject obstacle, Vector3[] coords, GameObject goalPassedGameObject, GameObject goalMissedCheckpointWall)
    {
        ObstacleGO = obstacle;
        Coords = coords;


    }

    public void IntantiateGoal(Goal goal, GameObject passedCheckpointWall, GameObject missedCheckpointWall)
    {
        Vector3 coords0 = goal.Coords[0];
        Vector3 coords1 = goal.Coords[1];
        float widthObstacle = goal.ObstacleGO.transform.localScale.z;
        Quaternion goalRotationQuaternion = new Quaternion(0, 0, 0, 0);

        GameObject.Instantiate(goal.ObstacleGO, goal.Coords[0], goalRotationQuaternion);
        GameObject.Instantiate(goal.ObstacleGO, goal.Coords[1], goalRotationQuaternion);


        //instantiate passed checkpoint wall between obstacles 
        Vector3 pos = coords1 - coords0;
        GameObject passedWall = GameObject.Instantiate(passedCheckpointWall, this.GetMidPoint(coords0, coords1), goalRotationQuaternion);

        Vector3 actScale = passedWall.transform.localScale;
        // calculate length between obstacles 
        passedWall.transform.localScale += new Vector3(0, 0, pos.magnitude - actScale.z - widthObstacle);

        //intantiate missed obstacle wall beside obstacles

        //left from right obstacles (bigger z is right when view from spawn)
        if (coords0.z > coords1.z)
        {

            // intatiate missed Checkpoint wall from left obstacle of the goal to the border   
            float distanceToZLeft = Math.Abs(Constants.LEFTMOST_Z - coords0.z);
            Vector3 pointLeftBorder = new Vector3(coords0.x, coords0.y, Constants.LEFTMOST_Z);
            Vector3 midPointToLeftBorder = this.GetMidPoint(coords0, pointLeftBorder);

            GameObject missedWallLeft = GameObject.Instantiate(missedCheckpointWall, midPointToLeftBorder, goalRotationQuaternion);
            missedWallLeft.transform.localScale += new Vector3(0, 0, distanceToZLeft - 1.25f*widthObstacle);


            // intatiate missed Checkpoint wall from right obstacle of the goal to the border   
            float distanceToZRight = Math.Abs(Constants.RIGHTMOST_Z - coords1.z);
            Vector3 pointRightBorder = new Vector3(coords1.x, coords1.y, Constants.RIGHTMOST_Z);
            Vector3 midPointToRightBorder = this.GetMidPoint(coords1, pointRightBorder);

            GameObject missedWallRight = GameObject.Instantiate(missedCheckpointWall, midPointToRightBorder, goalRotationQuaternion);
            missedWallRight.transform.localScale += new Vector3(0, 0, distanceToZRight - 1.5f*widthObstacle);


        }
        // other obstacle is more left (coord1 is more left then coord0)
        else
        {
            // intatiate missed Checkpoint wall from left obstacle of the goal to the border   
            float distanceToZLeft = Math.Abs(Constants.LEFTMOST_Z - coords1.z);
            Vector3 pointLeftBorder = new Vector3(coords1.x, coords1.y, Constants.LEFTMOST_Z);
            Vector3 midPointToLeftBorder = this.GetMidPoint(coords1, pointLeftBorder);

            GameObject missedWallLeft = GameObject.Instantiate(missedCheckpointWall, midPointToLeftBorder, goalRotationQuaternion);
            missedWallLeft.transform.localScale += new Vector3(0, 0, distanceToZLeft - 1.25f * widthObstacle);

            // intatiate missed Checkpoint wall from right obstacle of the goal to the border   
            float distanceToZRight = Math.Abs(Constants.RIGHTMOST_Z - coords0.z);
            Vector3 pointRightBorder = new Vector3(coords0.x, coords0.y, Constants.RIGHTMOST_Z);
            Vector3 midPointToRightBorder = this.GetMidPoint(coords0, pointRightBorder);

            GameObject missedWallRight = GameObject.Instantiate(missedCheckpointWall, midPointToRightBorder, goalRotationQuaternion);
            missedWallRight.transform.localScale += new Vector3(0, 0, distanceToZRight - 1.5f * widthObstacle);
        }


    }

    private Vector3 GetMidPoint(Vector3 a, Vector3 b)
    {
        Vector3 c = a + b;
        Vector3 midPoint = new Vector3(0.5f * c.x, 0.5f * c.y, 0.5f * c.z);
        return midPoint;
    }

}

public class ObstacleList
{
    public int listId;
    public Goal[] goals;
}


public class ObstacleMapManager : MonoBehaviour
{
    public List<UnityEngine.Object> obstacles = new List<UnityEngine.Object>();

    private GameObject obstacleBlue;
    private GameObject obstacleRed;
    private GameObject goalPassedGameOjbect;
    private GameObject goalMissedGameObject;

    public ObstacleMapManager(GameObject obstacleBlue, GameObject obstacleRed, GameObject goalPassedGameObject, GameObject goalMissedGameObject)
    {
        this.obstacleBlue = obstacleBlue;
        this.obstacleRed = obstacleRed;
        this.goalPassedGameOjbect = goalPassedGameObject;
        this.goalMissedGameObject = goalMissedGameObject;
    }


    public void IntantiateObstacles(ObstacleList goalList)
    {

        foreach (Goal goal in goalList.goals)
        {
            goal.IntantiateGoal(goal, this.goalPassedGameOjbect, this.goalMissedGameObject);
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

        ObstacleList obstacleList = new ObstacleList { listId= id, goals = obstacles };
             
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

        for (int x = Constants.MIN_X; x < Constants.MAX_X ; x += Constants.MINXDISTANCEGOALS)
        {

            // left obstacles of goals
            Vector3 coordLeft = new Vector3(x, Constants.SPAWNHEIGHT_Y, zLeftRow);

            Vector3 coordRight = new Vector3(x, Constants.SPAWNHEIGHT_Y, zLeftRow + Constants.MAXWIDTH_GOAL );
            Vector3[] coordsGoal = { coordLeft, coordRight };

            Goal goal = new Goal(actualColorObject, coordsGoal, this.goalPassedGameOjbect, this.goalMissedGameObject);
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
