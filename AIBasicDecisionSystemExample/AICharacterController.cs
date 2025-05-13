using System.Threading.Tasks;
using UnityEngine;

public class AICharacterController : MonoBehaviour
{
    public PerceptionModule perceptionModule;
    public DecisionModule decisionModule;
    public ExecutionModule executionModule;

    public async Task StartTurn()
    {
        Debug.Log("AI Turn Started");

        // Step 1: Perceive the environment
        var perceptionResults = await perceptionModule.Perceive();
        Debug.Log("Perception Complete");

        // Step 2: Make a decision
        var actionQueue = await decisionModule.MakePlan(perceptionResults);
        Debug.Log("Decision Making Complete");

        // Step 3: Execute the plan
        await executionModule.ExecutePlan(actionQueue);
        Debug.Log("Execution Complete");
    }
}