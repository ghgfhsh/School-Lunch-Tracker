using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ModifyUsersScreen : menuBaseClass
{
    [SerializeField]private TMP_InputField usernameInput;
    [SerializeField]private TMP_InputField newPasswordInput;
    [SerializeField]private TMP_Dropdown isAdmin;

    [SerializeField] private TMP_Text errorMessage;

    [SerializeField] private GameObject entryTemplate;
    [SerializeField] private Transform content;

    private List<GameObject> instantiatedObjects;

    private List<User> users;

    private User selectedUser;

    private void OnEnable()
    {
        instantiatedObjects = new List<GameObject>();
        PopulateUsersTable();
    }

    private void OnDisable()
    {
        ClearInsantiatedObjectsTable();
    }

    private void ClearInsantiatedObjectsTable()
    {
        foreach (GameObject o in instantiatedObjects)
        {
            Destroy(o);
        }
        instantiatedObjects.Clear();
    }

    void PopulateUsersTable()
    {
        ClearInsantiatedObjectsTable();
        users = DataBase.Instance.GetListofUsers();

        foreach (User user in users)
        {
            GameObject newEntry = Instantiate(entryTemplate);
            newEntry.name = user.username;
            newEntry.transform.SetParent(content);
            newEntry.GetComponentInChildren<TMP_Text>().text = user.username;
            instantiatedObjects.Add(newEntry);

            Button button = newEntry.GetComponent<Button>();
            button.onClick.AddListener(() => { ChangeSelection(button); });
        }
    }

    public void ChangeSelection(Button button)
    {
        if (users.Count == 0)
            return;

        foreach (User user in users)
        {
            if(user.username == button.name)
            {
                selectedUser = user;
                usernameInput.text = user.username;
                isAdmin.value = (user.isAdmin) ? 1 : 0; //convert is admin to int so it will correctly update it
                return;
            }
        }
    }

    private bool ValidateInput(string input)
    {
        if (input.Contains(" ") || input.Contains("\"") || input.Contains("\'"))
        {
            return false;
        }

        return true;
    }

    public void UpdateUser()
    {
        int isAdminInt = isAdmin.value;

        if(selectedUser == null)
        {
            errorMessage.text = "No User Selected";
            return;
        }

        if (!ValidateInput(newPasswordInput.text) || !ValidateInput(usernameInput.text))
        {
            errorMessage.text = "Using Special Characters Not Allowed";
            return;
        }

        if(!DataBase.Instance.UpdateUser(selectedUser.username, usernameInput.text, newPasswordInput.text, isAdmin.value))// if the update fails it is almost surely due to a username already taken
        {
            errorMessage.text = "Username Already Taken";
        }

        PopulateUsersTable();
    }

    public void CreateNewUser()
    {
        DataBase.Instance.CreateNewUser();
        selectedUser = null;
        PopulateUsersTable();
    }

    public void DeleteUser()
    {
        if (selectedUser == null || users.Count <= 1) {
            errorMessage.text = "No User Selected";
            return;
        }
        DataBase.Instance.DeleteUser(selectedUser.username);
        PopulateUsersTable();
    }
}
