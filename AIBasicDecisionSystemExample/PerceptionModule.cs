using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PerceptionModule : MonoBehaviour
{
    public async Task<PerceptionResults> Perceive()
    {
        // Simulate perception logic
        await Task.Delay(500);
        return new PerceptionResults
        {
            Enemies = new List<Vector3> { new Vector3(5, 0, 0), new Vector3(-3, 0, 0) },
            Items = new List<Vector3> { new Vector3(2, 0, 0) }
        };
    }
}

public class PerceptionResults
{
    public List<Vector3> Enemies { get; set; }
    public List<Vector3> Items { get; set; }
}