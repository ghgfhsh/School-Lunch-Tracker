using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class StudentLunchChoicesScreen : menuBaseClass
{
    [SerializeField] private GameObject entryTemplate;


    [SerializeField] private Transform lunchChoiceContent;
    [SerializeField] private TMP_InputField lunchChoiceAmountInput;
    private List<GameObject> lunchChoiceInstantiatedObjects;
    List<LunchChoice> lunchChoices;
    private LunchChoice selectedLunchChoice;

    private void OnEnable()
    {
        lunchChoiceInstantiatedObjects = new List<GameObject>();
        PopulateLunchChoiceTable();
    }

    private void OnDisable()
    {
        foreach (GameObject o in lunchChoiceInstantiatedObjects)
        {
            Destroy(o);
        }
        lunchChoiceInstantiatedObjects.Clear();
        lunchChoiceAmountInput.text = "";
    }

    private void PopulateLunchChoiceTable()
    {
        foreach (GameObject lunchChoice in lunchChoiceInstantiatedObjects)
        {
            Destroy(lunchChoice);
        }
        lunchChoiceInstantiatedObjects.Clear();

        lunchChoices = DataBase.Instance.GetLunchChoices().OrderBy(p => p.lunchChoiceName).ToList(); // this will order the lunch choices by name
        foreach (LunchChoice lunchChoice in lunchChoices)
        {
            GameObject newEntry = Instantiate(entryTemplate);
            newEntry.name = lunchChoice.lunchChoiceName;
            newEntry.transform.SetParent(lunchChoiceContent);
            newEntry.GetComponentInChildren<TMP_Text>().text = lunchChoice.lunchChoiceName;
            lunchChoiceInstantiatedObjects.Add(newEntry);

            Button button = newEntry.GetComponent<Button>();
            button.onClick.AddListener(() => { ChangeLunchChoiceSelection(button); });
        }
    }

    public void ChangeLunchChoiceSelection(Button button)
    {
        if (lunchChoices.Count == 0)
            return;

        foreach (LunchChoice lunchChoice in lunchChoices)
        {
            if (lunchChoice.lunchChoiceName == button.name)
            {
                selectedLunchChoice = lunchChoice;
                return;
            }
        }
    }

    public void SaveButtonClicked()
    {
        StudentLunchChoice newLunchChoice = new StudentLunchChoice(selectedLunchChoice, DataBase.Instance.activeUser.username, int.Parse(lunchChoiceAmountInput.text));

        int value;
        if (!int.TryParse(lunchChoiceAmountInput.text, out value)) {
            Debug.Log("Input is invalid");
            return;
        }

        DataBase.Instance.AddStudentLunchChoices(newLunchChoice);
    }
}
