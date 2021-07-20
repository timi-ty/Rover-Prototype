using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{

    #region Inspector Parameters

    [Header("Movement Settings")]
    public float maxMoveSpeed;
    public float maxTurnSpeed;
    public float maxBlockedSpeed;
    [Range(0.1f, 8f)]
    public float accelerationTime;
    [Range(0.1f, 8f)]
    public float decelerationTime;
    public InvertibleCurve speedUpCurve;
    public InvertibleCurve slowDownCurve;

    #region Engine Parts
    [Header("Engine Parts")]
    public Transform axle;
    public Rigidbody leftWheel;
    public Rigidbody rightWheel;
    public Rigidbody wheelShaft;
    #endregion

    #region Chasis
    [Header("Chasis")]
    public Chasis mainChasis;
    public Transform leftTyre;
    public Transform rightTyre;
    [Header("Chasis Config")]
    public bool maintainTyreOrientation;
    public float maxAbberation;
    #endregion

    #endregion

    #region Internal
    internal float wheelRadius;
    internal float leftTyreAngle;
    internal float rightTyreAngle;
    internal float targetMoveSpeed;
    internal float targetTurnSpeed;
    internal float moveSpeed;
    internal float turnSpeed;
    internal Vector3 bodyOffset;
    internal Quaternion leftTyreOrientation;
    internal Quaternion rightTyreOrientation;
    internal Collider leftWheelCollider;
    internal Collider rightWheelCollider;
    internal AudioSource leftMotorAudio;
    internal AudioSource rightMotorAudio;
    #endregion

    private void Start()
    {
        speedUpCurve.Init();
        slowDownCurve.Init();

        leftWheelCollider = leftWheel.GetComponent<Collider>();
        rightWheelCollider = rightWheel.GetComponent<Collider>();

        leftMotorAudio = leftWheel.GetComponent<AudioSource>();
        rightMotorAudio = rightWheel.GetComponent<AudioSource>();

        wheelRadius = leftWheel.GetComponent<SphereCollider>().radius;

        bodyOffset = mainChasis.position - (leftTyre.position + rightTyre.position) / 2;
        leftTyreOrientation = leftTyre.rotation;
        rightTyreOrientation = rightTyre.rotation;

        leftMotorAudio.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
        rightMotorAudio.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;

        mainChasis.transform.parent = axle;
    }

    private void FixedUpdate()
    {
        moveSpeed = SpeedControl(moveSpeed, targetMoveSpeed, maxMoveSpeed);
        turnSpeed = SpeedControl(turnSpeed, targetTurnSpeed, maxTurnSpeed);
        WheelControl();
        AxleControl();
        ChasisControl();
    }

    public void ParseInput(float horizontalInput, float verticalInput, Vector3 targetForwardDir)
    {
        Vector2 inputVector = new Vector2(horizontalInput, verticalInput);

        Vector3 currentForwardDir = Vector3.ProjectOnPlane(axle.forward, Vector3.up);

        float facingAngle = Vector3.SignedAngle(targetForwardDir, currentForwardDir, Vector3.up);
        float inputAngle = Vector2.SignedAngle(inputVector, Vector2.up);

        targetMoveSpeed = Mathf.Cos((inputAngle - facingAngle) * Mathf.Deg2Rad) * maxMoveSpeed * inputVector.magnitude;
        targetTurnSpeed = Mathf.Sin((inputAngle - facingAngle) * Mathf.Deg2Rad) * maxTurnSpeed * inputVector.magnitude;
    }

    private float SpeedControl(float currentSpeed, float targetSpeed, float maxSpeed)
    {
        float normalizedSpeed = Mathf.Abs(currentSpeed) / maxSpeed;

        accelerationTime = Mathf.Clamp(accelerationTime, 0.1f, 8.0f);
        decelerationTime = Mathf.Clamp(decelerationTime, 0.1f, 8.0f);

        bool isChangingDirection = (targetSpeed * currentSpeed < 0);
        bool isSpeedingUp = Mathf.Abs(targetSpeed) > Mathf.Abs(currentSpeed) && !isChangingDirection;

        if (isSpeedingUp)
        {
            float currentTime = speedUpCurve.InverseEvaluate(normalizedSpeed) + Time.fixedDeltaTime / accelerationTime;

            int direction = targetSpeed < 0 ? -1 : 1;

            currentSpeed = Mathf.Clamp(speedUpCurve.Evaluate(currentTime) * maxSpeed, 0, Mathf.Abs(targetSpeed)) * direction;
        }
        else
        {
            float currentTime = slowDownCurve.InverseEvaluate(normalizedSpeed) + Time.fixedDeltaTime / decelerationTime;

            int direction = currentSpeed < 0 ? -1 : 1;

            currentSpeed = Mathf.Clamp(slowDownCurve.Evaluate(currentTime) * maxSpeed, isChangingDirection ? 0 : Mathf.Abs(targetSpeed), maxSpeed) * direction;
        }

        return currentSpeed;
    }

    private void WheelControl()
    {
        Vector3 leftDefaultVelocity = leftWheel.velocity;
        Vector3 rightDefaultVelocity = rightWheel.velocity;

        float leftSpeed = moveSpeed + turnSpeed;
        float rightSpeed = moveSpeed - turnSpeed;

        if (moveSpeed != 0 || turnSpeed != 0)
        {
            leftWheel.freezeRotation = false;
            rightWheel.freezeRotation = false;

            if (mainChasis.isInCollision)
            {
                //The four "if" statements below ensure that wheels do not forcibly move in a blocked direction
                if (mainChasis.isFrontBlocked && leftSpeed > 0) leftSpeed = Mathf.Min(leftSpeed, maxBlockedSpeed);
                if (mainChasis.isBackBlocked && leftSpeed < 0) leftSpeed = Mathf.Max(leftSpeed, -maxBlockedSpeed);
                if (mainChasis.isFrontBlocked && rightSpeed > 0) rightSpeed = Mathf.Min(rightSpeed, maxBlockedSpeed);
                if (mainChasis.isBackBlocked && rightSpeed < 0) rightSpeed = Mathf.Max(leftSpeed, -maxBlockedSpeed);
            }

            Vector3 leftVelocity = leftSpeed * axle.forward;
            Vector3 rightVelocity = rightSpeed * axle.forward;

            leftVelocity = new Vector3(leftVelocity.x, leftDefaultVelocity.y, leftVelocity.z);
            rightVelocity = new Vector3(rightVelocity.x, rightDefaultVelocity.y, rightVelocity.z);

            leftWheel.velocity = leftVelocity;
            rightWheel.velocity = rightVelocity;
        }
        else
        {
            leftWheel.freezeRotation = true;
            rightWheel.freezeRotation = true;
        }

        float leftRemap = Mathf.Abs(leftSpeed / (maxMoveSpeed + maxTurnSpeed));
        float rightRemap = Mathf.Abs(rightSpeed / (maxMoveSpeed + maxTurnSpeed));

        leftMotorAudio.pitch = 0.5f + leftRemap * 0.5f;
        rightMotorAudio.pitch = 0.5f + rightRemap * 0.5f;

        leftMotorAudio.volume = leftRemap * 0.5f;
        rightMotorAudio.volume = rightRemap * 0.5f;
    }

    private void AxleControl()
    {
        Vector3 leftWheelPosition = leftWheel.position;
        Vector3 rightWheelPosition = rightWheel.position;

        axle.position = (leftWheelPosition + rightWheelPosition) / 2.0f;

        wheelShaft.MovePosition(axle.position);

        Plane referencePlane = new Plane(rightWheelPosition, axle.position + Vector3.up, leftWheelPosition);

        Vector3 forward = referencePlane.normal.normalized;
        Vector3 right = (rightWheelPosition - leftWheelPosition).normalized;
        Vector3 upward = Vector3.Cross(forward, right).normalized;

        axle.rotation = Quaternion.LookRotation(forward, upward);
    }

    private void ChasisControl()
    {
        Vector3 lean = mainChasis.transform.InverseTransformDirection(Vector3.Project((leftWheel.velocity + rightWheel.velocity)/2, mainChasis.transform.forward));
        float nextTilt = Mathf.Clamp(lean.z * 3, -10, 10);
        Vector3 localEulers = mainChasis.transform.localEulerAngles;
        localEulers.x = Mathf.LerpAngle(localEulers.x, nextTilt, Time.fixedDeltaTime * 6);
        mainChasis.transform.localEulerAngles = localEulers;

        leftTyre.position = leftWheelCollider.bounds.center;
        rightTyre.position = rightWheelCollider.bounds.center;

        float deltaDistanceLeft = (moveSpeed + turnSpeed) * Time.fixedDeltaTime;
        float deltaDistanceRight = (moveSpeed - turnSpeed) * Time.fixedDeltaTime;

        float tyrePerimeter = 2 * Mathf.PI * wheelRadius;

        float deltaAngleLeft = (deltaDistanceLeft / tyrePerimeter) * 360;
        float deltaAngleRight = (deltaDistanceRight / tyrePerimeter) * 360;

        leftTyreAngle += deltaAngleLeft;
        rightTyreAngle += deltaAngleRight;

        leftTyreAngle %= 360;
        rightTyreAngle %= 360;

        Plane referencePlane = new Plane(leftTyre.position, (leftTyre.position + rightTyre.position) / 2 + Vector3.up, rightTyre.position);

        Vector3 forward = referencePlane.normal;
        Vector3 right = (rightTyre.position - leftTyre.position).normalized;
        Vector3 upward = Vector3.Cross(forward, right);

        Quaternion commonTyreRotation = Quaternion.LookRotation(forward, upward);

        Quaternion leftTyreRotation = Quaternion.AngleAxis(leftTyreAngle, right) * commonTyreRotation;
        Quaternion rightTyreRotation = Quaternion.AngleAxis(rightTyreAngle, right) * commonTyreRotation;

        leftTyre.rotation = leftTyreRotation * (maintainTyreOrientation ? leftTyreOrientation : Quaternion.identity);
        rightTyre.rotation = rightTyreRotation * (maintainTyreOrientation ? rightTyreOrientation : Quaternion.identity);


        //System to ensure that the chasis does not sway too far off from the axle position
        if ((mainChasis.position - (axle.position + bodyOffset )).sqrMagnitude 
            > (maxAbberation * maxAbberation) && !mainChasis.isInCollision)
        {
            StartCoroutine(ForceCorrectChasisPosition());
        }
    }

    private IEnumerator ForceCorrectChasisPosition()
    {
        mainChasis.isKinematic = true;
        yield return null;
        mainChasis.isKinematic = false;
    }
}