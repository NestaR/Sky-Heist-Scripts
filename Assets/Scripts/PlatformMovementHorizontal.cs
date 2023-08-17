using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlatformMovementHorizontal : MonoBehaviour
{

    [SerializeField] float offsetLeft = 5, offsetRight = 5, speed = 1;
    [SerializeField] bool hasReachedRight = false, hasReachedLeft = false;
    [SerializeField] private FieldOfView fieldOfView;
    Vector3 startPosition = Vector3.zero;
    public bool isTrigger = false, isBoat, isBoar, isRobot, waiting, hasPlayer, doorTrigger, boarCaught;
    public GameObject[] bounds;
    public GameObject newPlat;
    private float waitTimer, startLeft;
    public int stage = 1;
    public int numOfEnemies;
    private const string PlayerDeathZone = "PlayerDeathZone";
    private string sceneName;
    private Animator animator;
    void Awake()
    {
        startPosition = transform.position;
        startLeft = offsetLeft;
    }
    public void SetInt(string KeyName, int Value)
    {
        PlayerPrefs.SetInt(KeyName, Value);
    }
    public int GetInt(string KeyName)
    {
        return PlayerPrefs.GetInt(KeyName);
    }
    void Start()
    {
        Scene scene = SceneManager.GetActiveScene();
        sceneName = scene.name;
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        //Vector3 test = new Vector3(transform.position);
        //fieldOfView.SetOrigin(transform.position);
        if (boarCaught)
        {//Change the boar to appear caught
            animator.SetBool("Walking", false);
            animator.SetBool("BoarCaught", true);
            speed = 0;
            hasReachedRight = true;
        }
        if (numOfEnemies > 0)
        {//If there are enemies on the plat it will stop moving
            waiting = true;
        }
    }
    void LateUpdate()
    {
        PlayerPrefs.GetInt(PlayerDeathZone);
        if (PlayerPrefs.GetInt(PlayerDeathZone) == 1)
        {//When the player hits a death zone the platforms will reset
            Invoke("resetPlatform", 1f);
        }
    }
    void FixedUpdate()
    {

        if(isTrigger)
        {
            //offsetLeft = 0;
            if(isBoat)
            {//If the platform is a boat, when the player is on it will start moving and enables its bounds
                foreach (GameObject bound in bounds)
                {
                    bound.GetComponent<BoxCollider2D>().isTrigger = false;
                }
                if(hasReachedRight)
                {//Stop the boat when it reaches the end of its path
                    offsetLeft = offsetRight;
                    //isTrigger = false;
                    hasPlayer = false;
                }
            }
            else
            {
                if (hasReachedRight && !hasPlayer)
                {//When the plat reaches its destination it will reset after 3 seconds
                    Invoke("newPlatform", 3f);
                }
            }
        }
        else
        {
            if (isBoat)
            {//At the end of the boats path remove its bounds
                foreach (GameObject bound in bounds)
                {
                    bound.GetComponent<BoxCollider2D>().isTrigger = true;
                }
            }
            else if(isBoar && hasReachedRight)
            {
                offsetLeft = offsetRight;
                animator.SetBool("Walking", false);
            }
            else if(isBoar && !hasReachedRight)
            {
                animator.SetBool("Walking", true);
            }
        }
        if(!hasPlayer)
        {
            foreach (GameObject bound in bounds)
            {
                bound.GetComponent<BoxCollider2D>().isTrigger = true;
            }
        }
        if (!hasReachedRight)
        {//Move the platform until it reaches its right location
            if (transform.position.x < startPosition.x + offsetRight)
            {
                Move(offsetRight);
            }
            else if (transform.position.x >= startPosition.x + offsetRight)
            {
                hasReachedRight = true;
                hasReachedLeft = false;
                //this.transform.Rotate(0, 180, 0, Space.Self);
                if (isRobot)
                {
                    fieldOfView.SetViewDistance();
                }
            }
        }
        else if (!hasReachedLeft)
        {//Move the platform until it reaches its left location
            if (transform.position.x > startPosition.x + offsetLeft)
            {
                Move(offsetLeft);
            }
            else if (transform.position.x <= startPosition.x + offsetLeft)
            {
                hasReachedRight = false;
                hasReachedLeft = true;
                //this.transform.Rotate(0, 180, 0, Space.Self);
                if (isRobot)
                {
                    fieldOfView.SetViewDistance();
                }
            }
        }
    }

    void Move(float offset)
    {//Move the platform by a specified distance horizontally
        if (!waiting)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                                                     new Vector3(startPosition.x + offset,
                                                                 transform.position.y,
                                                                 transform.position.z),
                                                     speed * Time.deltaTime);
        }
    }
    public void newPlatform()
    {//Instantiate a new platform at its start position
        hasReachedRight = false;
        Instantiate(newPlat, startPosition, Quaternion.identity);
        Destroy(this.gameObject);
    }
    public void resetPlatform()
    {//Move the platform back to its original position
        hasReachedRight = false;
        transform.position = startPosition;
        offsetLeft = startLeft;
        if(isTrigger)
        {
            this.GetComponent<PlatformMovementHorizontal>().enabled = false;
            isTrigger = false;
        }
        PlayerPrefs.SetInt(PlayerDeathZone, 0);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if (collision.gameObject.CompareTag("Enemy"))
        //{//Check if there is an enemy on
        //    numOfEnemies += 1;
        //}
        //else if (collision.gameObject.CompareTag("Player"))
        //{//Check if the player is on
        //    hasPlayer = true;
        //}
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        //if (collision.gameObject.CompareTag("Enemy"))
        //{
        //    numOfEnemies -= 1;
        //}
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if (collision.gameObject.tag == "Lava")
        //{
        //    collision.gameObject.transform.GetChild(0).gameObject.SetActive(false);
        //}
        //else if (collision.gameObject.CompareTag("Enemy"))
        //{//Check if there is an enemy on
        //    numOfEnemies += 1;
        //}
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        //if (collision.gameObject.tag == "Lava")
        //{
        //    collision.gameObject.transform.GetChild(0).gameObject.SetActive(true);
        //}
        //else if (collision.gameObject.CompareTag("Enemy"))
        //{//Check if there is an enemy on
        //    numOfEnemies -= 1;
        //}
    }
}
