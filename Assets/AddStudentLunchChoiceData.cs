using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AddStudentLunchChoiceData : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(AddDataToDatabase("C:/Users/ghgfh/Downloads/Generated Data/Lunch Choices/Chicken Patty.csv", 1));
        StartCoroutine(AddDataToDatabase("C:/Users/ghgfh/Downloads/Generated Data/Lunch Choices/Hamburger.csv", 3));
        StartCoroutine(AddDataToDatabase("C:/Users/ghgfh/Downloads/Generated Data/Lunch Choices/Ham Meal.csv", 4));

        //ReduceGeneratedData(2, "C:/Users/ghgfh/Downloads/Generated Data/Total Students Generated.csv");
        //GenerateRandomOrderAmounts(.4f, "Chicken Patty.csv");
        //GenerateRandomOrderAmounts(.32f, "Hamburger.csv");
        //GenerateRandomOrderAmounts(.28f, "Ham Meal.csv");
    }

    public IEnumerator AddDataToDatabase(string filePath, int lunchChoiceID)
    {
        List<Tuple<string, int>> lunchChoices = new List<Tuple<string, int>>();


        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var values = line.Split(',');
                string date = values[0];
                int orderAmount = int.Parse(values[1]);
                lunchChoices.Add(Tuple.Create(date, orderAmount));
            }
        }

        DataBase.Instance.AddListToStudentLunchChoices(lunchChoices, lunchChoiceID);
        yield return null;
    }

    private void GenerateRandomOrderAmounts(float percentageOfChoices, string outputFileName)
    {
        string outputFilePath = "C:/Users/ghgfh/Downloads/Generated Data/Lunch Choices/" + outputFileName;
        List<Tuple<string, int>> studentsAtSchool = new List<Tuple<string, int>>();
        const float varianceOfChoices = 0.26f;

        using (StreamReader reader = new StreamReader("C:/Users/ghgfh/Downloads/Generated Data/ReducedData/Total Students Reduced.csv"))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var values = line.Split(',');
                string date = values[0];
                int orderAmount = int.Parse(values[1]);
                studentsAtSchool.Add(Tuple.Create(date, orderAmount));
            }
        }

        List<Tuple<string, int>> lunchChoiceAmounts = new List<Tuple<string, int>>();
        System.Random rand = new System.Random();
        foreach (var value in studentsAtSchool)
        {
            int amountOfLunchesMedian = Mathf.RoundToInt(value.Item2 * percentageOfChoices);
            int minimum = Mathf.RoundToInt(amountOfLunchesMedian - (varianceOfChoices * amountOfLunchesMedian));
            int maximum = Mathf.RoundToInt(amountOfLunchesMedian + (varianceOfChoices * amountOfLunchesMedian));
            int newRandomNumber = rand.Next(minimum, maximum);
            lunchChoiceAmounts.Add(Tuple.Create(value.Item1, newRandomNumber));
        }

        //write data to new csv
        using (StreamWriter writer = new StreamWriter(outputFilePath, false))
        {
            foreach (var value in lunchChoiceAmounts)
            {
                DateTime date = DateTime.ParseExact(value.Item1, "MM/dd/yy", System.Globalization.CultureInfo.InvariantCulture);
                writer.WriteLine(date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) + ", " + value.Item2);
            }
        }
    }

    //reduces the number of enties in a csv by a factor of numOutput reads from column 2
    private void ReduceGeneratedData(int numOutput, string filepath)
    {
        List<int> dataConvertedtoInt = new List<int>();

        using (StreamReader reader = new StreamReader(filepath))
        {

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var values = line.Split(',');
                dataConvertedtoInt.Add(Mathf.RoundToInt(float.Parse(values[1]))); //reads in generated value and converts it to ints
            }
        }

        //this is two loops, one stores the last 3 numbers, the other stores the avg of those numbers into a new array
        List<int> reducedData = new List<int>();
        int count = 0;
        int valuesAdded = 0;
        for(int i = 0; i < dataConvertedtoInt.Count; i++)
        {
            if(count < numOutput)
            {
                valuesAdded += dataConvertedtoInt[i];
                count++;
            }
            else
            {
                int newNumber = valuesAdded / numOutput; // get the avg of the values
                reducedData.Add(newNumber);
                count = 0;
                valuesAdded = 0;
            }
        }

        //write data to new csv
        StreamWriter writer = new StreamWriter("C:/Users/ghgfh/Downloads/Generated Data/ReducedData/", false);

        writer.WriteLine("Lunch Choice,Amount of Lunches");
        foreach (int value in reducedData)
        {
            writer.WriteLine(value);
        }
        writer.Close();

    }

    private int[] GenereateRandomKValuesWithSum(int amountOfNumbers, int sumOfNumbers, int avg, float range)
    {
        System.Random rnd = new System.Random();
        int[] x = new int[amountOfNumbers];

        // the endpoints of the interval
        x[0] = 0;

        int testSumofNumbers = 0;

        // generate the k - 1 random sectioning points
        for (int i = 0; i < amountOfNumbers; i++)
        {
            x[i] = rnd.Next(Mathf.RoundToInt(avg * range), Mathf.RoundToInt(avg + (1f - range)));
            testSumofNumbers += x[i];
        }
        int[] N = new int[amountOfNumbers];
        for (int i = 0; i < amountOfNumbers; i++)
        {
            if(x[i] < N[i])
            {
                //DO LATER
            }
        }

        return N;
    }
}
