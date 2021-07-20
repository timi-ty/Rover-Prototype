using UnityEngine;

[System.Serializable]
public class Gripper
{
    private const float MAX_OPEN_ANGLE = 70;

    public string name { get; private set; }

    private Transform dGripper { get { return dEndPoint.parent; } }
    private Transform sGripper { get { return sEndPoint.parent; } }
    private Transform wrist { get { return dGripper.parent; } }

    public Transform iKTarget;
    public Transform dEndPoint;
    public Transform sEndPoint;
    public Vector3 position { get { return iKTarget.position; } set { iKTarget.position = value; } }
    public Quaternion rotation { get { return iKTarget.rotation; } set { iKTarget.rotation = value; } }
    public Vector3 localPosition { get { return iKTarget.localPosition; } set { iKTarget.localPosition = value; } }
    public Vector3 localRestPosition { get; set; }
    private Vector3 dRestEulerAngles { get; set; }
    private Vector3 sRestEulerAngles { get; set; }
    public float restRotationY { get; private set; }
    /// <summary>
    /// Rotation of the whole claw with refrence to the wrist bone
    /// </summary>
    public float rotationY
    {
        /***All rotation manipulations here must be LOCAL!!!***/
        get
        {
            float difference = dGripper.localEulerAngles.y - sGripper.localEulerAngles.y;
            if (difference * difference > 0.1f)
            {
                Debug.LogWarning(name + ": " + "Grippers fell out of ROTATION synchronization. Sync will be forced by the D.Gripper");
                Vector3 sLocalEulerAngles = new Vector3(sGripper.localEulerAngles.x, dGripper.localEulerAngles.y, sGripper.localEulerAngles.z);
                sGripper.localEulerAngles = sLocalEulerAngles;
            }
            return dGripper.localEulerAngles.y;
        }
        set
        {
            Vector3 dLocalEulerAngles = new Vector3(dGripper.localEulerAngles.x, value, dGripper.localEulerAngles.z);
            Vector3 sLocalEulerAngles = new Vector3(sGripper.localEulerAngles.x, value, sGripper.localEulerAngles.z);
            dGripper.localEulerAngles = dLocalEulerAngles;
            sGripper.localEulerAngles = sLocalEulerAngles;
        }
    }
    public float averageGripperRadius { get; private set; }
    public float maxOpenness { get; private set; }
    public float openness
    {
        get
        {
            float dOpenAngle = dGripper.localEulerAngles.x - dRestEulerAngles.x;
            float sOpenAngle = sRestEulerAngles.x - sGripper.localEulerAngles.x;

            dOpenAngle = dOpenAngle < 0 ? dOpenAngle + 360 : dOpenAngle;
            sOpenAngle = sOpenAngle < 0 ? sOpenAngle + 360 : sOpenAngle;

            float difference = dOpenAngle - sOpenAngle;

            if(difference * difference > 0.1f)
            {
                Debug.LogWarning(name + ": " + "Grippers fell out of OPENNESS synchronization. Sync will be forced by the D.Gripper");

                sGripper.localEulerAngles = new Vector3(sRestEulerAngles.x - dOpenAngle, sGripper.localEulerAngles.y, sGripper.localEulerAngles.z);
            }

            return 2 * averageGripperRadius * Mathf.Sin(dOpenAngle * Mathf.Deg2Rad);
        }
        set
        {
            value = Mathf.Clamp(value, 0, maxOpenness);

            float openAngle = Mathf.Asin(value / (2 * averageGripperRadius)) * Mathf.Rad2Deg;

            dGripper.localEulerAngles = new Vector3(dRestEulerAngles.x + openAngle, dGripper.localEulerAngles.y, dGripper.localEulerAngles.z);
            sGripper.localEulerAngles = new Vector3(sRestEulerAngles.x - openAngle, sGripper.localEulerAngles.y, sGripper.localEulerAngles.z);
        }
    }
    public bool isActive { get; set; }

    public float GetPerpendicularAlignmentRotation(Vector3 alignmentVector)
    {
        Vector3 projectedAlignment = Vector3.ProjectOnPlane(alignmentVector, wrist.up);
        float rotationOffset = Vector3.SignedAngle(wrist.forward, projectedAlignment, wrist.up);
        return restRotationY + rotationOffset;
    }

    public void Init(string name)
    {
        this.name = name;
        localRestPosition = localPosition;
        restRotationY = rotationY;

        averageGripperRadius = ((dEndPoint.position - dGripper.position).magnitude + (sEndPoint.position - sGripper.position).magnitude) / 2.0f;
        maxOpenness = 2 * averageGripperRadius * Mathf.Sin(MAX_OPEN_ANGLE * Mathf.Deg2Rad);
    }
}