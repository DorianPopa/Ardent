using System;
using TMPro;
using UnityEngine;

public class LoginMenuManager : MonoBehaviour
{
    public NetworkManager networkManager = null;
    public AppController appController = null;

    public TMP_InputField usernameField;
    public TMP_InputField passwordField;

    public TMP_Text errorField;

    public string Username;
    public string PasswordPlain;

    private Session currentUserSession = null;


    void Awake()
    {
        networkManager = FindObjectOfType<NetworkManager>();
        appController = FindObjectOfType<AppController>();

        passwordField.contentType = TMP_InputField.ContentType.Password;
        errorField.enabled = false;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }


    public void UpdatePassword()
    {
        PasswordPlain = passwordField.text;
        errorField.enabled = false;
    }

    public void UpdateUsername()
    {
        Username = usernameField.text;
        errorField.enabled = false;
    }

    public async void AttemptLogin()
    {
        if(currentUserSession == null)
        {
            LoginUserModel loginAttemptData = new LoginUserModel { Username = this.Username, PasswordPlain = this.PasswordPlain};

            try
            {
                currentUserSession = await networkManager.Login(loginAttemptData);
                appController.LoadNextScene();
            }
            catch
            {
                errorField.text = "Login Error";
                errorField.enabled = true;
            }
        }
    }
}
