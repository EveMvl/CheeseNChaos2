using UnityEngine;
using System;
using System.Collections.Generic;

public class CheeseManager : MonoBehaviour
{
    public static List<Transform> CheeseList { get; private set; } = new List<Transform>();
    public static event Action OnCheeseListChanged;

    public static void RegisterCheese(Transform cheese)
    {
        if (!CheeseList.Contains(cheese))
        {
            CheeseList.Add(cheese);
            Debug.Log($"🧀 Cheese Registered: {cheese.name} | Total Cheese: {CheeseList.Count}");
            OnCheeseListChanged?.Invoke();
        }
        else
        {
            Debug.LogWarning($"⚠️ Tried to register cheese already in list: {cheese.name}");
        }
    }

    public static void RemoveCheese(Transform cheese)
    {
        if (CheeseList.Contains(cheese))
        {
            CheeseList.Remove(cheese);
            Debug.Log($"❌ Cheese Removed: {cheese.name} | Remaining Cheese: {CheeseList.Count}");
            OnCheeseListChanged?.Invoke();
        }
        else
        {
            Debug.LogWarning($"⚠️ Tried to remove cheese not found: {cheese.name}");
        }
    }

    private void Update()
    {
        // optional runtime monitor (visible every few seconds)
        if (Time.frameCount % 300 == 0)
        {
            Debug.Log($"[CheeseManager] Current cheese count: {CheeseList.Count}");
        }
    }
}
