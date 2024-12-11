using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    public GameObject owner;
    public string partName;
    public bool parryCollider;

    public GameObject GetOwner()
    {
        return owner;
    }

    public string GetPartName()
    {
        return partName;
    }

    public bool IsParryCollider()
    {
        return parryCollider;
    }
}
