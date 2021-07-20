using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[ExecuteAlways]
public class Chasis : MonoBehaviour
{
    #region Components
    private Rigidbody mRigidBody;
    #endregion

    #region Properties
    public Vector3 position { get { return mRigidBody.position; } set { mRigidBody.MovePosition(value); } }
    public Quaternion rotation { get { return mRigidBody.rotation; } set { mRigidBody.MoveRotation(value); } }
    public bool isKinematic { get { return mRigidBody.isKinematic; } set { mRigidBody.isKinematic = value; } }
    public bool isInCollision { get; set; }
    public bool isFrontBlocked { get; set; }
    public bool isBackBlocked { get; set; }
    #endregion

    #region Inspector Parameters
    [Header("Obstacle Detection")]
    public Transform frontDetector;
    public Transform backDetector;
    public Vector3 detectorBoxSize = new Vector3(0.25f, 1.4f, 0.4f);
    #endregion

    void OnEnable()
    {
        mRigidBody = GetComponent<Rigidbody>();
    }


    private void OnCollisionEnter(Collision collision)
    {
        isInCollision = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        isInCollision = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        isInCollision = false;
    }

    private void FixedUpdate()
    {
        if (!Application.isPlaying) return;

        Collider[] frontBlocks = Physics.OverlapBox(frontDetector.position, detectorBoxSize / 2, frontDetector.rotation, Layers.AllButGroundAndCharacter);
        Collider[] backBlocks = Physics.OverlapBox(backDetector.position, detectorBoxSize / 2, backDetector.rotation, Layers.AllButGroundAndCharacter);

        isFrontBlocked = frontBlocks.Length > 0;
        isBackBlocked = backBlocks.Length > 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 0.5f, 0.35f);
        Gizmos.DrawCube(frontDetector.position, detectorBoxSize);
        Gizmos.DrawCube(backDetector.position, detectorBoxSize);
    }
}
