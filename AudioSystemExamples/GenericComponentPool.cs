using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Based on Anders Malmgren generic performant pooling https://forum.unity.com/threads/perfomant-audiosource-pool.503056/
/// </summary>
/// <typeparam name="T"></typeparam>

public abstract class GenericComponentPool<T> where T : Component
{
    private readonly T prefab;

    protected readonly Queue<T> pool = new Queue<T>();
    private readonly LinkedList<T> inuse = new LinkedList<T>();
    private readonly Queue<LinkedListNode<T>> nodePool = new Queue<LinkedListNode<T>>();
 
    private int lastCheckFrame = -1;
 
    protected GenericComponentPool(T prefab)
    {
        this.prefab = prefab;
    }
 
    private void CheckInUse()
    {
        var node = inuse.First;
        while (node != null)
        {
            var current = node;
            node = node.Next;
 
            if (!IsActive(current.Value))
            {
                current.Value.gameObject.SetActive(false);
                pool.Enqueue(current.Value);
                inuse.Remove(current);
                nodePool.Enqueue(current);
            }
        }
    }
 
    protected T Get(GameObject parent = null)
    {
        T item;
 
        if (lastCheckFrame != Time.frameCount)
        {
            lastCheckFrame = Time.frameCount;
            CheckInUse();
        }

        if (pool.Count == 0)
        {
            item = GameObject.Instantiate(prefab);
            
            if(parent != null)
                item.transform.SetParent(parent.transform);
        }
        else
            item = pool.Dequeue();
 
        if (nodePool.Count == 0)
            inuse.AddLast(item);
        else
        {
            var node = nodePool.Dequeue();
            node.Value = item;
            inuse.AddLast(node);
        }
         
        item.gameObject.SetActive(true);
 
        return item;
    }
 
    protected abstract bool IsActive(T component);
}
