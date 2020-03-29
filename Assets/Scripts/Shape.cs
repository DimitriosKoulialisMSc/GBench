using UnityEngine;
using UnityEngine.SceneManagement;


public class Shape : MonoBehaviour
{
    #region Variables
    public bool IsActive { set; get; }

    // Variables related to minishape generation for explosion method
    private GameObject miniShape; // The actual mini shape
    private string shapeId; // The identification for the minishape e.g. if we explore a sphere we want minispheres... this var holds it for us.
    private Material materialId; // Likewise for material
    private const float miniShapesSize = 0.1f; 
    private int miniShapesInRow; // The quantifier
    private int incrementMiniShapesInRow;
    private Color black;

    // Variables related to explosion effect
    private const float explosionRadius = 2f;
    private const float explosionForce = 10f;
    private const float explosionUpward = 0.3f;
    int totalMiniShapes;
    float miniShapesPivotDistance;
    Vector3 miniShapesPivot;

    private static Vector3 cameraMovementChecker;
    private float shapeProjectedOffsetZ;




    private float verticalVelocity;
    private bool isNotCollided;
    private float speed;
    private const float GRAVITY = 2.0f;
    #endregion

    private void Start()
    {
        // Giving a differnt color for the misishapes.
        black = Color.gray;
        // Objects start us untouched
        isNotCollided = false;
        // Initial exploding items
        miniShapesInRow = 2 + incrementMiniShapesInRow;
        // Initialize & Reset the holder of miniShapesInRow with its increment
        totalMiniShapes = 0;
        incrementMiniShapesInRow = 0;
        // Making a random pivot distance to keep the explosions interesting. 
        int randomizePivot = (int)Random.Range(2f, 10f);
        //Calculating pivot distance
        miniShapesPivotDistance = miniShapesSize * miniShapesInRow / randomizePivot;
        // Creating a pivot vector
        miniShapesPivot = new Vector3(miniShapesPivotDistance, miniShapesPivotDistance, miniShapesPivotDistance);



    }
    // Update is called once per frame
    private void Update()
    {
        if (!IsActive)
            return;
        verticalVelocity -= GRAVITY * Time.deltaTime;  //Giving a form of gravity.
        transform.position += new Vector3(speed, verticalVelocity, 0) * Time.deltaTime;


            if (transform.position.y <= -1f) // The shape is out of view if true.
            {
                IsActive = false; //Terminate the update for that particular shape.
                //Destroy(this.gameObject, 10f); // Destroy the object
        
            }
    }
    public static void GrabCurrentCameraPosition()
    {
        cameraMovementChecker = Camera.main.transform.position;
    }
    // Launch shape should propably be another class that accepts Shapes and launches them.
    public void LaunchShape(float verticalVelocity, float xSpeed, float xStart)
    {

        IsActive = true;
        speed = xSpeed;
        this.verticalVelocity = verticalVelocity;
        transform.position = new Vector3(xStart, 0.1f, 0);
        if (SceneManager.GetActiveScene().name == "3.FocusTestC")
        {

            if (cameraMovementChecker.z <= Camera.main.transform.position.z) 
            { 
                shapeProjectedOffsetZ = 4f;
            }
            else
            {
                shapeProjectedOffsetZ = 2f;
            }

   
                            
              
            //Instantiate main Camera from scene
            Vector3 projectedShapePosition = new Vector3(Camera.main.transform.position.x,
                                                        Camera.main.transform.position.y,
                                                        Camera.main.transform.position.z + shapeProjectedOffsetZ);
            transform.position = projectedShapePosition;
            transform.rotation = Camera.main.transform.rotation;
            
        }
            
             
        isNotCollided = false;

    }
    //Similarly explode should propably accepts Shapes and its' positions for the collision.
    public void Explode(int incrementalStress)
    {
   

        if (isNotCollided == false)
        {
            // Cloning the tag id to feed to create mini shape method.
            shapeId = gameObject.tag.Clone().ToString();
            // Likewise we're feeding the material to create minishape method.
            materialId = gameObject.GetComponent<Renderer>().material;
            // Destroy the Collided Shape
            Destroy(gameObject, 0.1f);

            LimitIsNotReachedIncreaseBy(incrementalStress);

            for (int x = 0; x < totalMiniShapes; x++)
            {
                for (int y = 0; y < totalMiniShapes; y++)
                {
                    for (int z = 0; z < totalMiniShapes; z++)
                    {
                        CreateMiniShape(x, y, z);
                    }
                }
            }

            // Get explosion position
            Vector3 explosionPos = transform.position;
            // Get colliders in that position and radius
            Collider[] miniShapeColliders = Physics.OverlapSphere(explosionPos, explosionRadius);

            foreach (Collider hit in miniShapeColliders)
            {
                // Get rigidbody "collison"
                Rigidbody rigidBody = hit.GetComponent<Rigidbody>();
                if (rigidBody != null)
                {
                    //Add explosion force to this body with existing parameters
                    rigidBody.AddExplosionForce(explosionForce, transform.position, explosionRadius, explosionUpward);
                    //Set new gravity of explosion (We are stopping the shape on its track and expand with a new gravity).
                    Physics.gravity = new Vector3(0, -GRAVITY, 0);
                    //Destroying the miniShape collider to avoid gravity interference between mini shapes and shapes.
                    Destroy(this.GetComponent<Collider>());
                }
            }

        }
    }
    private void LimitIsNotReachedIncreaseBy(int incrementalStress)
    {
        if (totalMiniShapes < 20)
        {
            totalMiniShapes = miniShapesInRow + incrementalStress;
        }
    }
    void CreateMiniShape(int x, int y, int z)
    {
        //Creating miniShapes for explosion
        

        switch (shapeId) 
        {
            case "Cube": miniShape = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
            case "Sphere": miniShape = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                break;
            case "Cylinder": miniShape = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                break;
            case "Capsule": miniShape = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                break;
            default: miniShape = null;
                break;
        }

        //Setting the miniShapes
        miniShape.transform.position = transform.position + new Vector3(miniShapesSize * x, miniShapesSize * y, miniShapesSize * z) - miniShapesPivot;
        miniShape.transform.localScale = new Vector3(miniShapesSize, miniShapesSize, miniShapesSize);
        miniShape.layer = 9; //Reference to miniShape Layer so that the miniShapes won't interfere with the Actual shape colliders

        //Give mass of the minishapes
        miniShape.AddComponent<Rigidbody>();
        miniShape.GetComponent<Rigidbody>().mass = miniShapesSize;

        //Set the color
        miniShape.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
       if (SceneManager.GetActiveScene().name == "3.FocusTestC")
        {
            gameObject.GetComponent<Renderer>().material.color = black;
        }
            

        miniShape.GetComponent<Renderer>().material = materialId;
        //Set a tag
        miniShape.tag = "Minishape";


        //Manipulate gravity
        Physics.gravity = new Vector3(0, -1.0F, 0);



    }
}
