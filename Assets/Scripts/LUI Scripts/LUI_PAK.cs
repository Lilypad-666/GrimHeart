using UnityEngine;
using DarkRift.Client.Unity;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using DarkRift;

public class LUI_PAK : MonoBehaviour {

    [Header("VARIABLES")]
    public GameObject mainCanvas;
    public GameObject scriptObject;
    public Animator animatorComponent;
    public string animName;
    public UnityClient unityClient;

    LUI_DissolveEdge dissolve;

    void Start() {
        animatorComponent.GetComponent<Animator>();

        dissolve = transform.parent.GetComponentInChildren<LUI_DissolveEdge>();
    }

    void Update() {
        if (Input.anyKeyDown && transform.parent.GetComponentInChildren<Text>().text != "LOADING...") {
            if (unityClient.Connected) {
                StartCoroutine(FadeOut(true));
            } else {
                StartCoroutine(FadeOut(false));
            }
        }
    }

    IEnumerator FadeOut(bool first) {
        startFadeOut();

        yield return new WaitForSeconds(0.8f);

        if (first) {
            sendSession();
            mainCanvas.SetActive(true);
            animatorComponent.Play(animName);
            startFadeIn();
            setInvisible();
            Destroy(scriptObject);
        } else {
            unityClient.reconnect();

            if (unityClient.Connected) {
                sendSession();
                mainCanvas.SetActive(true);
                animatorComponent.Play(animName);
                startFadeIn();
                setInvisible();
                Destroy(scriptObject);
            } else {
                startFadeIn();
            }
        }
    }

    void setInvisible() {
        transform.parent.GetChild(0).GetComponent<Image>().enabled = false;
        transform.parent.GetChild(1).GetComponent<Image>().enabled = false;
        transform.parent.GetChild(2).GetComponent<Text>().enabled = false;
    }

    void startFadeOut() {
        dissolve.dissolveValue = 0;
        dissolve.playedOnce = false;
        dissolve.fadingOut = false;
    }

    void startFadeIn() {
        dissolve.dissolveValue = 1;
        dissolve.playedOnce = false;
        dissolve.fadingOut = true;
    }

    void sendSession() {
        string sessionKey = HandleTextFile.readFromTextFile("session");
        Debug.Log("Session key: " + sessionKey);
        if (!sessionKey.Equals("")) {
            using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                writer.Write(sessionKey);
                using (Message message = Message.Create(Tags.SessionLogIn, writer))
                    unityClient.client.SendMessage(message, SendMode.Reliable);
            }
        }
    }
}