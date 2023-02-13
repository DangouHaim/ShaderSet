using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PersonController : MonoBehaviour
{
    public float PlayerSpeed = 0;
    public float PlayerSpeedEfficiency = 0;
    public float MovementSpeed = 0.5f;
    public float MovingSmooth = 0.05f;
    public float AccelerationMultiplier = 2;
    public float RotationSpeed = 600;
    public float PickupAnimationOffset = 0.2f;
    public float MinMovementAnimationSpeed = 0.3f;
    public bool StopMovementAnimationOnObstacles = true;
    public List<Transform> PickAbleObjects = new List<Transform>();
    public Transform CurrentPickedItem = null;

    private const string PickAbleObjectTag = "PickAbleObject";
    private const string LeftHandTag = "LeftHand";
    private const string RightHandTag = "RightHand";
    private const float ActionAnimationSpeed = 1.5f;
    private const float GravityValue = -9.81f;

    private Animator animator;
    private CharacterController controller;
    private ActionType currentAction;
    private Transform leftHand;
    private Transform rightHand;
    private Transform player;
    private Vector3 lastPlayerPosition;
    private Vector3 playerVelocity;
    private float predictedSpeed = 0;
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
        controller = gameObject.GetComponent<CharacterController>();
        leftHand = GameObject.FindGameObjectWithTag(LeftHandTag).transform;
        rightHand = GameObject.FindGameObjectWithTag(RightHandTag).transform;
        player = gameObject.transform;
        lastPlayerPosition = player.position;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        ApplyMovement();
        ApplyGravity();
        ApplyRotation();
        ApplyAnimation();
        ApplyAction();
        CalculateCurrentMovementSpeed();
        CalculateActualSpeed();
        CalculateMovementAnimationSpeed();
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

        return Vector3.Distance(rightHand.position, target) < Vector3.Distance(leftHand.position, target);
    }

    private bool IsMoving()
    {
        return GetMovementDirection() != Vector3.zero
            && !IsActionAnimationPlayed()
            && currentAction == ActionType.None;
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
        controller.Move(moveVector * predictedSpeed * Time.deltaTime);

        if (moveVector != Vector3.zero)
        {
            transform.forward = moveVector;
        }
    }

    private void ApplyGravity()
    {
        var groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        playerVelocity.y += GravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
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
            ? rightHand
            : leftHand;

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

            if (predictedSpeed < speed)
            {
                predictedSpeed += MovingSmooth;
            }

            if (predictedSpeed > speed)
            {
                predictedSpeed -= MovingSmooth;
            }
        }
        else
        {
            if (predictedSpeed > 0)
            {
                predictedSpeed -= MovingSmooth;
            }

            if (predictedSpeed < MovingSmooth)
            {
                predictedSpeed = 0;
            }
        }
    }

    private void CalculateActualSpeed()
    {
        if (predictedSpeed == 0)
        {
            PlayerSpeedEfficiency = IsActionAnimationPlayed()
                ? ActionAnimationSpeed
                : 1;
            return;
        }

        var playerPosition = player.position;
        playerPosition.y = 0;
        lastPlayerPosition.y = 0;

        PlayerSpeed = Vector3.Distance(lastPlayerPosition, playerPosition);
        PlayerSpeedEfficiency = PlayerSpeed / (predictedSpeed * Time.deltaTime);

        lastPlayerPosition = playerPosition;
    }

    private bool IsMovementAfficient()
    {
        return PlayerSpeedEfficiency > 0;
    }

    private bool IsActionAnimationPlayed()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("LiftingRight")
            || animator.GetCurrentAnimatorStateInfo(0).IsName("LiftingLeft");
    }

    private bool IsMovementAnimationPlayed()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("Walking")
            || animator.GetCurrentAnimatorStateInfo(0).IsName("Running");
    }

    private void CalculateMovementAnimationSpeed()
    {
        if (!IsMovementAfficient())
        {
            animator.speed = 1;
            return;
        }

        if (!IsMovementAnimationPlayed())
        {
            return;
        }

        animator.speed = PlayerSpeedEfficiency < MinMovementAnimationSpeed
            ? MinMovementAnimationSpeed
            : PlayerSpeedEfficiency;
    }

    private void ApplyAnimation()
    {
        var actionAvailable = !isActionCompleted && !IsActionAnimationPlayed() && predictedSpeed == 0;
        currentAnimationPlayTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

        animator.SetBool("Walk", IsMoving() && IsMovementAfficient());
        animator.SetBool("Run", IsMoving() && IsAccelerated() && IsMovementAfficient());
        animator.SetBool("PickUpRight", actionAvailable && currentAction == ActionType.PickUpRight);
        animator.SetBool("PickUpLeft", actionAvailable && currentAction == ActionType.PickUpLeft);
    }
}
