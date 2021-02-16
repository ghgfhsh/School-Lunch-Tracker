using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;


//FIX DATA DROPDOWN
public class LunchDataScreen : menuBaseClass
{
    [SerializeField]private WindowGraph windowGraph;
    [SerializeField] private TMP_Dropdown dataTypeDropDown;
    [SerializeField] private TMP_Dropdown dataChoiceDropDown;
    [SerializeField] private TMP_Dropdown displayModeDropdown;
    [SerializeField] private PredictFutureOrders predictFutureOrders;

    private bool isDiplayingProducts = false;

    private int currentID = 1;
    private WindowGraph.DisplayMode currentDisplayMode = WindowGraph.DisplayMode.Day;

    Dictionary<int, LunchChoice> availableLunchChoices = new Dictionary<int, LunchChoice>();
    Dictionary<int, Product> availalbeProducts = new Dictionary<int, Product>();

    private void OnEnable()
    {
        WindowGraph.Instance.CreateGraphVisuals(5);
        PopulateDataDropDown();

        PopulateGraph();
    }

    private void PopulateGraph()
    {
        List<Tuple<string, int>> studentChoices = DataBase.Instance.GetOrderInformation(currentID, currentDisplayMode, isDiplayingProducts);
        List<Tuple<string, int>> predictedStudentChoices = predictFutureOrders.GetForecastedChoices(studentChoices, currentDisplayMode);
        List<string> dateStrings = new List<string>();
        int predictionStartIndex = 0;

        predictionStartIndex = studentChoices.Count;
        studentChoices.AddRange(predictedStudentChoices);

        List<int> lunchChoiceAmounts = new List<int>();

        if (studentChoices.Count == 0)
        {
            studentChoices.Add(Tuple.Create("1900-1-1", 0));
        }

        foreach (Tuple<string, int> studentLunchChoice in studentChoices)
        {
            lunchChoiceAmounts.Add(studentLunchChoice.Item2);
            dateStrings.Add(studentLunchChoice.Item1);
        }


        WindowGraph.Instance.LoadGraphData(lunchChoiceAmounts, dateStrings, predictionStartIndex, currentDisplayMode);
        WindowGraph.Instance.SetGraphDisplayMode(currentDisplayMode);

    }

    public void OnDataTypeDropDownChanged(TMP_Dropdown change)
    {
        if(change.value == 0)
        {
            isDiplayingProducts = false;
        }
        else
        {
            isDiplayingProducts = true;
        }

        PopulateDataDropDown();
        PopulateGraph();
    }

    public void OnDisplayModeDropdownChanged(TMP_Dropdown change)
    {
        switch (change.value)
        {
            case 0:
                currentDisplayMode = WindowGraph.DisplayMode.Day;
                break;
            case 1:
                currentDisplayMode = WindowGraph.DisplayMode.Month;
                break;
            case 2:
                currentDisplayMode = WindowGraph.DisplayMode.Year;
                break;
        }
        PopulateGraph();
    }

    public void OnDataDropDownChanged(TMP_Dropdown change)
    {
        if (!isDiplayingProducts)
        {
            currentID = availableLunchChoices[change.value].lunchChoiceID;
        }
        else
        {
            currentID = currentID = availalbeProducts[change.value].productId;
        }
        PopulateGraph();
    }

    private void PopulateDataDropDown()
    {
        dataChoiceDropDown.ClearOptions();
        if (!isDiplayingProducts)
        {
            availableLunchChoices.Clear();
            List<LunchChoice> lunchChoices = DataBase.Instance.GetLunchChoices();
            List<TMP_Dropdown.OptionData> optionDataList = new List<TMP_Dropdown.OptionData>();

            int index = 0;
            foreach (LunchChoice lunchChoice in lunchChoices)
            {
                TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                optionData.text = lunchChoice.lunchChoiceName;
                optionDataList.Add(optionData);
                availableLunchChoices.Add(index, lunchChoice);
                index++;
            }
            dataChoiceDropDown.AddOptions(optionDataList);
        }
        else
        {
            availalbeProducts.Clear();
            List<Product> products = DataBase.Instance.GetProducts();
            List<TMP_Dropdown.OptionData> optionDataList = new List<TMP_Dropdown.OptionData>();

            int index = 0;
            foreach (Product product in products)
            {
                TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                optionData.text = product.productName;
                optionDataList.Add(optionData);
                availalbeProducts.Add(index, product);
                index++;
            }
            dataChoiceDropDown.AddOptions(optionDataList);
        }
    }
}
