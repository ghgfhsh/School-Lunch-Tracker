using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.EventSystems;

public class WindowGraph : MonoBehaviour
{
    #region Singleton Pattern
    private static WindowGraph _instance;

    public static WindowGraph Instance { get { return _instance; } }


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        loginScreen.SetActive(true);
        gameObject.transform.parent.parent.gameObject.SetActive(false);
        gameObject.transform.parent.parent.gameObject.GetComponent<LunchDataScreen>().enabled = true;
    }
    #endregion


    public GameObject loginScreen;

    [SerializeField] private Sprite dotSprite;
    [SerializeField] private RectTransform graphContainer;
    [SerializeField] private RectTransform labelTemplateX;
    [SerializeField] private RectTransform labelTemplateY;
    [SerializeField] private RectTransform dashTemplateX;
    [SerializeField] private RectTransform dashTemplateY;
    private List<GameObject> gameObjectList;
    [SerializeField] private GameObject tooltipGameObject;

    //cached variables
    private List<int> valueList;
    private IGraphVisual graphVisual;
    private int maxVisibleValueAmount;
    private Func<float, string> getAxisLabelY = null;

    IGraphVisual lineGraphVisual;
    IGraphVisual barChartVisual;

    bool isShowingLineGraph = true;

    public List<string> dateStrings;

    DisplayMode displayMode = DisplayMode.Day;

    private void OnDisable()
    {
        if(graphVisual != null)
        {
            graphVisual.Reset();
        }

        if (gameObjectList != null)
        {
            //Destorys previous graph
            foreach (GameObject o in gameObjectList)
            {
                Destroy(o);
            }
            gameObjectList.Clear();
        }
    }

    public void CreateGraphVisuals(int predictionIndex)
    {
        //graphContainer = transform.Find("graphContainer").GetComponent<RectTransform>();
        //labelTemplateX = graphContainer.Find("labelTemplateX").GetComponent<RectTransform>();
        //labelTemplateY = graphContainer.Find("labelTemplateY").GetComponent<RectTransform>();
        //dashTemplateX = graphContainer.Find("dashTemplateX").GetComponent<RectTransform>();
        //dashTemplateY = graphContainer.Find("dashTemplateY").GetComponent<RectTransform>();
        //tooltipGameObject = graphContainer.Find("tooltip").gameObject;
        gameObjectList = new List<GameObject>();

        Color dataColor = new Color(0.4726771f, 0.8867924f, 0.4782762f, 1);
        Color dotConnectionColor = new Color(1, 1, 1, .5f);
        Color predictionColor = new Color(1, 0.3952115f, 0.3349057f, 1);
        Color dotConnectionPredictionColor = new Color(1f, 0.5424528f, 0.5424528f, .5f);


        lineGraphVisual = new LineGraphVisual(graphContainer, dotSprite, dataColor, dotConnectionColor, predictionColor, dotConnectionPredictionColor, predictionIndex);
        barChartVisual = new BarChartVisual(graphContainer, dataColor, predictionColor, .8f, predictionIndex);

        ShowGraph(valueList, lineGraphVisual, 12, (float _f) => Mathf.RoundToInt(_f).ToString());
    }

    public void LoadGraphData(List<int> data, List<string> dateStrings, int predictionIndex, DisplayMode displayMode)
    {
        this.dateStrings = dateStrings;
        this.displayMode = displayMode;

        lineGraphVisual.UpdatePredictionIndex(predictionIndex);
        barChartVisual.UpdatePredictionIndex(predictionIndex);
        ShowGraph(data, lineGraphVisual, 60, (float _f) => Mathf.RoundToInt(_f).ToString());
    }

    // this is so we don't need an instance reference inside of graph visual
    public static void ShowTooltip_Static(string tooltipText, Vector2 anchoredPosition)
    {
        Instance.ShowTooltip(tooltipText, anchoredPosition);
    }

    private void ShowTooltip(string tooltipText, Vector2 anchoredPosition)
    {
        tooltipGameObject.SetActive(true);

        //get what the back ground size should be based on the text
        tooltipGameObject.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
        TMP_Text tmpText = tooltipGameObject.transform.Find("text").GetComponent<TMP_Text>();
        tmpText.text = tooltipText;
        float textPaddingSize = 4f;
        Vector2 backgroundSize = new Vector2(
            tmpText.preferredWidth + textPaddingSize * 2f, 
            tmpText.preferredHeight + textPaddingSize * 2f
            );
        //set tool tip background size to match text
        tooltipGameObject.transform.Find("background").GetComponent<RectTransform>().sizeDelta = backgroundSize;

        tooltipGameObject.transform.SetAsLastSibling();
    }

    public static void HideToolTip_Static()
    {
        Instance.HideToolTip();
    }

    private void HideToolTip()
    {
        tooltipGameObject.SetActive(false);
    }

    public void LineGraphButtonClicked()
    {
        if (isShowingLineGraph)
            return;

        SetGraphVisual(lineGraphVisual);
        isShowingLineGraph = true;
    }

    public void BarChartButtonClicked()
    {
        if (!isShowingLineGraph)
            return;

        SetGraphVisual(barChartVisual);
        isShowingLineGraph = false;
    }

    public void IncreaseVisibleAmount()
    {
        this.maxVisibleValueAmount += 5;
        SetGraphVisual(graphVisual);
    }

    public void DecreaseVisibleAmount()
    {
        this.maxVisibleValueAmount -= 5;
        SetGraphVisual(graphVisual);
    }

    private void SetGraphVisual(IGraphVisual graphVisual)
    {
        ShowGraph(valueList, graphVisual, maxVisibleValueAmount, getAxisLabelY);
    }

    public string GetXAxisString(int i)
    {
        string year, month, day = "";

        //format the x axis display label and display amount according the the mode, implemented as a call back function when graph is shown
        string[] splitDateString = dateStrings[i].Split('-');


        if(displayMode == DisplayMode.Day)
        {
            year = splitDateString[0];
            month = splitDateString[1];
            day = splitDateString[2];
            return month + "/" + day + "/" + year;
        }
        else if (displayMode == DisplayMode.Month)
        {
            year = splitDateString[0];
            month = splitDateString[1];
            return month + "/" + year;
        }
        else if (displayMode == DisplayMode.Year)
        {
            year = splitDateString[0];
            return year;
        }
        return "";
    }

    public void SetGraphDisplayMode(DisplayMode displayMode)
    {
        this.displayMode = displayMode;
        SetGraphVisual(graphVisual);
    }

    private void ShowGraph(List<int> valueList, IGraphVisual graphVisual, int maxVisibleValueAmount, Func<float, string> getAxisLabelY = null)
    {
        this.valueList = valueList;
        this.graphVisual = graphVisual;
        this.getAxisLabelY = getAxisLabelY;

        if (graphVisual == null || valueList == null)
            return;

        graphVisual.Reset();

        if (maxVisibleValueAmount <= 0)
        {
            // Show all if no amount specified
            maxVisibleValueAmount = valueList.Count;
        }
        else if (maxVisibleValueAmount > valueList.Count)
        {
            // Validate the amount to show the maximum
            maxVisibleValueAmount = valueList.Count;
        }
        this.maxVisibleValueAmount = maxVisibleValueAmount;

        //assign default function if not already assigned
        if (getAxisLabelY == null)
        {
            getAxisLabelY = delegate (float _f) { return Mathf.RoundToInt(_f).ToString(); };
        }

        //Destorys previous graph
        foreach (GameObject o in gameObjectList)
        {
            Destroy(o);
        }
        gameObjectList.Clear();


        //defines graph position and size from container
        float graphWidth = graphContainer.sizeDelta.x;
        float graphHeight = graphContainer.sizeDelta.y;

        //Itendify y Min and Max Values
        float yMaximum = valueList[0];
        float yMinimum = valueList[0];

        for (int i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++)
        {
            int value = valueList[i];
            if (value > yMaximum)
            {
                yMaximum = value;
            }
            if (value < yMinimum)
            {
                yMinimum = value;
            }
        }

        
        float yDifference = yMaximum - yMinimum;
        if(yDifference <= 0)
        {
            yDifference = 0f; //makes sure the minimum y value can't go below 0
        }

        yMaximum = yMaximum + (yDifference * 0.2f); // makes highest value not hug top of graph
        yMaximum = yMaximum + (10 - (yMaximum % 10)); // makes the number divisible by 10
        yMinimum = yMinimum - (yDifference * 0.2f);

        //set max plotted points to max of graph
        float xSize = graphWidth / (maxVisibleValueAmount + 1);

        int xIndex = 0;
        for (int i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++)
        {
            float xPosition = xSize + xIndex * xSize;
            float yPosition = ((valueList[i] - yMinimum) / (yMaximum - yMinimum)) * graphHeight;

            string tooltipText = GetXAxisString(i) + " " + getAxisLabelY(valueList[i]);
            gameObjectList.AddRange(graphVisual.AddGraphVisual(new Vector2(xPosition, yPosition), xSize, tooltipText, i));

            //spawn X axis labels makes sure it doesn't overlap labels by making it only display 13 lables
            int modCheck = Mathf.CeilToInt(maxVisibleValueAmount / 13f);
            if(i % modCheck == 0)
            {
                RectTransform labelX = Instantiate(labelTemplateX);
                labelX.SetParent(graphContainer);
                labelX.gameObject.SetActive(true);
                labelX.anchoredPosition = new Vector2(xPosition, -25f);
                labelX.GetComponent<Text>().text = GetXAxisString(i);
                gameObjectList.Add(labelX.gameObject);


                //spawn x axis dashes
                RectTransform dashX = Instantiate(dashTemplateX);
                dashX.SetParent(graphContainer);
                dashX.SetSiblingIndex(1);
                dashX.gameObject.SetActive(true);
                dashX.anchoredPosition = new Vector2(xPosition, 0f);
                gameObjectList.Add(dashX.gameObject);
            }



            xIndex++;
        }

        //calculate where to put y values and spawn the axis lables and dashes
        int seperatorCount = 10;
        for(int i = 0; i <= seperatorCount; i++)
        {
            //spawns y axis labels
            RectTransform labelY = Instantiate(labelTemplateY);
            labelY.SetParent(graphContainer, false);
            labelY.gameObject.SetActive(true);
            float normalizedValue = i * 1f / seperatorCount;
            labelY.anchoredPosition = new Vector2(-30f, normalizedValue * graphHeight);
            labelY.GetComponent<Text>().text = getAxisLabelY(yMinimum + (normalizedValue * (yMaximum - yMinimum)));
            gameObjectList.Add(labelY.gameObject);


            //spawns y axis dashes
            RectTransform dashY = Instantiate(dashTemplateY);
            dashY.SetParent(graphContainer, false);
            dashY.SetSiblingIndex(1);
            dashY.gameObject.SetActive(true);
            dashY.anchoredPosition = new Vector2(0f, normalizedValue * graphHeight);
            gameObjectList.Add(dashY.gameObject);
        }
    }



    public interface IGraphVisual
    {
        List<GameObject> AddGraphVisual(Vector2 graphPosition, float graphPositionWidth, string tooltipText, int currentIndex);

        void AddTriggerEvent(GameObject o, string tooltipText, Vector2 graphPosition);

        void UpdatePredictionIndex(int _predictionIndex);

        void Reset();
    }


    private class BarChartVisual : IGraphVisual
    {
        private RectTransform graphContainer;
        private Color barColor;
        private Color predictionColor;
        private float barWidthMultiplier;
        private GameObject lastDotGameObject;
        private int predictionIndex;

        public BarChartVisual(RectTransform _graphContainer, Color _barColor, Color _predicitionColor, float _barWidthMultiplier, int _predictionIndex)
        {
            graphContainer = _graphContainer;
            barColor = _barColor;
            predictionColor = _predicitionColor;
            barWidthMultiplier = _barWidthMultiplier;
            lastDotGameObject = null;
            predictionIndex = _predictionIndex;
        }

        public void Reset()
        {
            lastDotGameObject = null;
        }

        public List<GameObject> AddGraphVisual(Vector2 graphPosition, float graphPositionWidth, string tooltipText, int currentIndex)
        {
            GameObject barGameObject = CreateBar(graphPosition, graphPositionWidth, currentIndex);
            AddTriggerEvent(barGameObject, tooltipText, graphPosition);


            return new List<GameObject> { barGameObject };
        }

        public void AddTriggerEvent(GameObject o, string tooltipText, Vector2 graphPosition)
        {
            //create mouse over event that will show the tool tip
            EventTrigger trigger = o.AddComponent<EventTrigger>();
            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => { ShowTooltip_Static(tooltipText, graphPosition); });

            //create mouse exit event that will hide the tool tip
            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => { HideToolTip_Static(); });

            //add events
            trigger.triggers.Add(enterEntry);
            trigger.triggers.Add(exitEntry);
        }

        private GameObject CreateBar(Vector2 graphPosition, float barWidth, int currentIndex)
        {
            //spawns the circle game object where the graph point should be and returns it
            GameObject newGameObject = new GameObject("bar", typeof(Image));
            newGameObject.transform.SetParent(graphContainer, false);

            //change the color to the prediction color
            if (currentIndex >= predictionIndex)
                newGameObject.GetComponent<Image>().color = predictionColor;
            else
                newGameObject.GetComponent<Image>().color = barColor;

            RectTransform rectTransform = newGameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(graphPosition.x, 0f);
            rectTransform.sizeDelta = new Vector2(barWidth * barWidthMultiplier, graphPosition.y);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(.5f, 0f);
            return newGameObject;
        }

        public void UpdatePredictionIndex(int _predictionIndex)
        {
            predictionIndex = _predictionIndex;
        }
    }

    private class LineGraphVisual : IGraphVisual
    {
        private RectTransform graphContainer;
        private Sprite dotSprite;
        private GameObject lastDotGameObject;
        Color dotColor;
        Color dotConnectionColor;
        Color dotPredictionColor;
        Color dotConnectionPredictionColor;
        private int predictionIndex;

        public LineGraphVisual (RectTransform _graphContainer, Sprite _dotSprite, Color _dotColor, Color _dotConnectionColor, Color _dotConnectionPredictionColor, Color _linePreditionColor, int _predictionIndex)
        {
            graphContainer = _graphContainer;
            dotSprite = _dotSprite;
            dotColor = _dotColor;
            dotConnectionColor = _dotConnectionColor;
            lastDotGameObject = null;
            predictionIndex = _predictionIndex;
            dotPredictionColor = _dotConnectionPredictionColor;
            dotConnectionPredictionColor = _linePreditionColor;
        }

        public List<GameObject> AddGraphVisual(Vector2 graphPosition, float graphPositionWidth, string tooltipText, int currentIndex)
        {
            List<GameObject> gameObjectList = new List<GameObject>();
            GameObject dotGameObject = CreateDot(graphPosition, currentIndex);
            AddTriggerEvent(dotGameObject, tooltipText, graphPosition);
            gameObjectList.Add(dotGameObject);
            //once there is two dot connections create a dot connection between them
            if (lastDotGameObject != null)
            {
                GameObject dotConnectionGameObject = CreateDotConnection(lastDotGameObject.GetComponent<RectTransform>().anchoredPosition, dotGameObject.GetComponent<RectTransform>().anchoredPosition, currentIndex);
                gameObjectList.Add(dotConnectionGameObject);
            }
            lastDotGameObject = dotGameObject;
            return gameObjectList;
        }

        public void AddTriggerEvent(GameObject o, string tooltipText, Vector2 graphPosition)
        {
            //create mouse over event that will show the tool tip
            EventTrigger trigger = o.AddComponent<EventTrigger>();
            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => { ShowTooltip_Static(tooltipText, graphPosition); });

            //create mouse exit event that will hide the tool tip
            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => { HideToolTip_Static(); });

            //add events
            trigger.triggers.Add(enterEntry);
            trigger.triggers.Add(exitEntry);
        }


        public void Reset()
        {
            lastDotGameObject = null;
        }

        private GameObject CreateDot(Vector2 anchoredPosition, int currentIndex)
        {
            //spawns the circle game object where the graph point should be and returns it
            GameObject newGameObject = new GameObject("dot", typeof(Image));
            newGameObject.transform.SetParent(graphContainer, false);
            newGameObject.GetComponent<Image>().sprite = dotSprite;

            //change the color to the prediction color
            if (currentIndex >= predictionIndex)
                newGameObject.GetComponent<Image>().color = dotPredictionColor;
            else
                newGameObject.GetComponent<Image>().color = dotColor;

            RectTransform rectTransform = newGameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(11, 11);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            return newGameObject;
        }

        private GameObject CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB, int currentIndex)
        {
            //spawns lines
            GameObject newGameObject = new GameObject("dotConnection", typeof(Image));
            newGameObject.transform.SetParent(graphContainer, false);
            newGameObject.transform.SetSiblingIndex(1);

            //change the color to the prediction color
            if (currentIndex >= predictionIndex)
                newGameObject.GetComponent<Image>().color = dotConnectionPredictionColor;
            else
                newGameObject.GetComponent<Image>().color = dotConnectionColor;

            RectTransform rectTransform = newGameObject.GetComponent<RectTransform>();

            //find direction from first dot to second dot and the distance between them
            Vector2 dir = (dotPositionB - dotPositionA).normalized;
            float distance = Vector2.Distance(dotPositionA, dotPositionB);

            //positions line between both dots and expands them by the distance
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.sizeDelta = new Vector2(distance, 3f);
            rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
            rectTransform.localEulerAngles = new Vector3(0, 0, GetAngleFromVectorFloat(dir));

            return newGameObject;
        }

        private float GetAngleFromVectorFloat(Vector3 dir)
        {
            //uses distance and vector2 direction information to get an angle
            dir = dir.normalized;
            float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (n < 0) n += 360;

            return n;
        }

        public void UpdatePredictionIndex(int _predictionIndex)
        {
            predictionIndex = _predictionIndex;
        }
    }

    public enum DisplayMode { Day, Month, Year}
}
