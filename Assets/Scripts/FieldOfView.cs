using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    private Mesh mesh;
    [SerializeField]
    private LayerMask layerMask;
    [SerializeField] private GameObject robot;
    public Vector3 origin;
    public Transform transformTarger;
    public Vector3 targetPosition;
    public float fov, viewDistance, laserAngle;

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
    private bool move = false, vertical = false, rotate = false;

    [SerializeField]
    [Tooltip("The speed at which to rotate the object.")]
    private float rotateSpeed = 5f;

    public bool movingRight = true;
    public bool playerHit = false;
    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        //origin = Vector3.zero;
        leftPosition.y = origin.y;
        rightPosition.y = origin.y;
        //targetPosition = trans.position;
    }
    private void LateUpdate()
    {
        int rayCount = 30;
        float angle = laserAngle;
        float angleIncrease = fov / rayCount;

        Vector3[] vertices = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = origin;

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex;
            RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, GetVectorFromAngle(angle), viewDistance, layerMask);
            if(raycastHit2D.collider != null && raycastHit2D.collider.tag == "Player")
            {
                playerHit = true;
                GameObject.Find("Player").GetComponent<BasicMovement>().isHit = true;
            }
            else
            {
                playerHit = false;
            }
            if (raycastHit2D.collider == null)
            {
                vertex = origin + GetVectorFromAngle(angle) * viewDistance;
            }
            else
            {
                vertex = raycastHit2D.point;
            }
            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }

            vertexIndex++;
            angle -= angleIncrease;
        }

        if (move && movingRight)
        {
            origin = new Vector3(origin.x + moveSpeed, origin.y, origin.z);
            if (robot != null)
            {
                robot.transform.position = new Vector3(robot.transform.position.x + moveSpeed, robot.transform.position.y, robot.transform.position.z);
            }
            if (Vector3.Distance(origin, rightPosition) < 0.1f)
            {
                movingRight = false;
                if(vertical)
                {
                    viewDistance = -viewDistance;
                    Vector3 scale = robot.transform.localScale;
                    scale.x *= -1;
                    robot.transform.localScale = scale;
                }
            }
        }
        else if (move && !movingRight)
        {
            origin = new Vector3(origin.x - moveSpeed, origin.y, origin.z);
            if(robot != null)
            {
                robot.transform.position = new Vector3(robot.transform.position.x - moveSpeed, robot.transform.position.y, robot.transform.position.z);
            }
            if (Vector3.Distance(origin, leftPosition) < 0.1f)
            {
                movingRight = true;
                if (vertical)
                {
                    viewDistance = -viewDistance;
                    Vector3 scale = robot.transform.localScale;
                    scale.x *= -1;
                    robot.transform.localScale = scale;
                }
            }
        }

        if (rotate)
        {
            targetPosition = transformTarger.position;
            Vector3 aimDir = (targetPosition - transform.position).normalized;
            SetAimDirection(aimDir);
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();


    }
    public void SetAimDirection(Vector3 aimDirection)
    {
        laserAngle = GetAngleFromVectorFloat(aimDirection) - fov;
    }
    public void SetOrigin(Vector3 origin)
    {
        this.origin = origin;
    }
    public void SetViewDistance()
    {
        this.viewDistance = -viewDistance;
    }
    private void OnDrawGizmos()
    {
        if(mesh)
        {
            //Gizmos.DrawMesh(mesh, transform.position, transform.rotation);
        }
    }
    public static Vector3 GetVectorFromAngle(float angle)
    {
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }
    public static float GetAngleFromVectorFloat(Vector3 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;
        return n;
    }
}
