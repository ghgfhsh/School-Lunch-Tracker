using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.IO;
using System.Data;
using System;

public class DataBase : MonoBehaviour
{
    public User activeUser;

    //sql connections
    string conn;
    IDbConnection dbconn;
    IDbCommand dbcmd;
    string DATABASE_NAME = "/DataBase/schoollunchtracker.s3db";

    #region Singleton Pattern
    private static DataBase _instance;

    public static DataBase Instance { get { return _instance; } }


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
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        Debug.developerConsoleVisible = true;
        ConnectToDB();
    }

    private void ConnectToDB() // makes sure unity correctly loads the table
    {
        string filepath = Application.streamingAssetsPath + DATABASE_NAME;
        conn = "URI=file:" + filepath;
        conn = "Data Source=" + filepath + "; Foreign Keys=false; FailIfMissing=True;";
        Debug.Log("Opening Db at conn: " + conn);
        dbconn = (IDbConnection)new SqliteConnection(conn);
    }

    //logs the user in
    public bool Login(string usernameToCheck, string passwordToCheck)
    {
        //Dictionary<string, User> loginDict = new Dictionary<string, User>();

        dbconn.Open();
        dbcmd = dbconn.CreateCommand();
        dbcmd.CommandText = "SELECT * FROM users WHERE username = '" + usernameToCheck +"'";
        Debug.Log(dbcmd.CommandText);
        IDataReader reader = dbcmd.ExecuteReader();
        if (reader.Read())
        {
            //retrieve info from DB
            string username = reader.GetString(0);
            string password = reader.GetString(1);
            bool isAdmin = reader.GetInt32(2) == 1 ? true : false; //convert stored int to bool
            dbcmd.Dispose();
            dbconn.Close();
            reader.Dispose();

            Debug.Log("Checking: username= " + username + "  password=" + password + "  isAdmin=" + isAdmin);
            if(password == passwordToCheck)
            {
                activeUser = new User(username, password, isAdmin);
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            dbcmd.Dispose();
            dbconn.Close();
            reader.Dispose();
            return false;
        }
        
    }

    //checks password provided with correct one from Database


    #region Methods involving Users

    public void ChangePassword(string newPassword)
    {
        //change password
        dbconn.Open();
        dbcmd = dbconn.CreateCommand();
        dbcmd.CommandText = "UPDATE users SET password = '"+ newPassword + "' WHERE username = '" + activeUser.username +"'";
        Debug.Log(dbcmd.CommandText);
        dbcmd.ExecuteScalar();
        dbcmd.Dispose();
        dbconn.Close();

        //update active user info
        Login(activeUser.username, newPassword);
    }

    public List<User> GetListofUsers()
    {
        List<User> users = new List<User>();

        dbconn.Open();
        dbcmd = dbconn.CreateCommand();
        dbcmd.CommandText = "SELECT * FROM users;";
        Debug.Log(dbcmd.CommandText);
        IDataReader reader = dbcmd.ExecuteReader();
        while (reader.Read())
        {
            //retrieve info from DB
            string username = reader.GetString(0);
            string password = reader.GetString(1);
            bool isAdmin = reader.GetInt32(2) == 1 ? true : false; //convert stored int to bool
            users.Add(new User(username, password, isAdmin));
        }

        dbconn.Close();
        dbcmd.Dispose();
        reader.Dispose();
        return users;
    }



    //DELETEME
    public void PopulateTable()
    {
        for (int i = 1; i < 32; i++)
        {
            int randomValue = UnityEngine.Random.Range(35, 65);
            dbconn.Open();
            dbcmd = dbconn.CreateCommand();

            string dayString = (i < 10) ? ("0" + i): i.ToString(); //makes sure there is a 0 behind single digit numbers in the date
            if(i < 10)
                dayString = "0" + i;

            dbcmd.CommandText = "INSERT INTO studentlunchchoices VALUES (\"2020-01-"+ dayString + "\", 1, " + randomValue + ", \"test\")";
            Debug.Log(dbcmd.CommandText);
            dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            dbconn.Close();
        }

    }

    public bool UpdateUser(string selectedUser, string username, string password, int isAdmin)
    {
        try
        {
            //update existing user
            dbconn.Open();
            dbcmd = dbconn.CreateCommand();
            dbcmd.CommandText = "UPDATE users SET username = '" + username + "', password = '" + password + "', isadmin = " + isAdmin + " WHERE username = '" + selectedUser + "'";
            Debug.Log(dbcmd.CommandText);
            dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            dbconn.Close();
            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    public void CreateNewUser()
    {
        List<User> users = GetListofUsers();
        string username = "New User";
        foreach (User user in users)
        {
            int i = 0;
            while(user.username == username)
            {
                username = username + " " + ++i;
            }
        }
        string password = "password";
        int isAdmin = 0;

        //change password
        dbconn.Open();
        dbcmd = dbconn.CreateCommand();
        dbcmd.CommandText = "INSERT INTO users VALUES('" + username + "','" + password + "'," + isAdmin + ")";
        Debug.Log(dbcmd.CommandText);
        dbcmd.ExecuteScalar();
        dbcmd.Dispose();
        dbconn.Close();
    }

    public void DeleteUser(string username)
    {
        //change password
        dbconn.Open();
        dbcmd = dbconn.CreateCommand();
        dbcmd.CommandText = "DELETE FROM users WHERE username = '" + username + "'";
        Debug.Log(dbcmd.CommandText);
        dbcmd.ExecuteScalar();
        dbcmd.Dispose();
        dbconn.Close();
    }
    #endregion

    public List<LunchChoice> GetLunchChoices()
    {

        //generate list of lunch choices
        List<LunchChoice> lunchChoices = new List<LunchChoice>();

        dbconn.Open();
        dbcmd = dbconn.CreateCommand();
        dbcmd.CommandText = "SELECT * FROM lunchchoices ORDER BY lunch_choice_id;";
        Debug.Log(dbcmd.CommandText);
        IDataReader reader = dbcmd.ExecuteReader();
        while (reader.Read())
        {
            //retrieve info from DB
            int lunchChoiceID = reader.GetInt32(0);
            string lunchChoiceName = reader.GetString(1);
            lunchChoices.Add(new LunchChoice(lunchChoiceID, lunchChoiceName));
        }
        dbconn.Close();
        dbcmd.Dispose();
        reader.Dispose();

        //generate add associated products to the lunch choice
        foreach (LunchChoice lunchChoice in lunchChoices)
        {
            dbconn.Open();
            dbcmd = dbconn.CreateCommand();
            dbcmd.CommandText = "SELECT product_id, product_name FROM(SELECT products.product_id, product_name, lunch_choice_id FROM lunchchoices_products INNER JOIN products ON products.product_id = lunchchoices_products.product_id) WHERE lunch_choice_id = " + lunchChoice.lunchChoiceID + ";";
            Debug.Log(dbcmd.CommandText);
            reader = dbcmd.ExecuteReader();
            while (reader.Read())
            {
                //retrieve info from DB
                int productID = reader.GetInt32(0);
                string productName = reader.GetString(1);
                lunchChoice.products.Add(new Product(productID, productName));
            }
            dbconn.Close();
            dbcmd.Dispose();
            reader.Dispose();
        }

        return lunchChoices;
    }

    public void RemoveProductFromLunchChoice(int lunchChoiceID, int ProductID)
    {
        //change password
        dbconn.Open();
        dbcmd = dbconn.CreateCommand();
        dbcmd.CommandText = "DELETE FROM lunchchoices_products WHERE lunch_choice_id = " + lunchChoiceID + " and product_id = " + ProductID + " AND rowid IN(SELECT rowid from lunchchoices_products WHERE lunch_choice_id = " + lunchChoiceID + " and product_id = " + ProductID + " limit 1);";
        Debug.Log(dbcmd.CommandText);
        dbcmd.ExecuteScalar();
        dbcmd.Dispose();
        dbconn.Close();
    }

    public List<Product> GetProducts()
    {
        //generate list of lunch choices
        List<Product> products = new List<Product>();

        dbconn.Open();
        dbcmd = dbconn.CreateCommand();
        dbcmd.CommandText = "SELECT * FROM products;";
        Debug.Log(dbcmd.CommandText);
        IDataReader reader = dbcmd.ExecuteReader();
        while (reader.Read())
        {
            //retrieve info from DB
            int productID = reader.GetInt32(0);
            string productName = reader.GetString(1);
            products.Add(new Product(productID, productName));
        }
        dbconn.Close();
        dbcmd.Dispose();
        reader.Dispose();
        return products;
    }

    public void AddProductToLunchChoice(int lunchChoiceID, int productID)
    {
        dbconn.Open();
        dbcmd = dbconn.CreateCommand();
        dbcmd.CommandText = "INSERT INTO lunchchoices_products VALUES(" + lunchChoiceID + ", " + productID + ");";
        Debug.Log(dbcmd.CommandText);
        dbcmd.ExecuteScalar();
        dbcmd.Dispose();
        dbconn.Close();
    }

    public void AddProduct(string name)
    {
        dbconn.Open();
        dbcmd = dbconn.CreateCommand();
        dbcmd.CommandText = "INSERT INTO products (product_name) VALUES(\"" + name + "\");";
        Debug.Log(dbcmd.CommandText);
        dbcmd.ExecuteScalar();
        dbcmd.Dispose();
        dbconn.Close();
    }

    public void DeleteProduct(int productID)
    {
        dbconn.Open();
        dbcmd = dbconn.CreateCommand();
        dbcmd.CommandText = "DELETE FROM products WHERE product_id = " + productID + ";";
        Debug.Log(dbcmd.CommandText);
        dbcmd.ExecuteScalar();
        dbcmd.Dispose();
        dbconn.Close();
    }

    public void AddLunchChoice(string name)
    {
        dbconn.Open();
        dbcmd = dbconn.CreateCommand();
        dbcmd.CommandText = "INSERT INTO lunchchoices (lunch_choice_name) VALUES(\"" + name + "\");";
        Debug.Log(dbcmd.CommandText);
        dbcmd.ExecuteScalar();
        dbcmd.Dispose();
        dbconn.Close();
    }

    public void DeleteLunchChoice(int lunchChoiceID)
    {
        dbconn.Open();
        dbcmd = dbconn.CreateCommand();
        dbcmd.CommandText = "DELETE FROM lunchchoices WHERE lunch_choice_id = " + lunchChoiceID + ";";
        Debug.Log(dbcmd.CommandText);
        dbcmd.ExecuteScalar();
        dbcmd.Dispose();
        dbconn.Close();

        dbconn.Open();
        dbcmd = dbconn.CreateCommand();
        dbcmd.CommandText = "DELETE FROM lunchchoices_products WHERE lunch_choice_id = " + lunchChoiceID + ";";
        Debug.Log(dbcmd.CommandText);
        dbcmd.ExecuteScalar();
        dbcmd.Dispose();
        dbconn.Close();
    }

    public void AddStudentLunchChoices(StudentLunchChoice studentLunchChoice)
    {
        dbconn.Open();
        dbcmd = dbconn.CreateCommand();
        dbcmd.CommandText = "INSERT INTO studentlunchchoices VALUES(date(), " + studentLunchChoice.lunchChoice.lunchChoiceID + ", " + studentLunchChoice.amountOfLunches + ", \"" + studentLunchChoice.userAddedBy + "\");";
        Debug.Log(dbcmd.CommandText);
        dbcmd.ExecuteScalar();
        dbcmd.Dispose();
        dbconn.Close();
    }

    public Dictionary<string, int> GetTodaysLunchChoices()
    {
        Dictionary<string, int> lunchChoices = new Dictionary<string, int>();


        dbconn.Open();
        dbcmd = dbconn.CreateCommand();
        dbcmd.CommandText = "SELECT lunch_choice_name, SUM(amount_of_lunches) FROM lunchchoices a, studentlunchchoices b WHERE a.lunch_choice_id = b.lunch_choice_id AND b.date = date() GROUP BY(lunch_choice_name);";
        Debug.Log(dbcmd.CommandText);
        IDataReader reader = dbcmd.ExecuteReader();
        while (reader.Read())
        {
            string name = reader.GetString(0);
            int amount = reader.GetInt32(1);
            lunchChoices.Add(name, amount);
        }
        dbconn.Close();
        dbcmd.Dispose();
        reader.Dispose();
        return lunchChoices;
    }

    public List<Tuple<string, int>> GetOrderInformation(int selectedID, WindowGraph.DisplayMode displayMode, bool isProduct)
    {
        List<Tuple<string, int>> studentChoices = new List<Tuple<string, int>>();

        //switches out the group by sql statement to group the sum of the days by the display mode
        string returnMode = "%Y-%m-%d";
        switch (displayMode)
        {
            case WindowGraph.DisplayMode.Day:
                returnMode = "%Y-%m-%d";
                break;
            case WindowGraph.DisplayMode.Month:
                returnMode = "%Y-%m";
                break;
            case WindowGraph.DisplayMode.Year:
                returnMode = "%Y";
                break;
        }

        dbconn.Open();
        dbcmd = dbconn.CreateCommand();


        //switch between grabbing product and lunchchoice information
        if (!isProduct)
        {
            dbcmd.CommandText = "SELECT * FROM (( SELECT strftime('" + returnMode + "', date), SUM(amount_of_lunches), lunch_choice_id " +
                "FROM studentlunchchoices WHERE lunch_choice_id = " + selectedID + " GROUP BY strftime('" + returnMode + "', date)) INNER JOIN ( SELECT " +
                "lunch_choice_id, lunch_choice_name FROM lunchchoices WHERE lunch_choice_id = " + selectedID +
                ") USING(lunch_choice_id));";
        }
        else
        {
            dbcmd.CommandText = "SELECT date, lunches, product_id FROM(SELECT * " +
                "FROM products INNER JOIN lunchchoices_products USING (product_id) " +
                "WHERE product_id = "+ selectedID + ") INNER JOIN (SELECT strftime('" + returnMode + "', date) " +
                "date, lunch_choice_id, SUM(amount_of_lunches) lunches FROM studentlunchchoices " +
                "GROUP BY strftime('" + returnMode + "', date), lunch_choice_id) USING (lunch_choice_id) " +
                "INNER JOIN products USING(product_id)";
        }


        Debug.Log(dbcmd.CommandText);
        IDataReader reader = dbcmd.ExecuteReader();

        while (reader.Read())
        {
            string date = reader.GetString(0);
            int amount = reader.GetInt32(1);
            studentChoices.Add(Tuple.Create(date, amount));
        }

        dbconn.Close();
        dbcmd.Dispose();
        reader.Dispose();
        return studentChoices;
    }

    public void AddListToStudentLunchChoices(List<Tuple<string, int>> dataToAdd, int lunchChoiceID)
    {
        foreach(var data in dataToAdd)
        {
            dbconn.Open();
            dbcmd = dbconn.CreateCommand();
            dbcmd.CommandText = "INSERT INTO studentlunchchoices VALUES (strftime(\"%Y-%m-%d\", \"" + data.Item1 + "\"), " + lunchChoiceID + ", " + data.Item2 + ", \"admin\")";
            Debug.Log(dbcmd.CommandText);
            dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            dbconn.Close();
        }
    }
}

public class User
{
    public string username;
    private string password;
    public bool isAdmin;

    public User(string _userName, string _password, bool _isAdmin)
    {
        username = _userName;
        isAdmin = _isAdmin;
        password = _password;
    }

    public bool CheckPassword(string passwordToCheck)
    {
        if (passwordToCheck == password)
            return true;
        else
            return false;
    }
}

public class LunchChoice
{
    public int lunchChoiceID;
    public string lunchChoiceName;
    public List<Product> products;

    public LunchChoice(int _lunchChoiceID, string _lunchChoiceName)
    {
        lunchChoiceID = _lunchChoiceID;
        lunchChoiceName = _lunchChoiceName;
        products = new List<Product>();
    }
}

public class Product
{
    public int productId;
    public string productName;

    public Product(int _productId, string _productName)
    {
        productId = _productId;
        productName = _productName;
    }
}

public class StudentLunchChoice
{
    public LunchChoice lunchChoice;
    public string userAddedBy;
    public int amountOfLunches;
    public string date;

    public StudentLunchChoice(LunchChoice _lunchChoice, string _userAddedby, int _amountOfLunches)
    {
        lunchChoice = _lunchChoice;
        userAddedBy = _userAddedby;
        amountOfLunches = _amountOfLunches;
        date = null;
    }

    public StudentLunchChoice(LunchChoice _lunchChoice, int _amountOfLunches, string _date)
    {
        lunchChoice = _lunchChoice;
        userAddedBy = null;
        amountOfLunches = _amountOfLunches;
        date = _date;
    }
}