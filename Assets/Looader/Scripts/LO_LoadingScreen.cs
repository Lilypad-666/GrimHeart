using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
 
public class LO_LoadingScreen : MonoBehaviour
{
	private static LO_LoadingScreen instance = null;

	[Header("RESOURCES")]
	public CanvasGroup canvasAlpha;
    public Text status;
	public Slider progressBar;
	public static string prefabName = "Stock_Style";

	[Header("SETTINGS")]
    public float animationSpeed = 1.25f;

	[Header("PRESS ANY KEY (WIP)")]
	public Animator objectAnimator;
	public bool pressAnyKeySupport;
	private bool isAnimPlayed = false;
 
    // Scene loading process
    private AsyncOperation loadingProcess;

    // Load a new scene
    public static void LoadScene(string sceneName)
    {
        // If there isn't a LoadingScreen, then create a new one
        if (instance == null)
        {
			instance = Instantiate(Resources.Load<GameObject>(prefabName)).GetComponent<LO_LoadingScreen>();
			// Don't destroy loading screen while it's loading
            DontDestroyOnLoad(instance.gameObject);
        }
         
        // Enable loading screen
        instance.gameObject.SetActive(true);
        // Start loading between scenes (Background process. That's why there is an Async)
        instance.loadingProcess = SceneManager.LoadSceneAsync(sceneName);
        // Don't switch scene even after loading is completed
        instance.loadingProcess.allowSceneActivation = false;
    }

    void Awake()
    {
        // Set loading screen invisible at first (panel alpha color)
		canvasAlpha.alpha = 0f;
    }
 
    void Update()
    {
        // Update loading status
		progressBar.value = loadingProcess.progress;
		status.text = Mathf.Round(progressBar.value * 100f).ToString() + "%";
         
        // If loading is complete
		if (loadingProcess.isDone && pressAnyKeySupport == false)
        {
            // Fade out
			canvasAlpha.alpha -= animationSpeed * Time.deltaTime;
             
            // If fade out is complete, then disable the object
			if (canvasAlpha.alpha <= 0)
            {
                gameObject.SetActive(false);
            }
        }
        else // If loading proccess isn't completed
        {
            // Start Fade in
			canvasAlpha.alpha += animationSpeed * Time.deltaTime;
             
            // If loading screen is visible
			if (canvasAlpha.alpha >= 1)
            {
                // We're good to go. New scene is on! :)
                loadingProcess.allowSceneActivation = true;
            }
        }

		if (progressBar.value == 1 && pressAnyKeySupport == true && isAnimPlayed == false) 
		{
			objectAnimator.enabled = true;
			objectAnimator.Play ("PAK Fade-in");
			isAnimPlayed = true;
		}
    }
}