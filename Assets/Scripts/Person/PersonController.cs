using UnityEngine;

public class PersonController : MonoBehaviour
{
    public float MovementSpeed = 0.5f;
    public float MovingSmooth = 0.05f;
    public float AccelerationMultiplier = 2;
    public float RotationSpeed = 600;

    private Animator animator;
    private float currentSpeed = 0;
    private bool isAccelerated = false;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
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
        CalculateCurrentMovementSpeed();
    }

    private Vector3 GetMovementDirection()
    {
        var vertical = Input.GetAxis("Vertical");
        var horizontal = Input.GetAxis("Horizontal");
        var direction = new Vector3(horizontal, 0, vertical);
        Vector3.Normalize(direction);

        return direction;
    }

    private bool IsMoving()
    {
        return GetMovementDirection() != Vector3.zero;
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
        animator.SetBool("Walk", IsMoving());
        animator.SetBool("Run", IsMoving() && IsAccelerated());
    }
}
