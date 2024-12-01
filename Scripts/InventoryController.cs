using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    private List<Key> keys = new List<Key>();

    public void AddKey(Key key)
    {
        keys.Add(key);
    }

    public Key FindKey(string keyID)
    {
        return keys.Find(k => k.keyID == keyID);
    }

    public void RemoveKey(Key key)
    {
        keys.Remove(key);
    }
}