using PathCreation;
using UnityEngine;

public class PathFollower : MonoBehaviour
{

    public PathCreator pathCreator;
    public float speed = 1.5f;
    private float distanceTravelled;
    private Vector3 eulers;

    void Start()
    {
        eulers = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.x + 0f);
    }


    // Update is called once per frame
    void Update()
    {
        distanceTravelled += speed * Time.deltaTime;
        transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled);
        transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled);
        transform.eulerAngles = eulers;
    }
}
