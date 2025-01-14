using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField] private Rigidbody motorRB;

    [SerializeField] private float speed = 1000f, forwardAccel = 8f, turnStrength = 180f, gravityForce = 10f, dragOnGround = 3f, powerUpTimer;

    public float speedInput, turnInput;

    private bool grounded;

    private bool increaseSpeed;

    private bool decreaseSpeed;

    private bool canPickPowerUp;

    private bool isInputInverted = false;

    [SerializeField] private int stunTime = 3;

    [SerializeField] private float safeZone = 20f;

    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float groundRayLength;
    [SerializeField] private Transform groundRayPoint;

    [SerializeField] private Transform leftFrontWheel, rightFrontWheel;
    [SerializeField] private float maxWheelTurn = 25f;
    [SerializeField] private ParticleSystem stunParticle;

    private bool _canMove = true;

    public bool CanMove
    {
        get => _canMove;
        set => _canMove = value;
    }

    void Start()
    {
        motorRB.transform.parent = null;
        canPickPowerUp = true;
    }

    void Update()
    {
        if (CanMove)
        {
            CarUpdate();
        }
    }

    private void FixedUpdate()
    {
        if (CanMove)
        {
            MovementRB();
        }
    }
    public float SafeZone => safeZone;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.red;

        Handles.DrawWireDisc(transform.position, Vector3.up, safeZone);
    }
#endif

    private void CarInput()
    {
        speedInput = 0f;

        if (Input.GetAxis("Vertical") > 0)
        {
            speedInput = Input.GetAxis("Vertical") * forwardAccel * speed;
        }
        if (isInputInverted)
        {
            turnInput = Input.GetAxis("Horizontal") * -1;
        }
        else
        {
            turnInput = Input.GetAxis("Horizontal");
        }
    }

    private void SetCarRotationWithGround()
    {
        if (grounded)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength * Time.deltaTime * Input.GetAxis("Vertical"), 0f));
        }

        leftFrontWheel.localRotation = Quaternion.Euler(leftFrontWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn) - 180, leftFrontWheel.localRotation.eulerAngles.z);
        rightFrontWheel.localRotation = Quaternion.Euler(rightFrontWheel.localRotation.eulerAngles.x, turnInput * maxWheelTurn, rightFrontWheel.localRotation.eulerAngles.z);

        Vector3 newPos = motorRB.transform.position - transform.forward * 1.5f;
        newPos.y -= 0.7f;
        transform.position = newPos;
    }

    private void MovementRB()
    {
        grounded = false;
        RaycastHit hit;

        if (Physics.Raycast(groundRayPoint.position, Vector3.down, out hit, groundRayLength, whatIsGround))
        {
            grounded = true;

            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

        }

        if (grounded)
        {
            motorRB.drag = dragOnGround;

            if (Mathf.Abs(speedInput) > 0)
            {
                motorRB.AddForce(transform.forward * speedInput);
            }
        }
        else
        {
            motorRB.drag = 0.1f;

            motorRB.AddForce(Vector3.up * -gravityForce * 100f);
        }
    }

    private void CarUpdate()
    {
        CarInput();

        SetCarRotationWithGround();

        CheckPowerUps();
    }

    public void DisableCarMovement()
    {
        CanMove = false;
    }

    public void IncreaseSpeed()
    {
        Debug.Log("increase speed");

        if (canPickPowerUp)
        {
            increaseSpeed = true;
            speed *= 1.3f;
        }
        canPickPowerUp = false;

    }
    public void DecreaseSpeed()
    {
        Debug.Log("decrease speed");
        if (canPickPowerUp)
        {
            decreaseSpeed = true;
            speed /= 1.3f;
        }
        canPickPowerUp = false;
    }

    private void CheckPowerUps()
    {
        if (increaseSpeed || decreaseSpeed)
        {
            powerUpTimer += Time.deltaTime;

            if (powerUpTimer > 10)
            {
                if (decreaseSpeed)
                {
                    speed *= 1.3f;
                    decreaseSpeed = false;
                }
                else if (increaseSpeed)
                {
                    speed /= 1.3f;
                    increaseSpeed = false;
                }
                powerUpTimer = 0f;
                canPickPowerUp = true;
            }
        }
    }

    public void ResetSpeed()
    {
        speedInput = 0;
        motorRB.velocity = Vector3.zero;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public IEnumerator Stun()
    {
        ResetSpeed();
        stunParticle.Play();
        Debug.Log("play");
        CanMove = false;
        yield return new WaitForSeconds(stunTime);
        stunParticle.Stop();
        CanMove = true;

    }
    public void StartStun()
    {
        StartCoroutine(Stun());
    }

    private IEnumerator InvertInput()
    {
        isInputInverted = true;
        yield return new WaitForSeconds(10);
        isInputInverted = false;
    }

    public void StartInvertInput()
    {
        StartCoroutine(InvertInput());
    }

}
