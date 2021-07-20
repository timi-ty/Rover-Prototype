using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionController : MonoBehaviour
{
    #region Inspector Parameters

    #region Gripper IK
    [Header("Gripper Control")]
    public Gripper leftGripper;
    public Gripper rightGripper;
    /// <summary>
    /// The common centre transform for the tails, attached to the base of the character
    /// </summary>
    public Transform tailBone;
    #endregion
    [Range(0f, 1f)]
    public float debugGripperOpen;

    #region Head IK
    [Header("Head Control")]
    public Transform headBone;
    public Transform lookTarget;
    #endregion

    #endregion

    #region Internal

    private Quaternion offsetRotation;
    private Vector3 offsetPosition;
    private CameraController tpCamera;

    #region Interaction
    private List<Grabable> mGrabables = new List<Grabable>();
    private Grabable activeGrabable;
    #endregion

    #endregion
    void Start()
    {
        tpCamera = FindObjectOfType<CameraController>();

        offsetPosition = transform.position - tailBone.position;
        offsetRotation = transform.rotation * Quaternion.Inverse(tailBone.rotation);

        leftGripper.Init("Left Gripper");
        rightGripper.Init("Right Gripper");

        StartCoroutine(IdleClawMovement());

    }

    
    void FixedUpdate()
    {
        transform.position = tailBone.position + offsetPosition;
        transform.rotation = tailBone.rotation * offsetRotation;

        Vector3 lookVector = (headBone.transform.position - tpCamera.transform.position).normalized;
        lookTarget.position = headBone.position + lookVector;
    }

    public void Grab()
    {
        if (activeGrabable && activeGrabable.isInFocus)
        {
            // Choose which claw to grab with with right claw as default
            Gripper chosenGripper = rightGripper;
            float leftDistance = (leftGripper.position - activeGrabable.transform.position).sqrMagnitude;
            float rightDistance = (rightGripper.position - activeGrabable.transform.position).sqrMagnitude;
            if (leftDistance < rightDistance) chosenGripper = leftGripper;

            if (!chosenGripper.isActive)
            {
                chosenGripper.isActive = true;
                // Grab
                GrabHandle grabHandle = activeGrabable.GetGrabHandle(chosenGripper.position);
                StartCoroutine(GrabObject(chosenGripper, grabHandle));
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Grabable newGrabable = other.GetComponent<Grabable>();
        if (newGrabable)
        {
            foreach (Grabable grabable in mGrabables)
            {
                grabable.OnLostFocus();
            }

            mGrabables.Add(newGrabable);
            newGrabable.OnRecievedFocus();
            activeGrabable = newGrabable;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Grabable lostGrabable = other.GetComponent<Grabable>();
        if (lostGrabable)
        {
            foreach (Grabable grabable in mGrabables)
            {
                grabable.OnLostFocus();
            }

            mGrabables.Remove(lostGrabable);
            if(mGrabables.Count > 0)
            {
                mGrabables[mGrabables.Count - 1].OnRecievedFocus();
                activeGrabable = mGrabables[mGrabables.Count - 1];
            }
        }
    }

    private IEnumerator GrabObject(Gripper gripper, GrabHandle grabHandle)
    {
        float maximumOpeningClearance = 0.5f * 0.5f;
        float currentClearance = (gripper.position - grabHandle.transform.position).sqrMagnitude;
        float movingSpeed = 1.0f / Mathf.Clamp(currentClearance - maximumOpeningClearance, 0.1f, maximumOpeningClearance);
        float targetRotation = gripper.GetPerpendicularAlignmentRotation(grabHandle.transform.right);
        float targetOpenness = grabHandle.width * 1.1f; ;

        while(currentClearance > maximumOpeningClearance)
        {
            gripper.position = Vector3.Slerp(gripper.position, grabHandle.transform.position, movingSpeed * Time.deltaTime);
            gripper.rotationY = Mathf.LerpAngle(gripper.rotationY, targetRotation, movingSpeed * Time.deltaTime);
            gripper.openness = Mathf.Lerp(gripper.openness, targetOpenness, movingSpeed * Time.deltaTime);
            currentClearance = (gripper.position - grabHandle.transform.position).sqrMagnitude;
            targetRotation = gripper.GetPerpendicularAlignmentRotation(grabHandle.transform.right);
            yield return null;
        }

        while(Mathf.Abs(targetOpenness - gripper.openness) > 0.01f)
        {
            gripper.rotationY = Mathf.LerpAngle(gripper.rotationY, targetRotation, 2 * movingSpeed * Time.deltaTime);
            gripper.openness = Mathf.Lerp(gripper.openness, targetOpenness, movingSpeed * Time.deltaTime);
            targetRotation = gripper.GetPerpendicularAlignmentRotation(grabHandle.transform.right);
            yield return null;
        }


        float maximumGrabClearance = 0.01f * 0.01f;
        targetOpenness = grabHandle.width;
        movingSpeed = 1.0f / Mathf.Clamp(currentClearance - maximumOpeningClearance, 0.1f, maximumOpeningClearance);

        while (currentClearance > maximumGrabClearance)
        {
            gripper.position = Vector3.Slerp(gripper.position, grabHandle.transform.position, movingSpeed * Time.deltaTime);
            gripper.rotationY = Mathf.LerpAngle(gripper.rotationY, targetRotation, 2 * movingSpeed * Time.deltaTime);
            gripper.openness = Mathf.Lerp(gripper.openness, targetOpenness, movingSpeed * Time.deltaTime);
            currentClearance = (gripper.position - grabHandle.transform.position).sqrMagnitude;
            targetRotation = gripper.GetPerpendicularAlignmentRotation(grabHandle.transform.right);
            yield return null;
        }

        while (Mathf.Abs(targetOpenness - gripper.openness) > 0.001f)
        {
            gripper.rotationY = Mathf.LerpAngle(gripper.rotationY, targetRotation, 2 * movingSpeed * Time.deltaTime);
            gripper.openness = Mathf.Lerp(gripper.openness, targetOpenness, movingSpeed * Time.deltaTime);
            targetRotation = gripper.GetPerpendicularAlignmentRotation(grabHandle.transform.right);
            yield return null;
        }

        activeGrabable.OnGrabbed(gripper.iKTarget);

        gripper.isActive = false;
    }

    private IEnumerator IdleClawMovement()
    {
        Vector3 randomPosition = Random.insideUnitSphere * 0.2f;
        float elapsedTime = 0;
        while (true)
        {
            if(!leftGripper.isActive)
            {
                leftGripper.localPosition = Vector3.Lerp(leftGripper.localPosition, Mathf.Sin(elapsedTime) * randomPosition + leftGripper.localRestPosition, Time.deltaTime);
            }

            if (!rightGripper.isActive)
            {
                rightGripper.localPosition = Vector3.Lerp(rightGripper.localPosition, Mathf.Cos(elapsedTime) * randomPosition + rightGripper.localRestPosition, Time.deltaTime);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}