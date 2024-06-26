using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolBehaviour : SteeringBehaviour
{
    Vector2 startPoint;
    float patrolRadius;
    float randomSeed;
    float randomTime;

    public PatrolBehaviour(Transform ownerTrm, float patrolRadius) : base(ownerTrm)
    {
        this.patrolRadius = patrolRadius;
    }

    public void Setting(Vector2 startPoint, float randomSeed)
    {
        this.startPoint = startPoint;
        this.randomSeed = randomSeed;
        this.randomTime = Random.Range(0f, 100f);
    }

    public override (float[] danger, float[] interest) GetSteering(float[] danger, float[] interest, AIData aiData)
    {
        float perlinXValue = Perlin.Noise((Time.time + randomTime + randomSeed) / (patrolRadius + 2));
        float perlinYValue = Perlin.Noise((Time.time + randomTime) / (patrolRadius + 2));
        
        Vector2 directionToTarget = new Vector2(perlinXValue, perlinYValue);
        for (int i = 0; i < interest.Length; i++)
        {
            float distance = Vector2.Distance((Vector2)transform.position 
                                           + Directions.eightDirections[i], startPoint);
            float weight = 0;
            if (distance > patrolRadius)
            {
                weight = distance - patrolRadius;
            }

            float result = Vector2.Dot(directionToTarget, Directions.eightDirections[i]) - weight;
            result = Mathf.Clamp(result, 0, 1);
            interest[i] = result;
        }
        return (danger, interest);
    }

    public override void OnDestroy()
    {
        
    }
}
