using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Layers
{
    private static GameObject tempObject;
    private static int defaultLayer;

    public static int AllButGroundAndCharacter { get { return LayerMask.GetMask("Default"); } }
    public static int OnlyGround { get { return LayerMask.GetMask("Ground"); } }
    public static int TempGrabable { get { return LayerMask.GetMask("TempGrabable"); } }

    public static void SetToTempGrabable(GameObject grabable)
    {
        if(tempObject && LayerMask.LayerToName(tempObject.layer).Equals("TempGrabable"))
        {
            Debug.LogWarning(tempObject.name + " was persisting on the TempGrabable layer. Layers.ResetToDefault() should always be called after Layers.SetToTempGrabable().");
            tempObject.layer = defaultLayer;
        }

        tempObject = grabable;
        defaultLayer = grabable.layer;

        grabable.layer = LayerMask.NameToLayer("TempGrabable");
    }

    public static void ResetToDefault()
    {
        if (tempObject && LayerMask.LayerToName(tempObject.layer).Equals("TempGrabable"))
        {
            tempObject.layer = defaultLayer;
        }
        tempObject = null;
        defaultLayer = LayerMask.NameToLayer("Default");
    }
}
