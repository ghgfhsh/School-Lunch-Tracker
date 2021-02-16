using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.TimeSeries;

public class PredictFutureOrders : MonoBehaviour
{
    private static string dataPath = "D:/TestCSV.csv";
    private List<string> dateStrings;

    public List<Tuple<string, int>> GetForecastedChoices(List<Tuple<string, int>> inputData, WindowGraph.DisplayMode displayMode)
    {
        dateStrings = new List<string>();

        var inputDataAsTimeSeriesData = new List<TimeSeriesData>();

        foreach (Tuple<string, int> tuple in inputData)
        {
            if (displayMode != WindowGraph.DisplayMode.Year)
                inputDataAsTimeSeriesData.Add(new TimeSeriesData(DateTime.Parse(tuple.Item1), tuple.Item2));
            else
                inputDataAsTimeSeriesData.Add(new TimeSeriesData(DateTime.ParseExact(tuple.Item1, "yyyy", System.Globalization.CultureInfo.InvariantCulture), tuple.Item2));
        }

        if (inputDataAsTimeSeriesData.Count < 7)
        {
            return new List<Tuple<string, int>>();
        }

        MLContext context = new MLContext(); // defines a new ML context

        //IDataView dataView = context.Data.LoadFromTextFile<ChoiceRecord>(path: dataPath, hasHeader: true, separatorChar: ','); // loads data from csv into the dataView
        IDataView dataView = context.Data.LoadFromEnumerable(inputDataAsTimeSeriesData);

        List<Tuple<string, int>> newTuples = new List<Tuple<string, int>>();

        // build a training pipeline for forecasting data
        var pipeline = context.Forecasting.ForecastBySsa(
            "Forecast",
            nameof(TimeSeriesData.amountOfChoices),
            windowSize: (inputDataAsTimeSeriesData.Count > 365) ? 365 : inputDataAsTimeSeriesData.Count / 4,
            seriesLength: inputDataAsTimeSeriesData.Count + 1,
            trainSize: (((inputDataAsTimeSeriesData.Count > 365) ? 365 : inputDataAsTimeSeriesData.Count / 4)) * 2 + 1,
            horizon: (inputDataAsTimeSeriesData.Count > 365) ? 365 : inputDataAsTimeSeriesData.Count
            );


        // train the model
        var model = pipeline.Fit(dataView);

        var forecastingEngine = model.CreateTimeSeriesEngine<TimeSeriesData, TimeSeriesForecast>(context);

        var forecasts = forecastingEngine.Predict();

        int index = 0;
        DateTime currentDate = inputDataAsTimeSeriesData.Last().date;

        //make sure next prediction date starts in the school year
        int schoolStartMonth = 8;
        int schoolStartDay = 24;
        int schoolEndMonth = 5;
        int schoolEndDay = 15;

        if (displayMode == WindowGraph.DisplayMode.Day)
        {
            currentDate.AddDays(1); // stops it from forecasting current day
            //makes sure date falls within school year
            if ((currentDate.Month < schoolStartMonth && currentDate.Month > schoolEndMonth) ^
                (currentDate.Month == schoolStartMonth && currentDate.Day < schoolStartDay) ^
                (currentDate.Month == schoolEndMonth && currentDate.Day >= schoolEndDay))
            {
                currentDate = new DateTime(currentDate.AddYears(1).Year, schoolStartMonth, schoolStartDay);

            }
        }
        else if (displayMode == WindowGraph.DisplayMode.Month)
        {
            currentDate = currentDate.AddMonths(1); // stops it from forecasting current day
            //makes sure date falls within school year
            if ((currentDate.Month <= schoolStartMonth && currentDate.Month >= schoolEndMonth))
            {
                currentDate = new DateTime(currentDate.AddYears(1).Year, schoolStartMonth - 1, 1);
            }
        }

        foreach (var forecast in forecasts.Forecast)
        {

            string currentDateString;

            //increments day properly
            if (displayMode == WindowGraph.DisplayMode.Day)
            {
                currentDate = currentDate.AddDays(1);
            }
            else if (displayMode == WindowGraph.DisplayMode.Month)
            {
                currentDate = currentDate.AddMonths(1);
            }
            else
            {
                currentDate = currentDate.AddYears(1);
            }
            currentDateString = currentDate.ToString("yyyy-MM-dd");
            //Debug.Log(currentDateString + ", " + Mathf.RoundToInt(forecast));
            newTuples.Add(Tuple.Create(currentDateString, Mathf.RoundToInt(forecast)));

            index++;
        }

        return newTuples;
    }
}

public class TimeSeriesData
{
    [LoadColumn(0)] public DateTime date { get; set; }
    [LoadColumn(1)] public float amountOfChoices { get; set; }

    public TimeSeriesData(DateTime _date, float _amountOfChoices)
    {
        amountOfChoices = _amountOfChoices;
        date = _date;
    }
}

public class TimeSeriesForecast
{
    //vector to hold alert,score,p-value values
    public float[] Forecast { get; set; }
}

