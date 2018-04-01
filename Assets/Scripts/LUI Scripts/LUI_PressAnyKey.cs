using UnityEngine;

public class LUI_PressAnyKey : MonoBehaviour {

	[Header("OBJECTS")]
	public GameObject scriptObject;
	public Animator animatorComponent;

	void Start ()
	{
		animatorComponent.GetComponent<Animator>();
	}

	void Update ()
	{
		if (Input.anyKeyDown) 
		{
            playAnim();
			Destroy (scriptObject);
		}
	}

    public void playAnim() {
        animatorComponent.Play("Bloody Splash Anim");
    }
}