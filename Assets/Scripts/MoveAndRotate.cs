using UnityEngine;

public class MoveAndRotate : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The left position to move to.")]
    private Vector3 leftPosition;

    [SerializeField]
    [Tooltip("The right position to move to.")]
    private Vector3 rightPosition;

    [SerializeField]
    [Tooltip("The speed at which to move the object.")]
    private float moveSpeed = 5f;

    [SerializeField]
    [Tooltip("Whether or not to rotate the object.")]
    private bool move = false, rotate = false;

    [SerializeField]
    [Tooltip("The speed at which to rotate the object.")]
    private float rotateSpeed = 5f;

    private bool movingRight = true;
    void Start()
    {
        leftPosition.y = transform.position.y;
        rightPosition.y = transform.position.y;
    }
    private void Update()
    {

        if (move && movingRight)
        {
            transform.position = Vector3.MoveTowards(transform.position, rightPosition, moveSpeed);

            if (Vector3.Distance(transform.position, rightPosition) < 0.1f)
            {
                movingRight = false;
            }
        }
        else if(move && !movingRight)
        {
            transform.position = Vector3.MoveTowards(transform.position, leftPosition, moveSpeed);

            if (Vector3.Distance(transform.position, leftPosition) < 0.1f)
            {
                movingRight = true;
            }
        }

        if (rotate)
        {
            transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
        }
    }
}