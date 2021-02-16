using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoginScreen : MonoBehaviour
{


    [SerializeField]GameObject wrongPasswordMessage;
    [SerializeField]private TMP_InputField usernameInput;
    [SerializeField]private TMP_InputField passwordInput;

    [SerializeField] private MainMenu mainMenu;

    public void loginButtonPressed()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        //check that username and password is not null
        if (username != "" && password != "")
        {
            if (DataBase.Instance.Login(username, password))
            {
                mainMenu.OpenMenu(gameObject);
            }
            else
                wrongPassword();

        }
        else
            wrongPassword();
    }

    void wrongPassword()
    {
        wrongPasswordMessage.SetActive(true);
    }
}
