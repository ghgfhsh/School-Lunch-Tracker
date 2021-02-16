using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ModifyLunchChoicesScreen : menuBaseClass
{
    [SerializeField] private GameObject entryTemplate;

    [SerializeField] private Transform lunchChoiceContent;
    [SerializeField] private TMP_InputField lunchChoiceNameInput;
    private List<GameObject> lunchChoiceInstantiatedObjects;
    List<LunchChoice> lunchChoices;
    private LunchChoice selectedLunchChoice;


    [SerializeField] private Transform availableProductsContent;
    [SerializeField] private TMP_InputField availableProductNameInput;
    private List<GameObject> availableProductsInstantiatedObjects;
    List<Product> availableProducts;
    private Product selectedAvailableProduct;

    [SerializeField] private Transform chosenProductsContent;
    private List<GameObject> chosenProductsInstantiatedObjects;
    List<Product> chosenProducts;
    private Product selectedChosenProduct;


    private void OnEnable()
    {
        lunchChoiceInstantiatedObjects = new List<GameObject>();
        availableProductsInstantiatedObjects = new List<GameObject>();
        chosenProductsInstantiatedObjects = new List<GameObject>();

        PopulateLunchChoiceTable();
        PopulateAvailableProductsTable();
    }

    private void OnDisable()
    {
        foreach(GameObject o in lunchChoiceInstantiatedObjects)
        {
            Destroy(o);
        }
        lunchChoiceInstantiatedObjects.Clear();

        foreach (GameObject o in availableProductsInstantiatedObjects)
        {
            Destroy(o);
        }
        availableProductsInstantiatedObjects.Clear();

        foreach (GameObject o in chosenProductsInstantiatedObjects)
        {
            Destroy(o);
        }
        chosenProductsInstantiatedObjects.Clear();
    }

    private void RepopulateTables()
    {
        if(selectedLunchChoice != null)
        {
            int lunchChoiceID = selectedLunchChoice.lunchChoiceID;
            PopulateLunchChoiceTable();

            foreach (LunchChoice lunchchoice in lunchChoices) //this makes the user not have to reselect their lunch choice after every delete
            {
                if (lunchchoice.lunchChoiceID == lunchChoiceID)
                    selectedLunchChoice = lunchchoice;
            }

        }
        else
        {
            PopulateLunchChoiceTable();
        }

        PopulateAvailableProductsTable();
        PopulateChosenProductsTable();
    }

    #region Lunch Choice Table

    private void PopulateLunchChoiceTable()
    {
        foreach(GameObject lunchChoice in lunchChoiceInstantiatedObjects)
        {
            Destroy(lunchChoice);
        }
        lunchChoiceInstantiatedObjects.Clear();

        lunchChoices = DataBase.Instance.GetLunchChoices();
        foreach(LunchChoice lunchChoice in lunchChoices)
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
                selectedChosenProduct = null;
                selectedAvailableProduct = null;
                lunchChoiceNameInput.text = lunchChoice.lunchChoiceName;
                PopulateChosenProductsTable();
                return;
            }
        }
    }

    public void DeleteSelectedLunchChoice()
    {
        if (selectedLunchChoice == null)
            return;

        DataBase.Instance.DeleteLunchChoice(selectedLunchChoice.lunchChoiceID);
        selectedLunchChoice = null;
        RepopulateTables();
    }

    public void CreateLunchChoice()
    {
        if (lunchChoiceNameInput.text == "")
            return;

        DataBase.Instance.AddLunchChoice(lunchChoiceNameInput.text);
        RepopulateTables();
    }
    #endregion lunch choice table

    #region chosen products table

    private void PopulateChosenProductsTable()
    {
        foreach (GameObject product in chosenProductsInstantiatedObjects)
        {
            Destroy(product);
        }
        chosenProductsInstantiatedObjects.Clear();
        if (selectedLunchChoice == null)
        {
            return;
        }

        chosenProducts = new List<Product>();
        foreach (Product product in selectedLunchChoice.products)
        {
            GameObject newEntry = Instantiate(entryTemplate);
            newEntry.name = product.productName;
            newEntry.transform.SetParent(chosenProductsContent);
            newEntry.GetComponentInChildren<TMP_Text>().text = product.productName;
            chosenProductsInstantiatedObjects.Add(newEntry);
            chosenProducts.Add(product);

            Button button = newEntry.GetComponent<Button>();
            button.onClick.AddListener(() => { ChangeSelectedChosenProduct(button); });
        }
    }

    public void ChangeSelectedChosenProduct(Button button)
    {
        if (chosenProductsInstantiatedObjects.Count == 0)
            return;

        foreach (Product product in chosenProducts)
        {
            if (product.productName == button.name)
            {
                selectedChosenProduct = product;
                selectedAvailableProduct = null;
                return;
            }
        }
    }

    public void RemoveSelectedChosenProduct()
    {
        if (selectedChosenProduct == null)
            return;

        DataBase.Instance.RemoveProductFromLunchChoice(selectedLunchChoice.lunchChoiceID, selectedChosenProduct.productId);
        RepopulateTables();
    }
    #endregion

    #region available products table

    private void PopulateAvailableProductsTable()
    {
        foreach (GameObject o in availableProductsInstantiatedObjects)
        {
            Destroy(o);
        }
        availableProductsInstantiatedObjects.Clear();

        availableProducts = DataBase.Instance.GetProducts();
        foreach (Product product in availableProducts)
        {
            GameObject newEntry = Instantiate(entryTemplate);
            newEntry.name = product.productName;
            newEntry.transform.SetParent(availableProductsContent);
            newEntry.GetComponentInChildren<TMP_Text>().text = product.productName;
            availableProductsInstantiatedObjects.Add(newEntry);

            Button button = newEntry.GetComponent<Button>();
            button.onClick.AddListener(() => { ChangeAvailableProductSelection(button); });
        }
    }

    private void ChangeAvailableProductSelection(Button button)
    {
        if (availableProductsInstantiatedObjects.Count == 0)
            return;

        foreach (Product product in availableProducts)
        {
            if (product.productName == button.name)
            {
                selectedAvailableProduct = product;
                selectedChosenProduct = null;
                return;
            }
        }
    }

    public void AddSelectedAvailableProduct()
    {
        if (selectedAvailableProduct == null || selectedLunchChoice == null)
            return;

        DataBase.Instance.AddProductToLunchChoice(selectedLunchChoice.lunchChoiceID, selectedAvailableProduct.productId);
        RepopulateTables();
    }

    public void DeleteSelectedAvailableProduct()
    {
        if (selectedAvailableProduct == null)
            return;

        DataBase.Instance.DeleteProduct(selectedAvailableProduct.productId);
        RepopulateTables();
    }

    public void CreateAvailableProduct()
    {
        if (availableProductNameInput.text == "")
            return;

        DataBase.Instance.AddProduct(availableProductNameInput.text);
        RepopulateTables();
    }
    #endregion
}
