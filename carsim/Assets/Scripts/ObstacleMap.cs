using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Json.Net;
using UnityEngine;
using Random = System.Random;

static class Constants
{
    public const int X_Map_MIN = 3;
    public const int X_WIDTH_JETBOT = 3;

    public const int MIN_X = 7;
    public const int MAX_X = 18;
    public const int X_WIDTH = 14;

    //links und rechts vertauscht

    public const float LEFTMOST_Z = 10;
    public const float RIGHTMOST_Z = 0f;
    public const float Z_WIDTH = 10.5f;

    public const float SPAWNHEIGHT_Y = 1f;
    public const float MINWIDTH_GOAL = 2;
    public const float MAXWIDTH_GOAL = 4;
    public const int MINXDISTANCEGOALS = 4;
    public const int MAXXDISTANCEGOALS = 6;
}


public enum MapType
{
    random,
    easyGoalLaneMiddleBlueFirst,
    easyGoalLaneMiddleRedFirst,

    twoGoalLanesBlueFirstLeftMedium,
    twoGoalLanesBlueFirstRightMedium,
    twoGoalLanesRedFirstLeftMedium,
    twoGoalLanesRedFirstRightMedium,


    twoGoalLanesBlueFirstLeftHard,
    twoGoalLanesBlueFirstRightHard,
    twoGoalLanesRedFirstLeftHard,
    twoGoalLanesRedFirstRightHard,


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

    public GameObject IntantiateGoal(Goal goal, GameObject passedCheckpointWall, GameObject missedCheckpointWall, Vector3 gameManagerPosition)
    {
        Vector3 coords0 = goal.Coords[0];
        Vector3 coords1 = goal.Coords[1];
        float widthObstacle = goal.ObstacleGO.transform.localScale.z;
        Quaternion goalRotationQuaternion = new Quaternion(0, 0, 0, 0);

        //local coordinates dependent from game manager position
        float localLeftMostZ = Constants.Z_WIDTH + gameManagerPosition.z;
        float localRightMostZ = gameManagerPosition.z;

        //parent Gameobject, add all single goal items to goalParentGameObject
        GameObject goalParentGameObject = new(name: "Goal");

        //goalposts
        GameObject.Instantiate(goal.ObstacleGO, goal.Coords[0], goalRotationQuaternion, goalParentGameObject.transform);
        GameObject.Instantiate(goal.ObstacleGO, goal.Coords[1], goalRotationQuaternion, goalParentGameObject.transform);


        //instantiate passed checkpoint wall between obstacles 
        Vector3 pos = coords1 - coords0;
        GameObject passedWall = GameObject.Instantiate(passedCheckpointWall, this.GetMidPoint(coords0, coords1), goalRotationQuaternion, goalParentGameObject.transform);

        Vector3 actScale = passedWall.transform.localScale;
        // calculate length between obstacles 
        passedWall.transform.localScale += new Vector3(0, 0, pos.magnitude - actScale.z - widthObstacle);

        //intantiate missed obstacle wall beside obstacles

        //left from right obstacles (bigger z is right when view from spawn)
        if (coords0.z > coords1.z)
        {

            // intatiate missed Checkpoint wall from left obstacle of the goal to the border   
            float distanceToZLeft = Math.Abs(localLeftMostZ - coords0.z);
            Vector3 pointLeftBorder = new Vector3(coords0.x, coords0.y, localLeftMostZ);
            Vector3 midPointToLeftBorder = this.GetMidPoint(coords0, pointLeftBorder);

            GameObject missedWallLeft = GameObject.Instantiate(missedCheckpointWall, midPointToLeftBorder, goalRotationQuaternion, goalParentGameObject.transform);
            missedWallLeft.transform.localScale += new Vector3(0, 0, distanceToZLeft - 1.25f*widthObstacle);


            // intatiate missed Checkpoint wall from right obstacle of the goal to the border   
            float distanceToZRight = Math.Abs(localRightMostZ - coords1.z);
            Vector3 pointRightBorder = new Vector3(coords1.x, coords1.y, localRightMostZ);
            Vector3 midPointToRightBorder = this.GetMidPoint(coords1, pointRightBorder);

            GameObject missedWallRight = GameObject.Instantiate(missedCheckpointWall, midPointToRightBorder, goalRotationQuaternion, goalParentGameObject.transform);
            missedWallRight.transform.localScale += new Vector3(0, 0, distanceToZRight - 1.5f*widthObstacle);


        }
        // other obstacle is more left (coord1 is more left then coord0)
        else
        {
            // intatiate missed Checkpoint wall from left obstacle of the goal to the border   
            float distanceToZLeft = Math.Abs(localLeftMostZ - coords1.z);
            Vector3 pointLeftBorder = new Vector3(coords1.x, coords1.y, localLeftMostZ);
            Vector3 midPointToLeftBorder = this.GetMidPoint(coords1, pointLeftBorder);

            GameObject missedWallLeft = GameObject.Instantiate(missedCheckpointWall, midPointToLeftBorder, goalRotationQuaternion, goalParentGameObject.transform);
            missedWallLeft.transform.localScale += new Vector3(0, 0, distanceToZLeft - 1.25f * widthObstacle);

            // intatiate missed Checkpoint wall from right obstacle of the goal to the border   
            float distanceToZRight = Math.Abs(localRightMostZ - coords0.z);
            Vector3 pointRightBorder = new Vector3(coords0.x, coords0.y, localRightMostZ);
            Vector3 midPointToRightBorder = this.GetMidPoint(coords0, pointRightBorder);

            GameObject missedWallRight = GameObject.Instantiate(missedCheckpointWall, midPointToRightBorder, goalRotationQuaternion, goalParentGameObject.transform);
            missedWallRight.transform.localScale += new Vector3(0, 0, distanceToZRight - 1.5f * widthObstacle);
        }

        return goalParentGameObject;
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

    private Boolean isFinishLineLastGoal;
    private Boolean isTrainingSpawnRandom;
    private Vector3 gameManagerPosition;
    private Transform gameManagerTransform;
    private GameObject obstacleBlue;
    private GameObject obstacleRed;
    private GameObject goalPassedGameOjbect;
    private GameObject goalMissedGameObject;
    private GameObject finishlineCheckpoint;
    private GameObject allGoals;
    private GameObject JetBot;
    private double JetBotXSpawn;

    public ObstacleMapManager(Transform gameManagerTransform, GameObject obstacleBlue, GameObject obstacleRed, GameObject goalPassedGameObject, GameObject goalMissedGameObject, GameObject finishlineCheckpoint, Boolean isFinishLine, GameObject JetBot, Boolean isTrainingSpawnRandom)
    {
        this.gameManagerTransform = gameManagerTransform;
        this.gameManagerPosition = gameManagerTransform.position;
        this.obstacleBlue = obstacleBlue;
        this.obstacleRed = obstacleRed;
        this.goalPassedGameOjbect = goalPassedGameObject;
        this.goalMissedGameObject = goalMissedGameObject;
        this.finishlineCheckpoint = finishlineCheckpoint;
        this.isFinishLineLastGoal = isFinishLine;
        this.isTrainingSpawnRandom = isTrainingSpawnRandom;
        this.JetBot = JetBot;
        

}

    public void SpawnJetBot()
    {
        Vector3 SpawnPoint;
        if (this.isTrainingSpawnRandom)
        {
            SpawnPoint = this.GetJetBotRandomCoords();
        }
       else
        {
            SpawnPoint = this.GetJetBotSpawnCoords();
        }

        GameObject.Instantiate(original:this.JetBot, position:SpawnPoint, rotation: new Quaternion(0,1,0,1), this.gameManagerTransform.parent);

    }
    public Vector3 GetJetBotSpawnCoords()
    {

        int minXLocal = (int)(Constants.X_Map_MIN + this.gameManagerPosition.x);
        int z = (int)(this.gameManagerPosition.z + Constants.Z_WIDTH / 2);
        Vector3 SpawnPoint = new(minXLocal, 1, z);


        return SpawnPoint;
    }

    public Vector3 GetJetBotRandomCoords()
    {
        //local goal post coordinaents depend arena position
        float zLeftMax = (2 + this.gameManagerPosition.z);
        // left post of goal max 
        float zRightMax = (this.gameManagerPosition.z + Constants.Z_WIDTH - 2);
        int minXLocal = (int)(Constants.X_Map_MIN + this.gameManagerPosition.x);
        int maxXLocal = minXLocal + Constants.X_WIDTH - Constants.MINXDISTANCEGOALS;

        Random rnd = new Random();
        float zRandomCoord = (float)rnd.NextDouble() * (zRightMax - zLeftMax) + zLeftMax;
        float xRandomCoord = (float)rnd.NextDouble() * (maxXLocal - minXLocal) + minXLocal;
        Vector3 SpawnPoint = new(xRandomCoord, 1, zRandomCoord);
        this.JetBotXSpawn = xRandomCoord;

        return SpawnPoint;
    }

    public Quaternion JetBotRandomRotation()
    {
        Quaternion originalQuaternion = new Quaternion(0, 1, 0, 1);

        // Convert to Euler angles
        Vector3 currentRotation = originalQuaternion.eulerAngles;

        // Generate a random angle between -45 and 45 degrees
        float randomAngle = UnityEngine.Random.Range(-45f, 45f);

        // Add the random angle to the current rotation
        Vector3 modifiedRotation = currentRotation + new Vector3(0, randomAngle, 0);

        // Convert back to quaternion
        Quaternion modifiedQuaternion = Quaternion.Euler(modifiedRotation);
        return modifiedQuaternion;
    }


    public void IntantiateObstacles(ObstacleList goalList)
    {

        allGoals = new GameObject(name: "AllGoals");
        if (this.isFinishLineLastGoal == false)
        {

            foreach (Goal goal in goalList.goals)
            {
                GameObject goalIntantiatedGameObject = goal.IntantiateGoal(goal, this.goalPassedGameOjbect, this.goalMissedGameObject, this.gameManagerPosition);
                goalIntantiatedGameObject.transform.SetParent(allGoals.transform);

            }
        }
        else
        {
            for (int i = 0; i < goalList.goals.Length -1; i++)
            {

                Goal goal = goalList.goals[i];
                GameObject goalInstantiatedGameObject = goal.IntantiateGoal(goal, this.goalPassedGameOjbect, this.goalMissedGameObject, this.gameManagerPosition);
                goalInstantiatedGameObject.transform.SetParent(allGoals.transform);
            }

            //make passed checkpoint of last goal to finishLine checkpoint object
            Goal goalLast = goalList.goals[goalList.goals.Length-1];
            GameObject goalInstantiatedGameObjectLast = goalLast.IntantiateGoal(goalLast, this.finishlineCheckpoint, this.goalMissedGameObject, this.gameManagerPosition);
            goalInstantiatedGameObjectLast.transform.SetParent(allGoals.transform);
        }
    }


    public void DestroyMap()
    {
        //DestroyImmediate(this.allGoals);
        Destroy(this.allGoals);
       

    }

    public void DestroyObstacles()
    {
        for (int i = 0; i < this.obstacles.Count; i++)
        {
            GameObject.DestroyImmediate(this.obstacles[i]);
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
                //Debug.Log("Random Map generated");
                break;
            case MapType.easyGoalLaneMiddleBlueFirst:
                obstacles = this.GenerateEasyGoalLaneMiddleMap(true);
                //Debug.Log("Easy middle lane with blue obstacles first map generated");
                break;
            case MapType.easyGoalLaneMiddleRedFirst:
                obstacles = this.GenerateEasyGoalLaneMiddleMap(false);
                //Debug.Log("Easy middle lane with red obstacles first map generated");
                break;


            case MapType.twoGoalLanesBlueFirstLeftMedium:
                obstacles = this.GenerateTwoGoalLanesMapMedium(true, true);
                //Debug.Log("Two lanes map with blue obstacles first generated");
                break;
            case MapType.twoGoalLanesBlueFirstRightMedium:
                obstacles = this.GenerateTwoGoalLanesMapMedium(true, false);
                Debug.Log("Two lanes map with blue obstacles first generated");
                break;
            case MapType.twoGoalLanesRedFirstLeftMedium:
                obstacles = this.GenerateTwoGoalLanesMapMedium(false, true);
                Debug.Log("Two lanes map with red obstacles first generated");
                break;
            case MapType.twoGoalLanesRedFirstRightMedium:
                obstacles = this.GenerateTwoGoalLanesMapMedium(false, false);
                Debug.Log("Two lanes map with red obstacles first generated");
                break;


            case MapType.twoGoalLanesBlueFirstLeftHard:
                obstacles = this.GenerateTwoGoalLanesMapHard(true, true);
                //Debug.Log("Two lanes map with blue obstacles first generated");
                break;
            case MapType.twoGoalLanesBlueFirstRightHard:
                obstacles = this.GenerateTwoGoalLanesMapHard(true, false);
                Debug.Log("Two lanes map with blue obstacles first generated");
                break;
            case MapType.twoGoalLanesRedFirstLeftHard:
                obstacles = this.GenerateTwoGoalLanesMapHard(false, true);
                Debug.Log("Two lanes map with red obstacles first generated");
                break;
            case MapType.twoGoalLanesRedFirstRightHard:
                obstacles = this.GenerateTwoGoalLanesMapHard(false, false);
                Debug.Log("Two lanes map with red obstacles first generated");
                break;
            }

        ObstacleList obstacleList = new ObstacleList { listId= id, goals = obstacles };
             
        return obstacleList;

    }

    private Goal[] GenerateRandomObstacleMap()
    {
        List<Goal> obstacles = new List<Goal>();
        Random rnd = new Random();
        bool randomIsBlueFirst = rnd.Next(0, 2) == 0;

        GameObject actualColorObject;
        if (randomIsBlueFirst)
        {
            actualColorObject = this.obstacleBlue;
        }
        else
        {
            actualColorObject = this.obstacleRed;
        }

        //local goal post coordinaents depend arena position
        float zLeftMax = (1 + this.gameManagerPosition.z);
        // left post of goal max 
        float zRightMax = (5.5f + this.gameManagerPosition.z);
        int minXLocal = (int)(Constants.MIN_X + this.gameManagerPosition.x);
        int maxXLocal = minXLocal + Constants.X_WIDTH;

        if (this.isTrainingSpawnRandom)
        {
            //first goal random distance to JetBot
            minXLocal = (int)(this.JetBotXSpawn + rnd.Next(Constants.MINXDISTANCEGOALS, Constants.MAXXDISTANCEGOALS));

        }
        else
        {
        }

        // choose random distance between goals every round
        int xDistanceGoals = 0;
        for (int x = minXLocal; x < maxXLocal; x += xDistanceGoals)
        {
            // choose distance to next goal
            xDistanceGoals = rnd.Next(Constants.MINXDISTANCEGOALS, Constants.MAXXDISTANCEGOALS + 1);
            // choose z-position of left goal post random 
            float zLeftPost = (float)rnd.NextDouble() * (zRightMax - zLeftMax) + zLeftMax;

            Vector3[] coordsGoal = { new Vector3(), new Vector3() };

            Vector3 coordLeft = new Vector3(x, Constants.SPAWNHEIGHT_Y, zLeftPost);

            Vector3 coordRight = new Vector3(x, Constants.SPAWNHEIGHT_Y, zLeftPost + Constants.MAXWIDTH_GOAL);

            coordsGoal[0] = coordLeft;
            coordsGoal[1] = coordRight;

           
            Goal goal = new Goal(actualColorObject, coordsGoal, this.goalPassedGameOjbect, this.goalMissedGameObject);
            obstacles.Add(goal);

            actualColorObject = actualColorObject == obstacleBlue ? obstacleRed : obstacleBlue;
        }




        return obstacles.ToArray();
    }

    private Goal[] GenerateEasyGoalLaneMiddleMap(Boolean isBlueFirst=true)
    {
        List<Goal> obstacles = new List<Goal>();
        // calculate how many goals


        // local coordinates fore every train arena
        float zLeftRow = (Constants.Z_WIDTH/2 + this.gameManagerPosition.z - Constants.MAXWIDTH_GOAL/2);

        int minXLocal = (int)(Constants.MIN_X + this.gameManagerPosition.x);
        int maxXLocal = minXLocal + Constants.X_WIDTH - 2;
        GameObject actualColorObject;

        if (isBlueFirst)
        {
            actualColorObject = this.obstacleBlue;
        }
        else
        {
            actualColorObject = this.obstacleRed;
        }


        for (int x = minXLocal; x < maxXLocal ; x += Constants.MINXDISTANCEGOALS +1)
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

    private Goal[] GenerateTwoGoalLanesMapHard(Boolean isBlueFirst=true, Boolean isLeftFirst=true)
    {

        List<Goal> obstacles = new List<Goal>();

        GameObject actualColorObject;
        if (isBlueFirst) {
            actualColorObject = this.obstacleBlue;
        }
        else
        {
            actualColorObject = this.obstacleRed;
        }

        bool left = isLeftFirst;

        //local goal post coordinaents depend arena position
        float zLeftRow = (1.5f + this.gameManagerPosition.z);
        float zRightRow = (5f + this.gameManagerPosition.z);
        int minXLocal = (int)(Constants.MIN_X + this.gameManagerPosition.x);
        int maxXLocal = minXLocal + Constants.X_WIDTH -2;


        for (int x = minXLocal; x < maxXLocal; x += Constants.MAXXDISTANCEGOALS - 1)
        {
            Vector3[] coordsGoal = { new Vector3(), new Vector3() };

            //goal on left side
            if (left)
            {
                Vector3 coordLeft = new Vector3(x, Constants.SPAWNHEIGHT_Y, zLeftRow);

                Vector3 coordRight = new Vector3(x, Constants.SPAWNHEIGHT_Y, zLeftRow + Constants.MAXWIDTH_GOAL);
             
                coordsGoal[0] = coordLeft;
                coordsGoal[1] = coordRight;

            }
            else
            {
                Vector3 coordLeft = new Vector3(x, Constants.SPAWNHEIGHT_Y, zRightRow);

                Vector3 coordRight = new Vector3(x, Constants.SPAWNHEIGHT_Y, zRightRow + Constants.MAXWIDTH_GOAL);

                coordsGoal[0] = coordLeft;
                coordsGoal[1] = coordRight;
            }

            Goal goal = new Goal(actualColorObject, coordsGoal, this.goalPassedGameOjbect, this.goalMissedGameObject);
            obstacles.Add(goal);

            left = left == true ? false : true;
            actualColorObject = actualColorObject == obstacleBlue ? obstacleRed : obstacleBlue;
        }




            return obstacles.ToArray() ;
    }

    private Goal[] GenerateTwoGoalLanesMapMedium(Boolean isBlueFirst = true, Boolean isLeftFirst = true)
    {

        List<Goal> obstacles = new List<Goal>();

        GameObject actualColorObject;
        if (isBlueFirst)
        {
            actualColorObject = this.obstacleBlue;
        }
        else
        {
            actualColorObject = this.obstacleRed;
        }

        bool left = isLeftFirst;

        //local goal post coordinaents depend arena position
        float zLeftRow = (2.5f + this.gameManagerPosition.z);
        float zRightRow = (4f + this.gameManagerPosition.z);
        int minXLocal = (int)(Constants.MIN_X + this.gameManagerPosition.x);
        int maxXLocal = minXLocal + Constants.X_WIDTH - 2;


        for (int x = minXLocal; x < maxXLocal; x += Constants.MAXXDISTANCEGOALS - 1)
        {
            Vector3[] coordsGoal = { new Vector3(), new Vector3() };

            //goal on left side
            if (left)
            {
                Vector3 coordLeft = new Vector3(x, Constants.SPAWNHEIGHT_Y, zRightRow);

                Vector3 coordRight = new Vector3(x, Constants.SPAWNHEIGHT_Y, zRightRow + Constants.MAXWIDTH_GOAL);

                coordsGoal[0] = coordLeft;
                coordsGoal[1] = coordRight;

            }
            else
            {
                Vector3 coordLeft = new Vector3(x, Constants.SPAWNHEIGHT_Y, zLeftRow);

                Vector3 coordRight = new Vector3(x, Constants.SPAWNHEIGHT_Y, zLeftRow + Constants.MAXWIDTH_GOAL);

                coordsGoal[0] = coordLeft;
                coordsGoal[1] = coordRight;
                
            }

            Goal goal = new Goal(actualColorObject, coordsGoal, this.goalPassedGameOjbect, this.goalMissedGameObject);
            obstacles.Add(goal);

            left = left == true ? false : true;
            actualColorObject = actualColorObject == obstacleBlue ? obstacleRed : obstacleBlue;
        }




        return obstacles.ToArray();
    }
}
