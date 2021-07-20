using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GrabHandle
{
    public Transform transform;
    public float width { get; set; }
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Grabable : MonoBehaviour
{
    public Material highlightMaterial;
    private Material defaultMaterial;
    private MeshRenderer meshRenderer;
    private Rigidbody mRigidBody;
    private Transform originalParent { get; set; }
    private bool originallyIsKinematic { get; set; }
    private bool originallyUsedGravity { get; set; }
    private bool originallyFreezeRotation { get; set; }
    private bool isGrabbed { get; set; }
    public bool isInFocus { get; set; }

    public List<GrabHandle> grabHandles = new List<GrabHandle>();
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        mRigidBody = GetComponent<Rigidbody>();
        defaultMaterial = meshRenderer.material;
        originalParent = transform.parent;
        originallyIsKinematic = mRigidBody.isKinematic;
        originallyUsedGravity = mRigidBody.useGravity;
        originallyFreezeRotation = mRigidBody.freezeRotation;

        if(grabHandles.Count < 1)
        {
            Debug.LogWarning(name + ": This grabable object needs at least one grab handle. Ensure they are assigned in the inspector.");
        }
    }

    public virtual void OnRecievedFocus()
    {
        meshRenderer.material = highlightMaterial;
        isInFocus = true;
    }

    public virtual void OnLostFocus()
    {
        meshRenderer.material = defaultMaterial;
        isInFocus = false;
    }

    public virtual GrabHandle GetGrabHandle(Vector3 clawPosition)
    {
        GrabHandle grabHandle = grabHandles[0];

        float distance = (clawPosition - grabHandle.transform.position).sqrMagnitude;

        foreach(GrabHandle handle in grabHandles)
        {
            if((clawPosition - handle.transform.position).sqrMagnitude < distance)
            {
                grabHandle = handle;
                distance = (clawPosition - handle.transform.position).sqrMagnitude;
            }
        }

        Layers.SetToTempGrabable(gameObject);

        Physics.Raycast(grabHandle.transform.position + grabHandle.transform.up * 10, -grabHandle.transform.up, out RaycastHit hitUp, 15, Layers.TempGrabable);
        Physics.Raycast(grabHandle.transform.position - grabHandle.transform.up * 10, grabHandle.transform.up, out RaycastHit hitDown, 15, Layers.TempGrabable);

        grabHandle.width = (hitUp.point - hitDown.point).magnitude;

        Layers.ResetToDefault();

        return grabHandle;
    }

    public virtual void OnGrabbed(Transform grabber)
    {
        mRigidBody.useGravity = false;
        mRigidBody.freezeRotation = true;
        mRigidBody.isKinematic = true;
        transform.parent = grabber;
        isGrabbed = true;
    }

    public virtual void OnDropped()
    {
        isGrabbed = false;
        transform.parent = originalParent;
        mRigidBody.useGravity = originallyUsedGravity;
        mRigidBody.freezeRotation = originallyFreezeRotation;
        mRigidBody.isKinematic = originallyIsKinematic;
    }
}
