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
    public float PickupAnimationOffset = 0.2f;
    public List<Transform> PickAbleObjects = new List<Transform>();
    public Transform CurrentPickedItem = null;

    private const string PickAbleObjectTag = "PickAbleObject";
    private const string LeftHandTag = "LeftHand";
    private const string RightHandTag = "RightHand";

    private Animator animator;
    private CharacterController controller;
    private ActionType currentAction;
    private Transform LeftHand;
    private Transform RightHand;
    private float currentSpeed = 0;
    private float currentAnimationPlayTime = 0;
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
        controller = gameObject.AddComponent<CharacterController>();
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
        if (other.gameObject.tag == PickAbleObjectTag)
        {
            if (!PickAbleObjects.Contains(other.gameObject.transform))
            {
                PickAbleObjects.Add(other.gameObject.transform);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == PickAbleObjectTag)
        {
            if (PickAbleObjects.Contains(other.gameObject.transform))
            {
                PickAbleObjects.Remove(other.gameObject.transform);
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
        var target = PickAbleObjects.First().position;

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
        var moveVector = transform.forward;
        controller.Move(moveVector * currentSpeed * Time.deltaTime);

        if (moveVector != Vector3.zero)
        {
            transform.forward = moveVector;
        }
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

        if (Input.GetKeyUp(KeyCode.E) && PickAbleObjects.Any() && currentAction == ActionType.None)
        {
            currentAction = IsRightHandPickUp()
                ? ActionType.PickUpRight
                : ActionType.PickUpLeft;
            
            if (CurrentPickedItem == null)
            {
                CurrentPickedItem = PickAbleObjects.First();
                PickAbleObjects.Remove(CurrentPickedItem);
            }
        }

        RunActions();
    }

    private void RunActions()
    {
        PickupItem();
    }

    private void PickupItem()
    {
        if (CurrentPickedItem == null
            || !IsActionAnimationPlayed()
            || currentAnimationPlayTime < PickupAnimationOffset)
        {
            return;
        }

        CurrentPickedItem.parent = currentAction == ActionType.PickUpRight
            ? RightHand
            : LeftHand;

        CurrentPickedItem.localPosition = Vector3.up * 0.15f + Vector3.forward * 0.05f;
        CurrentPickedItem = null;
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
        currentAnimationPlayTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

        animator.SetBool("Walk", IsMoving());
        animator.SetBool("Run", IsMoving() && IsAccelerated());
        animator.SetBool("PickUpRight", actionAvailable && currentAction == ActionType.PickUpRight);
        animator.SetBool("PickUpLeft", actionAvailable && currentAction == ActionType.PickUpLeft);
    }
}
