using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Engine))]
public class WheelPlacement : MonoBehaviour
{
    [Tooltip("Cannot be less than the wheel diameter")]
    [Range(0f, 10.0f)]
    public float wheelSpacing;

    private Engine mEngine;

    internal float wheelDiameter;
    internal Vector3 chasisOffset;

    void Start()
    {
        mEngine = GetComponent<Engine>();
        wheelDiameter = mEngine.leftWheel.GetComponent<SphereCollider>().radius + mEngine.rightWheel.GetComponent<SphereCollider>().radius;
        chasisOffset = mEngine.mainChasis.transform.position - ((mEngine.leftTyre.position + mEngine.rightTyre.position) / 2);
    }

    
    void Update()
    {
        wheelSpacing = Mathf.Clamp(wheelSpacing, wheelDiameter, Mathf.Infinity);

        if (!Application.isPlaying)
        {
            mEngine.axle.localPosition = Vector3.zero;
            mEngine.axle.localRotation = Quaternion.identity;
            mEngine.wheelShaft.transform.SetPositionAndRotation(mEngine.axle.position, mEngine.axle.rotation);

            mEngine.leftWheel.transform.position = mEngine.axle.position - (mEngine.axle.right * wheelSpacing / 2);
            mEngine.rightWheel.transform.position = mEngine.axle.position + (mEngine.axle.right * wheelSpacing / 2);

            mEngine.leftTyre.position = mEngine.leftWheel.transform.position;
            mEngine.rightTyre.position = mEngine.rightWheel.transform.position;

            mEngine.mainChasis.transform.position = chasisOffset + ((mEngine.leftTyre.position + mEngine.rightTyre.position) / 2);
        }
    }
}