using UnityEngine;
using UnityEngine.UI;

public class BasicMovement : MonoBehaviour
{
    [SerializeField] private LayerMask _platformLayer;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _climbingLayer;
    [SerializeField] private LayerMask _crouchLayer;
    [SerializeField] private Transform _groundCheckPoint;
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.7f, 0.03f);
    [SerializeField] private Transform _headCheckPoint;
    [SerializeField] private Vector2 _headCheckSize = new Vector2(0.7f, 0.03f);
    [SerializeField, Tooltip("Movement speed")]
    private float moveSpeed = 5f;

    [SerializeField, Tooltip("Jump force")]
    private float jumpForce = 10f;

    [SerializeField, Tooltip("Crouch scale")]
    private float crouchScale = 1f;

    [SerializeField, Tooltip("Ground check distance")]
    private float groundCheckDistance = 1f;

    [SerializeField, Tooltip("Coyote timer duration")]
    private float coyoteTimerDuration = 0.2f;

    [SerializeField, Tooltip("Grappling hook speed")]
    private float grapplingHookSpeed = 10f;

    [SerializeField, Tooltip("Grappling hook tag")]
    private string grapplingHookTag = "GrapplingHook";

    [SerializeField, Tooltip("Projectile prefab")]
    private GameObject projectilePrefab;

    [SerializeField, Tooltip("Projectile speed")]
    private float projectileSpeed = 10f;

    [SerializeField, Tooltip("Projectile layer")]
    private LayerMask projectileLayer;

    [SerializeField, Tooltip("Acceleration value")]
    private float acceleration = 10f;

    [SerializeField, Tooltip("Decceleration value")]
    private float decceleration = 10f;

    private Rigidbody2D rb;
    GameObject grapple;
    //Circle circle;
    public bool isGrounded = false, isJumping = false;
    private bool isCrouching = false;
    public bool isCramped = false, isHit = false, firing = false, hooked = false;
    private float coyoteTimer = 0f;
    public float grappleTimer = 0f, horizontalInput = 0;
    public Vector2 grapplePosition, mousePosWorld, mousePosView, respawnPosition;
    int gemCounter;
    public Text gemAmount;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (firing)
        {
            grappleTimer += Time.deltaTime;
            grapple = GameObject.FindWithTag("Grapple");

        }
        if (horizontalInput != 0)
        {
            if (!firing)
            {
                delGrapple();
            }
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            
        }
        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded || coyoteTimer > 0f) && !isCramped && !isJumping)
        {
            isGrounded = false;
            isJumping = true;
            coyoteTimer = 0f;
            resetPlayerBody();
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            delGrapple();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            delGrapple();
            if (!isCrouching)
            {
                transform.localScale = new Vector3(transform.localScale.x, crouchScale, transform.localScale.z);
                isCrouching = true;
                rb.mass = 1.8f;
            }
            else if (isCrouching && !isCramped)
            {
                transform.localScale = new Vector3(transform.localScale.x, 2f, transform.localScale.z);
                isCrouching = false;
                rb.mass = 1.2f;
            }
        }

        if (!isGrounded)
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Fire1") && !firing)
        {
            delGrapple();
            resetPlayerBody();
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            Vector3 direction = (mousePos - transform.position).normalized;
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);            
            projectile.GetComponent<Rigidbody2D>().velocity = direction * projectileSpeed;
            projectile.layer = projectileLayer;
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), projectile.GetComponent<Collider2D>());
            firing = true;
        }
        if (grapple != null && grapple.GetComponent<Circle>().collided && !hooked)
        {
            transform.position = Vector2.Lerp(transform.position, grapple.GetComponent<Circle>().freezePosition, Time.deltaTime * 4);
        }
        if (grapple != null && Vector3.Distance(grapple.GetComponent<Circle>().freezePosition,transform.position) < 0.7f)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            hooked = true;
        }
        if (grappleTimer > 0.4f)
        {                
            firing = false;
            grappleTimer = 0f;
        }
    }
    void LateUpdate()
    {
        if(isHit)
        {
            transform.position = respawnPosition;
            isHit = false;
        }
    }
    private void Run()
    {

        //Calculate the direction we want to move in and our desired velocity
        float targetSpeed = horizontalInput * moveSpeed;
        #region Calculate AccelRate
        float accelRate;

        //Gets an acceleration value based on if we are accelerating (includes turning) 
        //or trying to decelerate (stop). As well as applying a multiplier if we're air borne.
        if (isGrounded)
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;
        else
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;

        #endregion

        //Not used since no jump implemented here, but may be useful if you plan to implement your own

        #region Add Bonus Jump Apex Acceleration
        //Increase are acceleration and maxSpeed when at the apex of their jump, makes the jump feel a bit more bouncy, responsive and natural
        //if ((isJumping) && Mathf.Abs(rb.velocity.y) < jumpHangTimeThreshold)
        //{
        //    accelRate *= jumpHangAccelerationMult;
        //    targetSpeed *= jumpHangMaxSpeedMult;
        //    //targetSpeed2 *= jumpHangMaxSpeedMult;
        //}
        #endregion


        #region Conserve Momentum
        //We won't slow the player down if they are moving in their desired direction but at a greater speed than their maxSpeed
        //if (doConserveMomentum && Mathf.Abs(_rigidbody.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(_rigidbody.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
        //{
        //    //Prevent any deceleration from happening, or in other words conserve are current momentum
        //    //You could experiment with allowing for the player to slightly increase their speed whilst in this "state"
        //    accelRate = 50;
        //}
        #endregion

        //Calculate difference between current velocity and desired velocity
        float speedDif = targetSpeed - rb.velocity.x;
        //Calculate force along x-axis to apply to thr player

        float movement = speedDif * accelRate;
        //Convert this to a vector and apply to rigidbody
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);




        /*
		 * For those interested here is what AddForce() will do
		 * RB.velocity = new Vector2(RB.velocity.x + (Time.fixedDeltaTime  * speedDif * accelRate) / RB.mass, RB.velocity.y);
		 * Time.fixedDeltaTime is by default in Unity 0.02 seconds equal to 50 FixedUpdate() calls per second
		*/
    }
    public void delGrapple()
    {
        hooked = false;
        if (grapple != null)
        {
            
            Destroy(grapple.gameObject);
        }
    }
    public void resetPlayerBody()
    {
        transform.localScale = new Vector3(transform.localScale.x, 2f, transform.localScale.z);
        isCrouching = false;
        rb.mass = 1.2f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Checkpoint"))
        {
            respawnPosition = collision.transform.position;
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Laser"))
        {
            Destroy(GameObject.FindWithTag("Grapple"));
            transform.position = respawnPosition;
        }
        else if (collision.gameObject.CompareTag("Gem"))
        {
            gemCounter += 1;
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Hidden"))
        {
            collision.gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
            for (int i = 0; i < collision.gameObject.transform.childCount; i++)
            {
                if (collision.gameObject.transform.GetChild(i) != null)
                {
                    collision.gameObject.transform.GetChild(i).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
                }
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Hidden") && transform.parent == null)
        {
            collision.gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
            for(int i = 0; i < collision.gameObject.transform.childCount; i++)
            {
                if (collision.gameObject.transform.GetChild(i) != null)
                {
                    collision.gameObject.transform.GetChild(i).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
                }
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
        Gizmos.DrawRay(transform.position, grapplePosition);
        if (isCrouching)
        {
            Gizmos.DrawWireCube(_headCheckPoint.position, _headCheckSize);
        }
    }

    void FixedUpdate()
    {
        Run();
        if ((Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer)) || (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _platformLayer)) || (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _climbingLayer)) || (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _crouchLayer)))
        {//If the player has just left the ground they are still "grounded" for a time allowing smoother jumping at ledges
            isGrounded = true;
            coyoteTimer = coyoteTimerDuration;
            isJumping = false;
        }
        else
        {
            isGrounded = false;
        }
        if ((Physics2D.OverlapBox(_headCheckPoint.position, _headCheckSize, 0, _crouchLayer)))
        {//If the player has just left the ground they are still "grounded" for a time allowing smoother jumping at ledges
            isCramped = true;
        }
        else
        {
            isCramped = false;
        }
        gemAmount.text = gemCounter.ToString();
    }
}