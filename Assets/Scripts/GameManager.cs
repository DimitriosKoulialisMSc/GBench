using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.Scripts;
using System.Diagnostics;



// Game Manager is the class responsible in loading the main mechanisms of the benchmark and initializes constants.
public class GameManager : MonoBehaviour
{

    #region Variables

    #region public
    public GameObject Sphere, Cube, Cylinder, Capsule; // The variety of shapes to be launched.
    public Transform trail; // Trail effect of interaction.
    public Canvas transitionCanvas; // The canvas-panel that will take us to the next scene and a shortcut one
    public Canvas shortcutCanvas;
    public SceneManagemet mySceneManager;
    public GameObject LeftRed, RightBlue, BackgroundWall;
    public float showCPU;
    #endregion

    #region private
    private List<Shape> shapes = new List<Shape>(); // Keeping track of the amount of shapes launched.
    private Collider[] shapeCollider; // The array that will hold the colliders of our Shapes.
    private float timeLastShapeSpawned; // Will hold the time the last shape was spawn.
    private float intervalSpawnOfShapes = 1.0f; // The interval of spawns, every second.
    private const float VELOCITY = 400f; // Simulating a drag to interact with objects rather than click, giving a constant for a minimum velocity.
    private Vector3 lastMousePosition; // Will keep the last position of the mouse.
    private int incrementShapeStressor; // This will increase the amount of objects generated when the user breaks them over time.
    private int keyForDataAccu; // This will give us the key value for accumulating data in dictionaries.
    private bool menuSwitch; // Pause Menu Switch

    private Collider[] thisFrameArrayOfColliders;
    private UnityEngine.Object[] shapesMaterials;

    private float shakeDetectionThreshold;
    private float timer;

    private float currentFPS;
    private Evaluator evaluator;

#endregion

#endregion

    // This method runs on the start of execution of the script.
    private void Start()
    {
        InitializingVariables();
        PauseGameOnIntroStatementScene();
        DisableMenuCanvas();
        if (weAreOnFocusTestA())
            LoadTexturesPool();
        EnableShortCutCanvasOnTests(); //Enabling the shortcut button on tests.

        /*We're invoking a repeating method to increase in GPU workload over time. Every x seconds with an initial delay of x seconds.
        The incremented property will be taken as a parameter to the Explode method later on.*/
        InvokeRepeating("IncrementingExplodingShapes", 20f, 20f);
        

    }

    #region Variables Management
    private void InitializingVariables()
    {
        if (weAreOnFocusTestC())
        {
            Application.targetFrameRate = -1; //Disabling Vsync for Android
            keyForDataAccu = 0;
        }

        evaluator = new Evaluator(); // Instatiating evaluator
        incrementShapeStressor = 0; // Initializing the stressor;
        currentFPS = (float)1.0 / Time.deltaTime;
        shakeDetectionThreshold = 4.5f;  //  Setting the Shake threshold (3.6 was the initial default value)
        shapeCollider = new Collider[0]; // Initializing Colliders of gameobjects for the user interaction.
        menuSwitch = false; // Initializing Menu switch to false;
        trail.GetComponent<TrailRenderer>().forceRenderingOff = false;
        trail.GetComponent<TrailRenderer>().autodestruct = false;
        Time.timeScale = 1f; // Start the time, the test is runing
        evaluator.GetCpuUsage(); // Initializing the values in the method
    }
    private void LoadTexturesPool()
    {
        shapesMaterials = Resources.LoadAll("Textures", typeof(Material)); // Loading all available Textures 
    }
    #endregion

    #region Specialized Controls Management
    private void LightSourcesSwitchOverTime()
    {
        timer += Time.deltaTime;
        if (weAreOnFocusTestB())
        {
            if (timer < 20f) // The first 20seconds of the test
            {
                LeftRed.GetComponent<Light>().enabled = true;
                RightBlue.GetComponent<Light>().enabled = false;
            }
            else if (timer < 40f) // The next 20 seconds of the test after the first 20
            {
                LeftRed.GetComponent<Light>().enabled = false;
                RightBlue.GetComponent<Light>().enabled = true;
            }
            else // For the rest of the period
            {
                LeftRed.GetComponent<Light>().enabled = true;
            }
        }
    }
    void IncrementingExplodingShapes()
    {
        // The method that increases the total amount of minishapes on explosion.
        incrementShapeStressor++;


        if (weAreOnFocusTestC()) {

            keyForDataAccu++;
            incrementShapeStressor++;
            UnityEngine.Debug.Log(Evaluator.exposedAvgFrames);
            evaluator.StageAverageFrames(keyForDataAccu);
            evaluator.AccumulateDataForPhysicsScoreCalculation(keyForDataAccu, evaluator.GetCpuUsage());
            evaluator.CalculatePhysicsScore();

        }
        else
        {
            evaluator.StageAverageFrames(incrementShapeStressor);
            UnityEngine.Debug.Log("Kaboom"); // A console output to show the increase of objects.

        }
        
    }

    #endregion

    // This method runs on every frame.
    [System.Obsolete]
    private void Update()
    {
        //Generate Scoring for tests
        evaluator.UpdateCumulativeMovingAverageFPS(); 
        // User Input comes through the shake of the handheld device.
        ShakeDetection();
        //The how to launch shapes.
        ShapeLauncher();
        //Destroy Minishape GameObject if it goes out of the screen.
        OnBecomeInvisibleDestroyMiniShape();
        //Check if the user swipes the screen to meet an object.
        OnUserInteraction();
        //The light sources will be switched accordingle over time for FocusTestB.
        LightSourcesSwitchOverTime();
            
    }

    #region Scene Indicators Management
    private static bool weAreOnFocusTestA()
    {
        return SceneManager.GetActiveScene().name == "1.FocusTestA";
    }

    private static bool weAreOnFocusTestB()
    {
        return SceneManager.GetActiveScene().name == "2.FocusTestB";
    }

    private static bool weAreOnFocusTestC()
    {
        return SceneManager.GetActiveScene().name == "3.FocusTestC";
    }
    #endregion

    #region User Interaction Management


    [System.Obsolete]
    void ShakeDetection()
    {
        if (Input.acceleration.sqrMagnitude >= shakeDetectionThreshold)
        {


            hitTheTransitionSwitch();
            enableTransition(menuSwitch);

            /*if (weAreOnFocusTestB())
                if (BackgroundWall != null && menuSwitch == true)
                    BackgroundWall.GetComponent<MeshRenderer>().enabled = false;
                else
                    BackgroundWall.GetComponent<MeshRenderer>().enabled = true;*/

        }
    }
    private float MouseApproachSpeed()
    {
        return (Input.mousePosition - lastMousePosition).sqrMagnitude;
    }
    private void OnUserInteraction()
    {
        //On this method we're checking the speed of cursor approach, and if we have a collision of colliders we proceed with the effects.
        if (Input.GetMouseButton(0))
        {

            // Getting the current position of cursor.
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);


            //Bringing it on the same level.
            Vector3 posV3 = new Vector2(pos.x, pos.y);


            // Generate a new vector for the colliders from the cursor position.
            thisFrameArrayOfColliders = Physics.OverlapSphere(posV3, 0.3f, LayerMask.GetMask("Shape"));


            // Hold how many Shapes hit the collider component on the specific layer.
            if (SceneManager.GetActiveScene().name == "3.FocusTestC")
            {
                // Generate a new vector for the colliders from the cursor position.
                thisFrameArrayOfColliders = Physics.OverlapSphere(pos, 3.8f, LayerMask.GetMask("Shape"));
            }
            else
            {
                // Give the trail the same position.
                pos.z = -1f;
                trail.position = pos;
                // Generate a new vector for the colliders from the cursor position.
                thisFrameArrayOfColliders = Physics.OverlapSphere(posV3, 0.3f, LayerMask.GetMask("Shape"));
            }


            //Debug.Log(Camera.main.ScreenToWorldPoint(Input.mousePosition).ToString());
            if (MouseApproachSpeed() > VELOCITY)
            {
                foreach (Collider shapesCollider in thisFrameArrayOfColliders)
                {
                    for (int i = 0; i < shapeCollider.Length; i++)
                    {
                        if (shapesCollider == shapeCollider[i])
                        {
                            if (shapesCollider.GetComponent<Shape>() != null)
                            {
                                shapesCollider.GetComponent<Shape>().Explode(incrementShapeStressor);
                            }
                        }
                    }
                }
            }

            // Hold our last position
            lastMousePosition = Input.mousePosition;
            // Reset the collider
            shapeCollider = thisFrameArrayOfColliders;


        }
    }
    #endregion

    #region Shape Management

    private void ShapeLauncher()
    {
        if (Time.time - timeLastShapeSpawned > intervalSpawnOfShapes) //Checking the spawn interval.
        {

            Shape shape = GenerateShape();
            //Creating random values for launcher
            float randomX = UnityEngine.Random.Range(-1.7f, 1.7f);
            shape.LaunchShape(UnityEngine.Random.Range(1.9f, 2.8f), randomX, -randomX);
            timeLastShapeSpawned = Time.time;
        }
        else
        {
            //Grab the Camera position
            Shape.GrabCurrentCameraPosition();
        }
    }
    public Shape GenerateShape()
    {
        Shape generatedShape = shapes.Find(x => !x.IsActive); //Finding and Getting the inactive shapes that are populated in the List shapes.

        int shaperSelector = (int)UnityEngine.Random.Range(0f, 4f);

        if (generatedShape == null) // if we failed to find another shape
        {


            if (SceneManager.GetActiveScene().name == "1.FocusTestA")
            {

                switch (shaperSelector)
                {
                    case 0:
                        generatedShape = Instantiate(Sphere).GetComponent<Shape>();
                        break;
                    case 1:
                        generatedShape = Instantiate(Cylinder).GetComponent<Shape>();
                        break;
                    case 2:
                        generatedShape = Instantiate(Cube).GetComponent<Shape>();
                        break;
                    case 3:
                        generatedShape = Instantiate(Capsule).GetComponent<Shape>();
                        break;
                    default:
                        generatedShape = null;
                        break;
                }

                generatedShape.GetComponent<Renderer>().material = (Material)shapesMaterials[UnityEngine.Random.Range(0, 17)];
            }
            else if (SceneManager.GetActiveScene().name == "2.FocusTestB")
            {
                generatedShape = Instantiate(Capsule).GetComponent<Shape>();
                generatedShape.GetComponent<Renderer>().material.SetColor("_Color", Color.white);

            }
            else
            {
                generatedShape = Instantiate(Cylinder).GetComponent<Shape>();
                generatedShape.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            }

            shapes.Add(generatedShape); //Add it to the list
        }

        return generatedShape;
    }
    void OnBecomeInvisibleDestroyMiniShape()
    {
        if (!(SceneManager.GetActiveScene().name == "3.FocusTestC"))
        {
            if (GameObject.FindGameObjectWithTag("Minishape") != null && GameObject.FindGameObjectWithTag("Minishape").transform.position.y < -1f)
            {
                Destroy(GameObject.FindGameObjectWithTag("Minishape"));
            }
        }
        else
        {
            if (GameObject.FindGameObjectWithTag("Minishape") != null && GameObject.FindGameObjectWithTag("Minishape").transform.position.y < 1f)
            {
                Destroy(GameObject.FindGameObjectWithTag("Minishape"));
            }
        }
    }

    #endregion

    #region Transition Menu Management
    private void DisableMenuCanvas()
    {
        transitionCanvas.GetComponent<Canvas>().enabled = false;
        shortcutCanvas.GetComponent<Canvas>().enabled = false;
    }

    private void EnableShortCutCanvasOnTests()
    {
        if (SceneManager.GetActiveScene().name != "0.IntroStatement")
            shortcutCanvas.GetComponent<Canvas>().enabled = true;
    }
    private static void PauseGameOnIntroStatementScene()
    {
        if (SceneManager.GetActiveScene().name == "0.IntroStatement")
        {
            Time.timeScale = 0f;
        }
    }
    private void hitTheTransitionSwitch()
    {
        if (menuSwitch == false) menuSwitch = true; // Turn (Pause Menu/ Tests' Transition) on.
        else menuSwitch = false; // Trun it off;
    }
    private void enableTransition(bool menuSwitch)
    {

        if (menuSwitch == true)
        {
            transitionCanvas.GetComponent<Canvas>().enabled = true;
            shortcutCanvas.GetComponent<Canvas>().enabled = false;
            trail.GetComponent<TrailRenderer>().enabled = false;
            SceneManagemet.transitionSwitch = true;
            Time.timeScale = 0f;
        }
        else
        {
            transitionCanvas.GetComponent<Canvas>().enabled = false;
            shortcutCanvas.GetComponent<Canvas>().enabled = true;
            trail.GetComponent<TrailRenderer>().enabled = true;
            SceneManagemet.transitionSwitch = false;
            Time.timeScale = 1f;
        }
    }


    #endregion

}

