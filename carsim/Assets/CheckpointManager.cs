using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{

    private CarAgent carAgent;
    // Start is called before the first frame update
    void Start()
    {
        this.carAgent = GetComponentInParent<CarAgent>();
    }

    // automatically detects when transform to which this is assigned hit another object with a tag
    private void OnTriggerEnter(Collider other)
    {

        if (other.tag == "BlueObstacleTag")
        {
            //Debug.Log("Hit blue obstace");
            this.carAgent.AddReward(-1f);
            this.carAgent.EndEpisode();

        }
        if (other.tag == "RedObstacleTag")
        {
            //Debug.Log("Hit red obstace");
            this.carAgent.AddReward(-1f);
            this.carAgent.EndEpisode();

        }
        if (other.tag == "GoalPassed")
        {
            //Debug.Log("Passed goal reward");
            //this.carAgent.AddReward(100 / (this.carAgent.getTime()));
             this.carAgent.AddTime(10f);
            this.carAgent.AddReward(1f);
            // remove checkpoint wall gameobject to not hit it twice
            Destroy(other.gameObject);
            this.carAgent.EndEpisode();

        }
        if(other.tag == "GoalMissed")
        {
            //Debug.Log("Missed goal punishment");
            this.carAgent.AddReward(-1f);
            this.carAgent.EndEpisode();
        }
        if (other.tag == "Wall")
        {
            //Debug.Log("Wall punishment");
            this.carAgent.AddReward(-1f);
            this.carAgent.EndEpisode();

        }
        if(other.tag == "FinishCheckpoint")
        {
            //Debug.Log("Reward for finishing");
            this.carAgent.AddReward(1f);
            this.carAgent.EndEpisode();
        }

    }
}
