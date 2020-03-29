using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Unity.Editor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.Scripts;


public class SceneManagemet : MonoBehaviour
{

    #region Private Variables
    private TrueAuditor auditor;
    private TrueAuditor checkResult;
    private string databasePointer = "https://gbenchcasesobservations.firebaseio.com/";
    private string jsonData;
    private DatabaseReference databaseReference;
    private bool grabMacAddress = true;
    private string theInfo;
    private static bool _transitionSwitch = false;
    private Evaluator evaluator;
    #endregion

    #region Public Variables
    public Canvas transitionCanvas;
    public Canvas shortCutCanvas;
    public static bool testAdone, testBdone, testCdone;



    public decimal graphicsScore;
    public static bool transitionSwitch
    {
        get
        {
            return _transitionSwitch;
        }
        set
        {
            _transitionSwitch = value;
        }
    }
    #endregion
    public void Start()
    {
        InitializingVariables();
    }
    private void InitializingVariables()
    {
        evaluator = new Evaluator();
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(databasePointer);
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }
    public void QuitApplication()
    {
    
        evaluator.CalculatePhysicsScore();
        evaluator.CalculateFinalScore();
        testCdone = true;
        PushDataToFireBase();
        SceneManager.UnloadSceneAsync(3);
     
        Application.Quit();
    }


    private void PushDataToFireBase()
    {
        auditor = new TrueAuditor(grabMacAddress, SceneManagemet.testAdone, SceneManagemet.testBdone, SceneManagemet.testCdone);
        string jsonData = JsonUtility.ToJson(auditor);
        Debug.Log(jsonData);
        string uniqueIdForCase = "UserMAC: " + auditor.GettMyMacAddress();
        databaseReference.Child(uniqueIdForCase).SetRawJsonValueAsync(jsonData);
    }

    #region SceneLoaders
    public void LoadFirstFocusTest()
    {
        FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync().ContinueWith(task => { 
        
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log("The Task is faulted or cancelled");
                SceneManager.UnloadSceneAsync(0);
                SceneManager.LoadScene("0.IntroStatement", LoadSceneMode.Single);
            }

            if (task.IsCompleted)
            {
                Debug.Log("The task is completed!");
                GoToFirstScene();
             
                
            }


        });
        
    }

    private void GoToFirstScene()
    {
        SceneManager.UnloadSceneAsync(0);
        SceneManager.LoadScene("1.FocusTestA", LoadSceneMode.Single);
        ManageControlsAndRESUME();
        _transitionSwitch = false;
    }

    public void LoadSecondFocusTest()
    {

        testAdone = true;
        UnityEngine.Debug.Log(Evaluator.exposedAvgFrames);
        evaluator.CalculateGraphicsScore();
        SceneManager.UnloadSceneAsync(1);
        SceneManager.LoadScene("2.FocusTestB", LoadSceneMode.Single);
        ManageControlsAndRESUME();
        _transitionSwitch = false;

    }

    public void LoadThirdFocusTest()
    {
        UnityEngine.Debug.Log(Evaluator.exposedAvgFrames);
        testBdone = true;
        evaluator.CalculateGraphicsScore();
        SceneManager.UnloadSceneAsync(2);
        SceneManager.LoadScene("3.FocusTestC", LoadSceneMode.Single);
        ManageControlsAndRESUME();
        _transitionSwitch = false;
    }
    #endregion

    #region Scene Transition Functions
    public bool hitTheSwitch()
    {
        if (_transitionSwitch == false)
            _transitionSwitch = true;
        else
            _transitionSwitch = false;

        return _transitionSwitch;
    }
    public void enableTransition()
    {
        if (hitTheSwitch() == true)
        {
            ManageControlsAndPAUSE();
        }
        else
        {
            ManageControlsAndRESUME();
        }
    }

    private void ManageControlsAndRESUME()
    {
        // Controls
        transitionCanvas.GetComponent<Canvas>().enabled = false;
        shortCutCanvas.GetComponent<Canvas>().enabled = true;
        // Resume
        Time.timeScale = 1f;
    }

    private void ManageControlsAndPAUSE()
    {
        // Controls
        transitionCanvas.GetComponent<Canvas>().enabled = true;
        shortCutCanvas.GetComponent<Canvas>().enabled = false;
        // Pause
        Time.timeScale = 0f;
    }
    #endregion

}