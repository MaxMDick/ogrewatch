using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceTransformTest : MonoBehaviour
{
    public Vector3 eulerAngles;

    void LateUpdate()
    {
	    this.transform.localEulerAngles = eulerAngles;
    }
}
