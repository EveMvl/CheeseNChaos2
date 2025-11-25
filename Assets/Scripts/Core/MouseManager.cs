using UnityEngine;
using System.Collections.Generic;
using System;

public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance;

    private List<MouseAgent> allMice = new List<MouseAgent>();

    // ✅ Event to notify listeners when the mouse list changes
    public event Action OnMouseListChanged;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Register a mouse when it spawns
    /// </summary>
    public void RegisterMouse(MouseAgent mouse)
    {
        if (mouse != null && !allMice.Contains(mouse))
        {
            allMice.Add(mouse);
            OnMouseListChanged?.Invoke(); // notify UI or other systems
        }
    }

    /// <summary>
    /// Remove a mouse when it dies or is destroyed
    /// </summary>
    public void UnregisterMouse(MouseAgent mouse)
    {
        if (mouse != null && allMice.Contains(mouse))
        {
            allMice.Remove(mouse);
            OnMouseListChanged?.Invoke(); // notify UI or other systems

            // 🧠 Check if only one mouse remains → trigger lose
            if (allMice.Count <= 1)
            {
                Debug.Log("🐭 Only one mouse left! Game over!");
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.SendMessage("LoseGame", SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    Debug.LogWarning("⚠️ LevelManager not found — cannot trigger lose screen!");
                }
            }
        }
    }

    /// <summary>
    /// Get all active mice (optionally excluding cannibals or breeding mice)
    /// </summary>
    public List<MouseAgent> GetAllMice(bool excludeCannibals = true, bool excludeBreeding = true)
    {
        List<MouseAgent> result = new List<MouseAgent>();

        foreach (var m in allMice)
        {
            if (m == null) continue;
            if (excludeCannibals && m is CannibalMouseAgent) continue;
            if (excludeBreeding && m.isBreeding) continue;

            result.Add(m);
        }

        return result;
    }

    /// <summary>
    /// Returns the current number of mice (excluding null references)
    /// </summary>
    public int MouseCount => allMice.Count;

    /// <summary>
    /// Debug helper: prints all current mice
    /// </summary>
    public void DebugPrintMice()
    {
        Debug.Log($"🐭 MouseManager has {allMice.Count} mice:");
        foreach (var m in allMice)
        {
            if (m != null)
                Debug.Log($" - {m.name} (Adult: {m.isAdult}, Breeding: {m.isBreeding})");
        }
    }
}
