using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChangePasswordScreen : menuBaseClass
{
    [SerializeField]private TMP_InputField oldPasswordInput;
    [SerializeField]private TMP_InputField newPasswordInput;
    [SerializeField]private TMP_InputField confirmPasswordInput;

    [SerializeField] TMP_Text errorMessage;



    public void ChangePassword()
    {
        string oldPassword = oldPasswordInput.text;
        string newPassword = newPasswordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        if(confirmPassword != newPassword)
        {
            errorMessage.text = "new passwords do not match";
        }
        else if (!DataBase.Instance.activeUser.CheckPassword(oldPassword))
        {
            errorMessage.text = "Incorrect Password";
        }
        else
        {
            DataBase.Instance.ChangePassword(newPassword);
            errorMessage.text = "Password Changed";
        }
    }

    private bool VerifyPassword(string password)
    {
        if(password.Contains(" ") || password.Contains("\"") || password.Contains("\'"))
        {
            return false;
        }

        return true;
    }
}
