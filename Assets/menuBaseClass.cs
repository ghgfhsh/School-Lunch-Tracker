using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class menuBaseClass : MonoBehaviour
{
    public GameObject previousScreen;

    public void OpenMenu(GameObject previousScreen)
    {
        previousScreen.SetActive(false);
        gameObject.SetActive(true);
    }

    public void GoToPreviousScreen()
    {
        previousScreen.SetActive(true);
        gameObject.SetActive(false);
    }
}
