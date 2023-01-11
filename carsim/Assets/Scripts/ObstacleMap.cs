using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Json.Net;
using UnityEngine;

public enum MapType
{
    random,
    easyGoalLaneMiddle,
    twoGoalLanes,

}
public class Obstacle
{
    public GameObject ObstacleGO;
    public Vector3 Coords;

    public Obstacle(GameObject obstacle, Vector3 coords)
    {
        ObstacleGO = obstacle;
        Coords = coords;

    }

}

public class ObstacleList
{
    public int listId;
    public Obstacle[] obstacles;
}


public class ObstacleMap : MonoBehaviour
{
    public List<UnityEngine.Object> obstacles = new List<UnityEngine.Object>();
    public int obstacleCount = 11;

    public GameObject obstacleblue;
    public GameObject obstaclered;

    public void IntantiateObstacles(ObstacleList obstacleList)
    {
        Quaternion obstacleForm = new Quaternion(0, 0, 1, 0);

        foreach (Obstacle obs in obstacleList.obstacles)
        {
            this.obstacles.Add(Instantiate(obs.ObstacleGO, obs.Coords, obstacleForm));

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
        Obstacle[] obstacles = new Obstacle[0];

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

    private Obstacle[] GenerateRandomObstacleMap()
    {
        Obstacle[] obstacles = new Obstacle[0];
        return obstacles;
    }

    private Obstacle[] GenerateEasyGoalLaneMiddleMap()
    {
        List<Obstacle> obstacles = new List<Obstacle>();

        for (int i = 1; i < obstacleCount - 1; i++)
        {
            Obstacle blueObstacle;
            Obstacle redObstacle;
            // vector bounds: (0, 15 , 0 ,  10 - 0)

            // left row
            // red
            redObstacle = new Obstacle(this.obstaclered, new Vector3(i * 2, (float)1.2, 1));

            //lerigtft row
            //blue
            blueObstacle = new Obstacle(this.obstacleblue, new Vector3(i * 2, (float)1.2, 10));


            obstacles.Add(blueObstacle);
            obstacles.Add(redObstacle);

        }

        return obstacles.ToArray();


    }

    private Obstacle[] GenerateTwoGoalLanesMap()
    {

        List<Obstacle> obstacles = new List<Obstacle>();

        for (int i = 1; i < obstacleCount - 1; i++)
        {
            Obstacle blueObstacle;
            Obstacle redObstacle;
            // vector bounds: (0, 15 , 0 ,  10 - 0)

            // right row
            if (i % 2 == 0)
            {
                // blue
                blueObstacle = new Obstacle (this.obstacleblue, new Vector3(i * 2, (float)1.2, 1));

                // red
                redObstacle = new Obstacle(this.obstaclered, new Vector3(i * 2, (float)1.2, 3));
            }

            //left row
            else
            {
                //blue
                blueObstacle = new Obstacle(this.obstacleblue, new Vector3(i * 2, (float)1.2, 7));

                // red
                redObstacle = new Obstacle(this.obstaclered, new Vector3(i * 2, (float)1.2, 10));

            }
            obstacles.Add(blueObstacle);
            obstacles.Add(redObstacle);

        }

        return obstacles.ToArray() ;
    }
}
