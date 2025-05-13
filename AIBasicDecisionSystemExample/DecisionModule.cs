using System.Collections.Generic;
using System.Threading.Tasks;

public class DecisionModule
{
    public async Task<Queue<AIAction>> MakePlan(PerceptionResults perceptionResults)
    {
        var actionQueue = new Queue<AIAction>();

        // Example decision-making logic
        if (perceptionResults.Enemies.Count > 0)
        {
            actionQueue.Enqueue(AIAction.Move);
            actionQueue.Enqueue(AIAction.Attack);
        }
        else if (perceptionResults.Items.Count > 0)
        {
            actionQueue.Enqueue(AIAction.Move);
        }
        else
        {
            actionQueue.Enqueue(AIAction.EndTurn);
        }

        await Task.Delay(200); // Simulate decision-making time
        return actionQueue;
    }
}

public enum AIAction
{
    Move,
    Attack,
    EndTurn
}