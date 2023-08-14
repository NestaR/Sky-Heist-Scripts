using UnityEngine;

public class Circle : MonoBehaviour
{
    [Tooltip("The position to freeze at when colliding with the Hookspot tag.")]
    public Vector3 freezePosition;

    public bool collided = false;
    private float collisionTime = 0f;
    public float airTime = 0f, airLimit;
    private void Update()
    {
        airTime += Time.deltaTime;
        if ((collided && Time.time - collisionTime >= 3f) || (!collided && airTime > airLimit))
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Hookspot"))
        {
            collisionTime = Time.time;
            collided = true;
            Invoke("FreezePosition", 0f);
        }
        else if (collision.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }

    private void FreezePosition()
    {
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        freezePosition = transform.position;       
    }
}