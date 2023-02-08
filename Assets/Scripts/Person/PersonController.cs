using System.Net.Http.Headers;
using System.Security.AccessControl;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PersonController : MonoBehaviour
{
    public float MovementSpeed = 0.5f;
    public float MovingSmooth = 0.05f;
    public float AccelerationMultiplier = 2;
    public float RotationSpeed = 600;
    public List<Transform> PickableObjects = new List<Transform>();

    private const string PickableObjectTag = "PickableObject";
    private const string LeftHandTag = "LeftHand";
    private const string RightHandTag = "RightHand";

    private Animator animator;
    private ActionType currentAction;
    private Transform LeftHand;
    private Transform RightHand;
    private float currentSpeed = 0;
    private bool isAccelerated = false;
    private bool isActionCompleted = false;

    private enum ActionType
    {
        None,
        PickUpRight,
        PickUpLeft,
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        LeftHand = GameObject.FindGameObjectWithTag(LeftHandTag).transform;
        RightHand = GameObject.FindGameObjectWithTag(RightHandTag).transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        ApplyMovement();
        ApplyRotation();
        ApplyAnimation();
        ApplyAction();
        CalculateCurrentMovementSpeed();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == PickableObjectTag)
        {
            if (!PickableObjects.Contains(other.gameObject.transform))
            {
                PickableObjects.Add(other.gameObject.transform);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == PickableObjectTag)
        {
            if (PickableObjects.Contains(other.gameObject.transform))
            {
                PickableObjects.Remove(other.gameObject.transform);
            }
        }
    }

    private Vector3 GetMovementDirection()
    {
        var vertical = Input.GetAxis("Vertical");
        var horizontal = Input.GetAxis("Horizontal");
        var direction = new Vector3(horizontal, 0, vertical);
        Vector3.Normalize(direction);

        return direction;
    }

    private bool IsRightHandPickUp()
    {
        var target = PickableObjects.First().position;

        return Vector3.Distance(RightHand.position, target) < Vector3.Distance(LeftHand.position, target);
    }

    private bool IsMoving()
    {
        return GetMovementDirection() != Vector3.zero
            && !IsActionAnimationPlayed()
            && currentAction == ActionType.None;
    }

    private bool IsActionAnimationPlayed()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("LiftingRight")
            || animator.GetCurrentAnimatorStateInfo(0).IsName("LiftingLeft");
    }

    private void ApplyRotation()
    {
        var direction = GetMovementDirection();

        if (IsMoving())
        {
            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, RotationSpeed * Time.deltaTime);
        }
    }

    private void ApplyMovement()
    {
        transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);
    }

    private void ApplyAction()
    {
        if (IsActionAnimationPlayed())
        {
            isActionCompleted = true;
        }
        else if (isActionCompleted)
        {
            currentAction = ActionType.None;
            isActionCompleted = false;
        }

        if (Input.GetKeyUp(KeyCode.E) && PickableObjects.Any())
        {
            currentAction = IsRightHandPickUp()
                ? ActionType.PickUpRight
                : ActionType.PickUpLeft;
        }
    }

    private bool IsAccelerated()
    {
        if (!IsMoving())
        {
            isAccelerated = false;
            return isAccelerated;
        }

        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            isAccelerated = true;
        }

        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            isAccelerated = false;
        }

        return isAccelerated;
    }

    private void CalculateCurrentMovementSpeed()
    {
        if (IsMoving())
        {
            var speed = IsAccelerated()
                ? MovementSpeed * AccelerationMultiplier
                : MovementSpeed;

            if (currentSpeed < speed)
            {
                currentSpeed += MovingSmooth;
            }

            if (currentSpeed > speed)
            {
                currentSpeed -= MovingSmooth;
            }
        }
        else
        {
            if (currentSpeed > 0)
            {
                currentSpeed -= MovingSmooth;
            }

            if (currentSpeed < MovingSmooth)
            {
                currentSpeed = 0;
            }
        }
    }

    private void ApplyAnimation()
    {
        var actionAvailable = !isActionCompleted && !IsActionAnimationPlayed() && currentSpeed == 0;

        animator.SetBool("Walk", IsMoving());
        animator.SetBool("Run", IsMoving() && IsAccelerated());
        animator.SetBool("PickUpRight", actionAvailable && currentAction == ActionType.PickUpRight);
        animator.SetBool("PickUpLeft", actionAvailable && currentAction == ActionType.PickUpLeft);
    }
}
