using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceNoRotation : MonoBehaviour
{
    
    void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }
}
