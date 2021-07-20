using UnityEngine;
using UnityEngine.InputSystem;

public class RoverInput : MonoBehaviour
{
    #region Variables       

    private Vector2 moveInput;
    private Vector2 rotateCameraInput;

    [HideInInspector] public Engine roverEngine;
    [HideInInspector] public InteractionController roverInteraction;
    [HideInInspector] public CameraController tpCamera;

    private Vector3 screenCentre { get { return new Vector2(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2); } }

    #endregion

    private const string MainCameraTagMissing = "The third person camera is not tagged as the main camera.";

    protected virtual void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        InitilizeEngine();
        InitilizeInteractionController();
        InitializeCamera();
    }

    protected virtual void Update()
    {
        UpdateCamera();
        UpdateMovement();
    }

    #region Basic Locomotion Inputs

    protected virtual void InitializeCamera()
    {
        if (tpCamera == null)
        {
            tpCamera = FindObjectOfType<CameraController>();
            if (tpCamera == null)
                return;
            if (tpCamera)
            {
                if (!tpCamera.CompareTag("MainCamera")) DebugLog(MainCameraTagMissing);

                tpCamera.Init();
            }
        }
    }

    protected virtual void InitilizeEngine()
    {
        roverEngine = GetComponentInChildren<Engine>();
    }

    protected virtual void InitilizeInteractionController()
    {
        roverInteraction = GetComponentInChildren<InteractionController>();
    }

    public void OnLook(InputAction.CallbackContext lookCallback)
    {
        rotateCameraInput = lookCallback.ReadValue<Vector2>();
    }

    public void OnMove(InputAction.CallbackContext moveCallback)
    {
        moveInput = moveCallback.ReadValue<Vector2>();
    }

    protected virtual void UpdateCamera()
    {
        if (tpCamera == null)
            return;

        tpCamera.RotateCamera(rotateCameraInput.x, rotateCameraInput.y);
    }

    protected virtual void UpdateMovement()
    {
        Vector3 planarCameraForward = Vector3.ProjectOnPlane(tpCamera.transform.forward, Vector3.up);

        roverEngine.ParseInput(moveInput.x, moveInput.y, planarCameraForward);
    }

    #endregion

    #region Interaction Inputs

    public virtual void OnGrab(InputAction.CallbackContext moveCallback)
    {
        roverInteraction.Grab();
    }

    #endregion

    private void DebugLog(string message)
    {
        Debug.Log(this.ToString() + message);
    }
}