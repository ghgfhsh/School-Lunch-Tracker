using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SimpleFileBrowser;
using UnityEditor;

public class MainMenu : menuBaseClass
{
    public GameObject changePasswordScreen;
    public GameObject adminOptions;
    public GameObject modifyUserScreen;
    public GameObject modifyLunchOptionsScreen;
    public GameObject studentLunchChoicesScreen;
    public GameObject lunchDataScreen;

    public string initialFilename { get; private set; }

    private void OnEnable()
    {
        if (DataBase.Instance.activeUser.isAdmin)
            adminOptions.SetActive(true);
        else
            adminOptions.SetActive(false);
    }

    public void OpenChangePasswordScreen()
    {
        changePasswordScreen.SetActive(true);
        gameObject.SetActive(false);
    }

    public void OpenModifyUserScreen()
    {
        modifyUserScreen.SetActive(true);
        gameObject.SetActive(false);
    }

    public void OpenModifyLunchOptionsScreen()
    {
        modifyLunchOptionsScreen.SetActive(true);
        gameObject.SetActive(false);
    }

    public void OpenStudentLunchChoicesScreen()
    {
        studentLunchChoicesScreen.SetActive(true);
        gameObject.SetActive(false);
    }

    public void SaveLunchChoiceSpreadSheet()
    {
        //assign delegate to save the lunch data to the appropriate path once the pop window successfully retrieves it. 
        //the delegate is added to a call back inside the widow
        FileBrowser.OnSuccess onSuccessDelegate = new FileBrowser.OnSuccess(new System.Action<string[]>((string[] paths) => 
        {
            StreamWriter writer = new StreamWriter(paths[0], false);
            Dictionary<string, int> lunchChoices = DataBase.Instance.GetTodaysLunchChoices();

            writer.WriteLine("Lunch Choice,Amount of Lunches");
            foreach (string lunchChoice in lunchChoices.Keys)
            {
                Debug.Log(lunchChoice + ", " + lunchChoices[lunchChoice]);
                writer.WriteLine(lunchChoice + "," + lunchChoices[lunchChoice]);
            }
            writer.Close();
        }));
        FileBrowser.SetFilters(false, new FileBrowser.Filter("CSV", ".csv"));
        FileBrowser.ShowSaveDialog(onSuccessDelegate, null, FileBrowser.PickMode.FilesAndFolders, initialFilename : "Student Lunch Choice Spreadsheet");

    }

    public void OpenLunchDataScreen()
    {
        lunchDataScreen.SetActive(true);
        gameObject.SetActive(false);
    }
}
