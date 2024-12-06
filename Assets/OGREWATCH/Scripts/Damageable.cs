using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    public GameObject owner;
    public string partName;

    public GameObject GetOwner()
    {
        return owner;
    }

    public string GetPartName()
    {
        return partName;
    }
}
