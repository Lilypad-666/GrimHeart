using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

class MainMenuController : MonoBehaviour {

    public UnityClient client;

    public InputField loginUser;
    public InputField loginPass;

    public Text loginError;

    public InputField registerUser;
    public InputField registerPass;
    public InputField registerPass2;

    public Button loginButton;
    public Button registerButton;

    public Text registerError;

    public Transform mainMenuMount;
    public Transform loginRegisterMount;
    public Transform registerMount;
    public Transform logOutMount;

    public LUI_MenuCamControl camControl;

    public Text mainMenuText;

    public GameObject accountPanel;

    public Sprite logoutSprite;
    public Sprite userSprite;

    public Button playButton;

    public LO_SelectStyle selectStyle;
    public LO_LoadScene loadScene;

    public Button signOutButtonYes;
    public Button signOutButtonNo;

    public Toggle loginRemember;
    public Toggle registerRemember;

    private void Start() {
        playButton.onClick.AddListener(toRegister);
    }

    private void toRegister() {
        camControl.setMount(registerMount);
    }

    private void Awake() {
        client.MessageReceived += messageReceived;
    }

    private void messageReceived(object sender, MessageReceivedEventArgs con) {
        using (Message message = con.GetMessage() as Message) {
            if (message.Tag == Tags.LoginError) {
                using (DarkRiftReader reader = message.GetReader()) {
                    loginError.text = reader.ReadString();
                }
            }

            if (message.Tag == Tags.RegisterError) {
                using (DarkRiftReader reader = message.GetReader()) {
                    registerError.text = reader.ReadString();
                }
            }

            if (message.Tag == Tags.Login) {
                using (DarkRiftReader reader = message.GetReader()) {
                    UserAccount.id = reader.ReadInt32();
                    UserAccount.username = reader.ReadString();
                    string sessionKey = reader.ReadString();

                    if (!sessionKey.Equals("")) {
                        HandleTextFile.writeTextToFile("session", sessionKey);
                    }

                    mainMenuText.text = "Signed In as " + UserAccount.username;
                    accountPanel.transform.GetChild(0).GetComponent<Image>().sprite = logoutSprite;
                    accountPanel.GetComponent<Button>().onClick.RemoveAllListeners();
                    accountPanel.GetComponent<Button>().onClick.AddListener(logOut);
                    camControl.setMount(mainMenuMount);

                    loginButton.GetComponent<Button>().enabled = true;
                    registerButton.GetComponent<Button>().enabled = false;

                    playButton.onClick.RemoveAllListeners();
                    playButton.onClick.AddListener(playGame);

                    StartCoroutine(ClearText());
                }
            }

            if (message.Tag == Tags.SignOut) {
                mainMenuText.text = "";
                accountPanel.transform.GetChild(0).GetComponent<Image>().sprite = logoutSprite;
                accountPanel.GetComponent<Button>().onClick.RemoveAllListeners();
                accountPanel.GetComponent<Button>().onClick.AddListener(signIn);
                camControl.setMount(mainMenuMount);

                signOutButtonYes.enabled = true;
                signOutButtonNo.enabled = true;
            }

            if (message.Tag == Tags.Play) {
                selectStyle.SetStyle("Stock_Style");
                loadScene.ChangeToScene("Game");
            }
        }
    }

    IEnumerator ClearText() {
        yield return new WaitForSeconds(2);

        loginUser.text = "";
        loginPass.text = "";

        registerUser.text = "";
        registerPass.text = "";
        registerPass2.text = "";
    }

    private void playGame() {

        camControl.setMount(mainMenuMount);

        using (DarkRiftWriter writer = DarkRiftWriter.Create()) {

            using (Message message = Message.Create(Tags.Play, writer))
                client.SendMessage(message, SendMode.Reliable);
        }

        playButton.enabled = false;
    }

    private void logOut() {
        camControl.setMount(logOutMount);
    }

    public void sendLogin() {
        String user = loginUser.text;
        String pass = loginPass.text;

        if (user.Length < 1) {
            sendError("Please enter your username!", true);
            return;
        }

        if (pass.Length < 3) {
            sendError("Please enter your password!", true);
            return;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(user, @"^[a-zA-Z0-9]+$")) {
            sendError("Username must contain only alphanumeric characters.", true);
            return;
        }

        loginButton.GetComponent<Button>().enabled = false;

        using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
            writer.Write(user);
            writer.Write(pass);
            writer.Write(loginRemember.isOn);

            using (Message message = Message.Create(Tags.Login, writer))
                client.SendMessage(message, SendMode.Reliable);
        }
    }

    public void sendRegister() {
        String user = registerUser.text;
        String pass = registerPass.text;
        String pass2 = registerPass2.text;

        if (user.Length < 1) {
            sendError("Username must be at least 1 character!", true);
            return;
        }

        if (pass.Length < 3) {
            sendError("Password must be more than 3 characters!", true);
            return;
        }

        if (pass != pass2) {
            sendError("The two passwords must be identical!", false);
            return;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(user, @"^[a-zA-Z0-9]+$")) {
            sendError("Username must contain only alphanumeric characters.", false);
            return;
        }

        registerButton.GetComponent<Button>().enabled = false;

        using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
            writer.Write(user);
            writer.Write(pass);
            writer.Write(registerRemember.isOn);

            using (Message message = Message.Create(Tags.Register, writer))
                client.SendMessage(message, SendMode.Reliable);
        }
    }

    public void sendSignOut() {

        signOutButtonYes.enabled = false;
        signOutButtonNo.enabled = false;

        using (DarkRiftWriter writer = DarkRiftWriter.Create()) {

            using (Message message = Message.Create(Tags.SignOut, writer))
                client.SendMessage(message, SendMode.Reliable);
        }
    }

    private void signIn() {
        camControl.setMount(loginRegisterMount);
    }

    public void sendError(string error, bool login) {
        if (login) {
            GameObject.FindGameObjectWithTag("Login").transform.GetChild(0).GetComponent<Text>().text = error;
        } else {
            GameObject.FindGameObjectWithTag("Register").transform.GetChild(0).GetComponent<Text>().text = error;
        }
    }
}
