using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager
{
    private Dictionary<Object, Queue<Object>> m_poolsDict = new Dictionary<Object, Queue<Object>>();

    public void InitPool(Object prefab,int size)
    {
        if (m_poolsDict.ContainsKey(prefab))
        {
            return;
        }

        Queue<Object> queue = new Queue<Object>();
        for (int i = 0; i < size; i++)
        {
            Object go = Object.Instantiate(prefab);
            CreateGameObjectAndSetActive(go, false);
            queue.Enqueue(go);
        }

        m_poolsDict[prefab] = queue;
    }

    private void CreateGameObjectAndSetActive(Object obj, bool active)
    {
        GameObject itemGo = null;
        if (obj is Component)
        {
            Component component = obj as Component;
            itemGo = component.gameObject;
        }
        else
        {
            itemGo = obj as GameObject;
        }

        //itemGo.transform.SetParent(transform);
        itemGo.SetActive(active);
    }

    public T GetInstance<T>(Object prefab) where T : Object
    {
        Queue<Object> queue;
        if (m_poolsDict.TryGetValue(prefab, out queue))
        {
            Object obj;
            if (queue.Count>0)
            {
                obj= queue.Dequeue();
            }
            else
            {
                obj = Object.Instantiate(prefab);
            }
            CreateGameObjectAndSetActive(obj,true);
            queue.Enqueue(obj);
            return obj as T;
        }

        Debug.LogError($"There no such object type on Pool:{prefab.name}");
        return null;
    }
}
