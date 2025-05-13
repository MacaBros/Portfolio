using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ExecutionModule
{
    public async Task ExecutePlan(Queue<AIAction> actionQueue)
    {
        while (actionQueue.Count > 0)
        {
            var action = actionQueue.Dequeue();
            switch (action)
            {
                case AIAction.Move:
                    Debug.Log("Executing Move");
                    await Task.Delay(500); // Simulate movement
                    break;
                case AIAction.Attack:
                    Debug.Log("Executing Attack");
                    await Task.Delay(500); // Simulate attack
                    break;
                case AIAction.EndTurn:
                    Debug.Log("Ending Turn");
                    return;
            }
        }
    }
}