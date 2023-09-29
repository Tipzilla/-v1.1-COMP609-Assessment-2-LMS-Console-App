using System.Data.OleDb;
using System.Configuration;
using System.Data;
using Figgle;
using System.Diagnostics;

namespace COMP609_Assessment_2_LMS_Console_App
{
    internal class Program
    {
        static void PrintLivestockDataHeader()
        {
            Console.WriteLine("{0,-8}{1,-8}{2,-8}{3,-8}{4,-8}{5,-8}{6,-8}", "Type", "ID", "Water", "Cost", "Weight", "Colour", "Milk/Wool");
        }
        static void PrintCommodityDataHeader()
        {
            Console.WriteLine("{0,-18}{1,-8}", "Item", "Price");
        }
        static void PrintLineBreak()
        {
            int consoleWidth = Console.WindowWidth;
            string dashes = new string('-', consoleWidth);
            Console.Write(dashes);
        }
        static void PrintCowLivestockData(OleDbConnection connection)
        {
            string query = "SELECT * FROM Cow";

            OleDbCommand command = new OleDbCommand(query, connection);
            OleDbDataAdapter adapter = new OleDbDataAdapter(command);
            DataSet dataSet = new DataSet();
            adapter.Fill(dataSet);

            using (OleDbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = Convert.ToInt32(reader["ID"]);
                    double water = Convert.ToDouble(reader["Water"]);
                    double cost = Convert.ToDouble(reader["Cost"]);
                    double weight = Convert.ToDouble(reader["Weight"]);
                    string colour = reader["Colour"].ToString();
                    double cowMilk = Convert.ToDouble(reader["Milk"]);

                    Console.WriteLine("{0,-8}{1,-8}{2,-8}{3,-8}{4,-8}{5,-8}{6,-8}", "Cow", id, water, cost, weight, colour, cowMilk);
                }
            }
        }
        static void PrintGoatLivestockData(OleDbConnection connection)
        {
            string query = "SELECT * FROM Goat";

            OleDbCommand command = new OleDbCommand(query, connection);
            OleDbDataAdapter adapter = new OleDbDataAdapter(command);
            DataSet dataSet = new DataSet();
            adapter.Fill(dataSet);
            using (OleDbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = Convert.ToInt32(reader["ID"]);
                    double water = Convert.ToDouble(reader["Water"]);
                    double cost = Convert.ToDouble(reader["Cost"]);
                    double weight = Convert.ToDouble(reader["Weight"]);
                    string colour = reader["Colour"].ToString();
                    double goatMilk = Convert.ToDouble(reader["Milk"]);

                    Console.WriteLine("{0,-8}{1,-8}{2,-8}{3,-8}{4,-8}{5,-8}{6,-8}", "Goat", id, water, cost, weight, colour, goatMilk);
                }
            }
        }
        static void PrintSheepLivestockData(OleDbConnection connection)
        {
            string query = "SELECT * FROM Sheep";

            OleDbCommand command = new OleDbCommand(query, connection);
            OleDbDataAdapter adapter = new OleDbDataAdapter(command);
            DataSet dataSet = new DataSet();
            adapter.Fill(dataSet);
            using (OleDbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = Convert.ToInt32(reader["ID"]);
                    double water = Convert.ToDouble(reader["Water"]);
                    double cost = Convert.ToDouble(reader["Cost"]);
                    double weight = Convert.ToDouble(reader["Weight"]);
                    string colour = reader["Colour"].ToString();
                    double sheepWool = Convert.ToDouble(reader["Wool"]);

                    Console.WriteLine("{0,-8}{1,-8}{2,-8}{3,-8}{4,-8}{5,-8}{6,-8}", "Sheep", id, water, cost, weight, colour, sheepWool);
                }
            }
        }
        static void PrintCommodityData(OleDbConnection connection)
        {
            string query = "SELECT * FROM Commodity";

            OleDbCommand command = new OleDbCommand(query, connection);
            OleDbDataAdapter adapter = new OleDbDataAdapter(command);
            DataSet dataSet = new DataSet();
            adapter.Fill(dataSet);
            using (OleDbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string item = Convert.ToString(reader["Item"]);
                    double price = Convert.ToDouble(reader["Price"]);

                    Console.WriteLine("{0,-18}{1,-8}", item, price);
                }
            }
        }
        static bool IsIDValid(string selectedID, OleDbConnection connection)
        {
            using (OleDbCommand cmd = connection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;

                cmd.CommandText = "SELECT 'Cow' AS TableName, ID FROM Cow WHERE ID = ? " +
                                 "UNION ALL SELECT 'Goat' AS TableName, ID FROM Goat WHERE ID = ? " +
                                 "UNION ALL SELECT 'Sheep' AS TableName, ID FROM Sheep WHERE ID = ?";

                cmd.Parameters.AddWithValue("@ID1", selectedID);
                cmd.Parameters.AddWithValue("@ID2", selectedID);
                cmd.Parameters.AddWithValue("@ID3", selectedID);

                object result = cmd.ExecuteScalar();

                return result != null;
            }
        }
        static int GetNextLivestockID(OleDbConnection connection)
        {
            int nextID = 1; // Default to 1 if there are no existing records

            // Query the database to get the maximum livestock ID
            string query = "SELECT MAX(ID) FROM (" +
                           "SELECT ID FROM Cow " +
                           "UNION " +
                           "SELECT ID FROM Goat " +
                           "UNION " +
                           "SELECT ID FROM Sheep) AS AllIDs";

            using (OleDbCommand command = new OleDbCommand(query, connection))
            {
                object result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    int maxID = Convert.ToInt32(result);

                    // Check for gaps in the ID sequence
                    for (int i = 1; i <= maxID; i++)
                    {
                        string checkQuery = "SELECT COUNT(*) FROM (" +
                                           "SELECT ID FROM Cow " +
                                           "UNION " +
                                           "SELECT ID FROM Goat " +
                                           "UNION " +
                                           "SELECT ID FROM Sheep) AS AllIDs " +
                                           "WHERE ID = ?";
                        using (OleDbCommand checkCommand = new OleDbCommand(checkQuery, connection))
                        {
                            checkCommand.Parameters.AddWithValue("@ID", i);
                            int count = Convert.ToInt32(checkCommand.ExecuteScalar());
                            if (count == 0)
                            {
                                // Found a gap, use this ID
                                nextID = i;
                                break;
                            }
                        }
                    }

                    // If no gaps were found, use the next available ID
                    if (nextID == maxID)
                    {
                        nextID = maxID + 1;
                    }
                }
            }

            return nextID;
        }
        private static string SelectedIDLivestockType(string id, OleDbConnection connection)
        {
            string idCheckQuery = "SELECT 'Cow' AS TableName, ID FROM Cow WHERE ID = ? " +
                                 "UNION ALL SELECT 'Goat' AS TableName, ID FROM Goat WHERE ID = ? " +
                                 "UNION ALL SELECT 'Sheep' AS TableName, ID FROM Sheep WHERE ID = ?";

            using (OleDbCommand idCheckCommand = new OleDbCommand(idCheckQuery, connection))
            {
                idCheckCommand.Parameters.AddWithValue("@ID1", id);
                idCheckCommand.Parameters.AddWithValue("@ID2", id);
                idCheckCommand.Parameters.AddWithValue("@ID3", id);

                using (OleDbDataReader reader = idCheckCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {

                        return reader.GetString(0);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
        static bool IsColourValid(string colour, OleDbConnection connection)
        {
            string colourCheckQuery = "SELECT COUNT(*) FROM (SELECT Colour FROM Cow UNION ALL " +
                                     "SELECT Colour FROM Goat UNION ALL " +
                                     "SELECT Colour FROM Sheep) AS LivestockColors " +
                                     "WHERE Colour = ?";
            using (OleDbCommand colorCheckCommand = new OleDbCommand(colourCheckQuery, connection))
            {
                colorCheckCommand.Parameters.AddWithValue("@Colour", colour);
                int colourCount = Convert.ToInt32(colorCheckCommand.ExecuteScalar());
                return colourCount > 0;
            }
        }
        static bool IsEnteredColorValid(string input)
        {
            try
            {
                System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(input);

                return true;
            }
            catch
            {
                return false;
            }
        }
        static bool IsWeightValid(string weightThreshold)
        {
            if (string.IsNullOrWhiteSpace(weightThreshold))
            {
                return false;
            }

            if (double.TryParse(weightThreshold, out double result))
            {
                return true;
            }

            return false;
        }
        static bool RestoreDatabase(string currentDatabasePath, string backupDatabasePath)
        {
            try
            {
                string tempCopyPath = "TempCopy.accdb";
                File.Copy(backupDatabasePath, tempCopyPath, true);

                File.Copy(tempCopyPath, currentDatabasePath, true);

                File.Delete(tempCopyPath);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restoring database: {ex.Message}");
                return false;
            }
        }
        static double GetDoubleValue(object value)
        {
            if (value != null && value != DBNull.Value)
            {
                double result;
                if (double.TryParse(value.ToString(), out result))
                {
                    return result;
                }
                else
                {
                    Console.WriteLine($"Failed to convert {value.GetType().Name} to double.");
                }
            }
            return 0.0;
        }
        static double GetCommodityPrice(OleDbCommand cmd, string item)
        {
            cmd.Parameters.Clear();
            cmd.CommandText = "SELECT Price FROM Commodity WHERE Item = ?";
            cmd.Parameters.AddWithValue("@Item", item);
            object priceObj = cmd.ExecuteScalar();
            return GetDoubleValue(priceObj);
        }
        static void LivestockStatisticsOperation()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["FarmDataConnectionString"].ConnectionString;
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                OleDbCommand cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.Text;

                cmd.CommandText = "SELECT SUM(Weight) AS totalWeight FROM (SELECT Weight FROM Cow UNION ALL SELECT Weight FROM Goat UNION ALL SELECT Weight FROM Sheep) AS AllLivestock";
                object totalWeightObj = cmd.ExecuteScalar();
                double totalWeight = GetDoubleValue(totalWeightObj);

                double livestockWeightTax = GetCommodityPrice(cmd, "LivestockWeightTax");

                double totalMonthlyWeightTax = (totalWeight * livestockWeightTax) * 30;

                cmd.Parameters.Clear();
                cmd.CommandText = "SELECT SUM(TotalIncome) AS totalIncome FROM (" +
                                  "  SELECT (SUM(Milk) * (SELECT Price FROM Commodity WHERE Item = 'CowMilk')) AS TotalIncome FROM Cow UNION ALL " +
                                  "  SELECT (SUM(Milk) * (SELECT Price FROM Commodity WHERE Item = 'GoatMilk')) AS TotalIncome FROM Goat UNION ALL " +
                                  "  SELECT (SUM(Wool) * (SELECT Price FROM Commodity WHERE Item = 'SheepWool')) AS TotalIncome FROM Sheep) AS AllIncome";

                object totalIncomeObj = cmd.ExecuteScalar();
                double totalIncome = GetDoubleValue(totalIncomeObj);

                double waterPrice = GetCommodityPrice(cmd, "Water");

                cmd.Parameters.Clear();
                cmd.CommandText = $@"SELECT SUM(Cost +(Water * {waterPrice}) +(Weight * {livestockWeightTax})) AS totalCowCost FROM Cow";
                object totalCowCostObj = cmd.ExecuteScalar();
                double totalCowCost = GetDoubleValue(totalCowCostObj);

                cmd.Parameters.Clear();
                cmd.CommandText = $@"SELECT SUM(Cost +(Water * {waterPrice}) +(Weight * {livestockWeightTax})) AS totalGoatCost FROM Goat";
                object totalGoatCostObj = cmd.ExecuteScalar();
                double totalGoatCost = GetDoubleValue(totalGoatCostObj);

                cmd.Parameters.Clear();
                cmd.CommandText = $@"SELECT SUM(Cost +(Water * {waterPrice}) +(Weight * {livestockWeightTax})) AS totalSheepCost FROM Sheep";
                object totalSheepCostObj = cmd.ExecuteScalar();
                double totalSheepCost = GetDoubleValue(totalSheepCostObj);

                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.Text;

                cmd.CommandText = "SELECT AVG(Weight) AS AvgWeight " +
                                  "FROM (SELECT Weight FROM Cow UNION ALL " +
                                  "      SELECT Weight FROM Goat UNION ALL " +
                                  "      SELECT Weight FROM Sheep) AS LivestockWeight ";

                object avgWeightObj = cmd.ExecuteScalar();

                double avgWeight = Convert.ToDouble(avgWeightObj);

                double totalCost = totalCowCost + totalGoatCost + totalSheepCost;

                double totalProfitLoss = totalIncome - totalCost;

                if (totalProfitLoss > 0)
                {
                    Console.WriteLine($"Total monthly tax for all animals: {totalMonthlyWeightTax:C}\n" +
                                      $"Total income for all animals: {totalIncome:C}\n" +
                                      $"Total cost for all animals: {totalCost:C}\n" +
                                      $"Average weight of livestock: {avgWeight:F2}kg\n\n" +
                                      $"Profit: ${totalProfitLoss.ToString("F2")}");
                }
                else if (totalProfitLoss < 0)
                {
                    Console.WriteLine($"Total monthly tax for all animals: {totalMonthlyWeightTax:C}\n" +
                                      $"Total income for all animals: {totalIncome:C}\n" +
                                      $"Total cost for all animals: {totalCost:C}\n" +
                                      $"Average weight of livestock: {avgWeight:F2}kg\n\n" +
                                      $"Loss: ${totalProfitLoss.ToString("F2")}");

                }
            }
        }
        static void Main(string[] args)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["FarmDataConnectionString"].ConnectionString;
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string titleText = "LMS Application";
                    string asciiArt = FiggleFonts.Standard.Render(titleText);
                    Console.Write(asciiArt + "v1.1\n" + "By Hamish Getty\n\n");

                    Console.Write("This application can:\n" +
                                  "- Read data from a database file\n" +
                                  "- Insert, Update, and Delete data\n" +
                                  "- Query data and display statistics\n" +
                                  "- Backup and restore data\n\n");

                    Console.Write("Press any key to get started: ");

                    Console.ReadKey();

                    Console.Clear();

                    while (true)
                    {
                        PrintLineBreak();

                        Console.Write("1. View data\n" +
                                      "2. Modify data\n" +
                                      "3. Query data\n" +
                                      "4. Backup data\n" +
                                      "5. Exit\n");

                        Console.Write("Select an option: ");

                        string userInput_MainMenu = Console.ReadLine();

                        if (userInput_MainMenu == "1")
                        {
                            while (true)
                            {
                                PrintLineBreak();

                                Console.Write("1. View All Livestock Data\n" +
                                              "2. View Single Livestock Data\n" +
                                              "3. View Commodity Data\n" +
                                              "4. Main Menu\n");

                                Console.Write("Select an option: ");

                                string userInput_ViewData = Console.ReadLine();

                                switch (userInput_ViewData)
                                {
                                    case "1":
                                        PrintLivestockDataHeader();

                                        PrintCowLivestockData(connection);

                                        PrintGoatLivestockData(connection);

                                        PrintSheepLivestockData(connection);

                                        break;
                                    case "2":
                                        while (true)
                                        {
                                            Console.Write("Enter livestock ID: ");
                                            string userInput_ViewID = Console.ReadLine();

                                            if (IsIDValid(userInput_ViewID, connection) && int.TryParse(userInput_ViewID, out int userInput_ViewIDValid))
                                            {
                                                string tableName = SelectedIDLivestockType(userInput_ViewID, connection);

                                                string selectSql = $"SELECT '{tableName}' AS Type, * FROM {tableName} WHERE ID = ?";

                                                using (OleDbCommand selectCommand = new OleDbCommand(selectSql, connection))
                                                {
                                                    selectCommand.Parameters.AddWithValue("@ID", userInput_ViewID);

                                                    using (OleDbDataReader reader = selectCommand.ExecuteReader())
                                                    {
                                                        if (reader.HasRows)
                                                        {
                                                            for (int i = 0; i < reader.FieldCount; i++)
                                                            {
                                                                Console.Write(reader.GetName(i) + "\t");
                                                            }
                                                            Console.WriteLine();

                                                            while (reader.Read())
                                                            {
                                                                for (int i = 0; i < reader.FieldCount; i++)
                                                                {
                                                                    Console.Write(reader[i] + "\t");
                                                                }
                                                                Console.WriteLine();
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                        break;
                                    case "3":
                                        PrintCommodityDataHeader();

                                        PrintCommodityData(connection);

                                        break;
                                    case "4":
                                        break;
                                    default:
                                        Console.WriteLine("Invalid choice. Please try again.");
                                        break;
                                }
                                if (userInput_ViewData == "3")
                                {
                                    break;
                                }
                                break;
                            }
                        }
                        else if (userInput_MainMenu == "2")
                        {
                            while (true)
                            {
                                PrintLineBreak();

                                Console.Write("1. Insert Livestock Data\n" +
                                              "2. Update Livestock Data\n" +
                                              "3. Delete Livestock Data\n" +
                                              "4. Update Commodity Data\n" +
                                              "5. Main Menu\n");

                                Console.Write("Select an option: ");

                                string userInput_ModifyData = Console.ReadLine();

                                switch (userInput_ModifyData)
                                {
                                    case "1":
                                        PrintLineBreak();

                                        double userInput_InsertWaterValid;
                                        double userInput_InsertCostValid;
                                        double userInput_InsertWeightValid;
                                        double userInput_InsertMilkValid;
                                        double userInput_InsertWoolValid;

                                        while (true)
                                        {
                                            Console.Write("Enter livestock type (Cow/Goat/Sheep): ");
                                            string userInput_InsertType = Console.ReadLine();

                                            string lowerCaseInput_InsertType = userInput_InsertType.ToLower();

                                            if (lowerCaseInput_InsertType == "cow" || lowerCaseInput_InsertType == "goat" || lowerCaseInput_InsertType == "sheep")
                                            {
                                                string userInput_InsertType2 = char.ToUpper(lowerCaseInput_InsertType[0]) + lowerCaseInput_InsertType.Substring(1);

                                                while (true)
                                                {
                                                    int userInput_InsertIDValid = GetNextLivestockID(connection);

                                                    while (true)
                                                    {
                                                        Console.Write("Enter water: ");
                                                        string userInput_InsertWater = Console.ReadLine();

                                                        if (double.TryParse(userInput_InsertWater, out userInput_InsertWaterValid))
                                                        {
                                                            while (true)
                                                            {
                                                                Console.Write("Enter cost: ");
                                                                string userInput_InsertCost = Console.ReadLine();

                                                                if (double.TryParse(userInput_InsertCost, out userInput_InsertCostValid))
                                                                {
                                                                    while (true)
                                                                    {
                                                                        Console.Write("Enter weight: ");
                                                                        string userInput_InsertWeight = Console.ReadLine();

                                                                        if (double.TryParse(userInput_InsertWeight, out userInput_InsertWeightValid))
                                                                        {
                                                                            while (true)
                                                                            {
                                                                                Console.Write("Enter colour: ");
                                                                                string userInput_InsertColour = Console.ReadLine();

                                                                                string lowerCaseInput_InsertColour = userInput_InsertColour.ToLower();

                                                                                if (IsEnteredColorValid(lowerCaseInput_InsertColour) && !lowerCaseInput_InsertColour.Any(char.IsDigit) && !string.IsNullOrEmpty(lowerCaseInput_InsertColour))
                                                                                {
                                                                                    string userInput_InsertColour2 = char.ToUpper(lowerCaseInput_InsertColour[0]) + lowerCaseInput_InsertColour.Substring(1);

                                                                                    if (userInput_InsertType2 == "Cow" || userInput_InsertType2 == "Goat")
                                                                                    {
                                                                                        while (true)
                                                                                        {
                                                                                            Console.Write("Enter milk: ");
                                                                                            string userInput_InsertMilk = Console.ReadLine();

                                                                                            if (double.TryParse(userInput_InsertMilk, out userInput_InsertMilkValid))
                                                                                            {
                                                                                                PrintLineBreak();

                                                                                                PrintLivestockDataHeader();

                                                                                                Console.WriteLine("{0,-8}{1,-8}{2,-8}{3,-8}{4,-8}{5,-8}{6,-8}", userInput_InsertType2, userInput_InsertIDValid, userInput_InsertWaterValid, userInput_InsertCostValid, userInput_InsertWeightValid, userInput_InsertColour2, userInput_InsertMilkValid);

                                                                                                while (true)
                                                                                                {
                                                                                                    Console.Write("Are you sure you want to insert this record? Y/N: ");

                                                                                                    string userInput_InsertConfirm = Console.ReadLine();

                                                                                                    string lowerCaseInput_InsertConfirm = userInput_InsertConfirm.ToLower();

                                                                                                    if (lowerCaseInput_InsertConfirm == "y")
                                                                                                    {
                                                                                                        string insertSql = $"INSERT INTO {userInput_InsertType2} (ID, Water, Cost, Weight, Colour, Milk) " +
                                                                                                                            "VALUES (@IdValue, @WaterValue, @CostValue, @WeightValue, @ColourValue, @ProduceValue)";


                                                                                                        using (OleDbCommand command = new OleDbCommand(insertSql, connection))
                                                                                                        {
                                                                                                            command.Parameters.AddWithValue("@IdValue", userInput_InsertIDValid);
                                                                                                            command.Parameters.AddWithValue("@WaterValue", userInput_InsertWaterValid);
                                                                                                            command.Parameters.AddWithValue("@CostValue", userInput_InsertCostValid);
                                                                                                            command.Parameters.AddWithValue("@WeightValue", userInput_InsertWeightValid);
                                                                                                            command.Parameters.AddWithValue("@ColourValue", userInput_InsertColour2);
                                                                                                            command.Parameters.AddWithValue("@ProduceValue", userInput_InsertMilkValid);
                                                                                                            try
                                                                                                            {
                                                                                                                int rowsAffected = command.ExecuteNonQuery();
                                                                                                                Console.WriteLine($"{rowsAffected} row(s) inserted successfully.");
                                                                                                                break;
                                                                                                            }
                                                                                                            catch (Exception ex)
                                                                                                            {
                                                                                                                Console.WriteLine("Error: " + ex.Message);
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                    else
                                                                                                    {
                                                                                                        Console.WriteLine("Record insert cancelled.");
                                                                                                        break;
                                                                                                    }
                                                                                                }
                                                                                                break;
                                                                                            }
                                                                                            else
                                                                                            {
                                                                                                Console.Write("Invalid milk amount. Please enter a valid milk amount.\n");
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                    else if (userInput_InsertType2 == "Sheep")
                                                                                    {
                                                                                        while (true)
                                                                                        {
                                                                                            Console.Write("Enter wool: ");
                                                                                            string userInput_InsertWool = Console.ReadLine();

                                                                                            if (double.TryParse(userInput_InsertWool, out userInput_InsertWoolValid))
                                                                                            {
                                                                                                PrintLineBreak();

                                                                                                PrintLivestockDataHeader();

                                                                                                Console.WriteLine("{0,-8}{1,-8}{2,-8}{3,-8}{4,-8}{5,-8}{6,-8}", userInput_InsertType2, userInput_InsertIDValid, userInput_InsertWaterValid, userInput_InsertCostValid, userInput_InsertWeightValid, userInput_InsertColour2, userInput_InsertWoolValid);

                                                                                                while (true)
                                                                                                {
                                                                                                    Console.Write("Are you sure you want to insert this record? Y/N: ");

                                                                                                    string userInput_InsertConfirm = Console.ReadLine();

                                                                                                    string lowerCaseInput_InsertConfirm = userInput_InsertConfirm.ToLower();

                                                                                                    if (lowerCaseInput_InsertConfirm == "y")
                                                                                                    {
                                                                                                        string insertSql = $"INSERT INTO {userInput_InsertType2} (ID, Water, Cost, Weight, Colour, Wool) " +
                                                                                                                            "VALUES (@IdValue, @WaterValue, @CostValue, @WeightValue, @ColourValue, @ProduceValue)";


                                                                                                        using (OleDbCommand command = new OleDbCommand(insertSql, connection))
                                                                                                        {
                                                                                                            command.Parameters.AddWithValue("@IdValue", userInput_InsertIDValid);
                                                                                                            command.Parameters.AddWithValue("@WaterValue", userInput_InsertWaterValid);
                                                                                                            command.Parameters.AddWithValue("@CostValue", userInput_InsertCostValid);
                                                                                                            command.Parameters.AddWithValue("@WeightValue", userInput_InsertWeightValid);
                                                                                                            command.Parameters.AddWithValue("@ColourValue", userInput_InsertColour2);
                                                                                                            command.Parameters.AddWithValue("@ProduceValue", userInput_InsertWoolValid);
                                                                                                            try
                                                                                                            {
                                                                                                                int rowsAffected = command.ExecuteNonQuery();
                                                                                                                Console.WriteLine($"{rowsAffected} row(s) inserted successfully.");
                                                                                                                break;
                                                                                                            }
                                                                                                            catch (Exception ex)
                                                                                                            {
                                                                                                                Console.WriteLine("Error: " + ex.Message);
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                    else
                                                                                                    {
                                                                                                        Console.WriteLine("Record insert cancelled.");
                                                                                                        break;
                                                                                                    }
                                                                                                }
                                                                                                break;
                                                                                            }
                                                                                            else
                                                                                            {
                                                                                                Console.Write("Invalid wool amount. Please enter a valid wool amount.\n");
                                                                                            }
                                                                                        }
                                                                                        break;
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    Console.Write("Colour does not exist. Please enter a real colour.\n");
                                                                                }
                                                                            }
                                                                            break;
                                                                        }
                                                                        else
                                                                        {
                                                                            Console.Write("Invalid weight amount. Please enter a valid weight amount.\n");
                                                                        }
                                                                    }
                                                                    break;
                                                                }
                                                                else
                                                                {
                                                                    Console.Write("Invalid cost amount. Please enter a valid cost amount.\n");
                                                                }
                                                            }
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            Console.Write("Invalid water amount. Please enter a valid water amount.\n");
                                                        }
                                                    }
                                                    break;
                                                }
                                                break;
                                            }
                                            else
                                            {
                                                Console.Write("Invalid livestock type. Please enter a valid livestock type.\n");
                                            }
                                        }
                                        break;
                                    case "2":
                                        PrintLineBreak();

                                        double userInput_UpdateWaterValid;
                                        double userInput_UpdateCostValid;
                                        double userInput_UpdateWeightValid;
                                        double userInput_UpdateProduceValid;

                                        while (true)
                                        {
                                            Console.Write("Display livestock data? (Y/N): ");
                                            string userUpdateData_View = Console.ReadLine();

                                            string userUpdateData_ViewToLower = userUpdateData_View.ToLower();

                                            if (userUpdateData_ViewToLower == "y")
                                            {
                                                PrintLivestockDataHeader();

                                                PrintCowLivestockData(connection);

                                                PrintGoatLivestockData(connection);

                                                PrintSheepLivestockData(connection);

                                                PrintLineBreak();

                                                break;
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                        while (true)
                                        {
                                            Console.Write("Enter livestock ID: ");
                                            string userInput_UpdateID = Console.ReadLine();

                                            if (IsIDValid(userInput_UpdateID, connection) && int.TryParse(userInput_UpdateID, out int userInput_UpdateIDValid))
                                            {
                                                string tableName = SelectedIDLivestockType(userInput_UpdateID, connection);

                                                string selectSql = $"SELECT '{tableName}' AS Type, * FROM {tableName} WHERE ID = ?";

                                                using (OleDbCommand selectCommand = new OleDbCommand(selectSql, connection))
                                                {
                                                    selectCommand.Parameters.AddWithValue("@ID", userInput_UpdateID);

                                                    using (OleDbDataReader reader = selectCommand.ExecuteReader())
                                                    {
                                                        if (reader.HasRows)
                                                        {
                                                            for (int i = 0; i < reader.FieldCount; i++)
                                                            {
                                                                Console.Write(reader.GetName(i) + "\t");
                                                            }
                                                            Console.WriteLine();

                                                            while (reader.Read())
                                                            {
                                                                for (int i = 0; i < reader.FieldCount; i++)
                                                                {
                                                                    Console.Write(reader[i] + "\t");
                                                                }
                                                                Console.WriteLine();
                                                            }
                                                        }
                                                        while (true)
                                                        {
                                                            Console.Write("Enter water: ");
                                                            string userInput_UpdateWater = Console.ReadLine();

                                                            if (double.TryParse(userInput_UpdateWater, out userInput_UpdateWaterValid))
                                                            {
                                                                while (true)
                                                                {
                                                                    Console.Write("Enter cost: ");
                                                                    string userInput_UpdateCost = Console.ReadLine();

                                                                    if (double.TryParse(userInput_UpdateCost, out userInput_UpdateCostValid))
                                                                    {
                                                                        while (true)
                                                                        {
                                                                            Console.Write("Enter weight: ");
                                                                            string userInput_UpdateWeight = Console.ReadLine();

                                                                            if (double.TryParse(userInput_UpdateWeight, out userInput_UpdateWeightValid))
                                                                            {
                                                                                while (true)
                                                                                {
                                                                                    Console.Write("Enter colour: ");
                                                                                    string userInput_UpdateColour = Console.ReadLine();

                                                                                    string lowerCaseInputColour = userInput_UpdateColour.ToLower();

                                                                                    if (IsEnteredColorValid(lowerCaseInputColour) && !lowerCaseInputColour.Any(char.IsDigit) && !string.IsNullOrEmpty(lowerCaseInputColour))
                                                                                    {
                                                                                        string userInput_UpdateColourValid = char.ToUpper(lowerCaseInputColour[0]) + lowerCaseInputColour.Substring(1);

                                                                                        while (true)
                                                                                        {
                                                                                            Console.Write("Enter milk/wool: ");
                                                                                            string userInput_UpdateProduce = Console.ReadLine();

                                                                                            if (double.TryParse(userInput_UpdateProduce, out userInput_UpdateProduceValid))
                                                                                            {
                                                                                                string updateSql = $"UPDATE {tableName} " +
                                                                                                                   "SET Water = @WaterValue, Cost = @CostValue, Weight = @WeightValue, Colour = @ColourValue, " +
                                                                                                                   $"{(tableName == "Sheep" ? "Wool" : "Milk")} = @ProduceValue " +
                                                                                                                   "WHERE ID = @IdValue";

                                                                                                PrintLineBreak();

                                                                                                PrintLivestockDataHeader();

                                                                                                Console.WriteLine("{0,-8}{1,-8}{2,-8}{3,-8}{4,-8}{5,-8}{6, -8}", tableName, userInput_UpdateIDValid, userInput_UpdateWaterValid, userInput_UpdateCostValid, userInput_UpdateWeightValid, userInput_UpdateColourValid, userInput_UpdateProduceValid);

                                                                                                while (true)
                                                                                                {
                                                                                                    Console.Write("Are you sure you want to update this record? Y/N: ");

                                                                                                    string userInput_UpdateConfirm = Console.ReadLine();

                                                                                                    string lowerCaseInputConfirm = userInput_UpdateConfirm.ToLower();

                                                                                                    if (userInput_UpdateConfirm == "y")
                                                                                                    {
                                                                                                        using (OleDbCommand command = new OleDbCommand(updateSql, connection))
                                                                                                        {
                                                                                                            command.Parameters.AddWithValue("@WaterValue", userInput_UpdateWaterValid);
                                                                                                            command.Parameters.AddWithValue("@CostValue", userInput_UpdateCostValid);
                                                                                                            command.Parameters.AddWithValue("@WeightValue", userInput_UpdateWeightValid);
                                                                                                            command.Parameters.AddWithValue("@ColourValue", userInput_UpdateColourValid);
                                                                                                            command.Parameters.AddWithValue("@ProduceValue", userInput_UpdateProduceValid);
                                                                                                            command.Parameters.AddWithValue("@IdValue", userInput_UpdateIDValid);
                                                                                                            try
                                                                                                            {
                                                                                                                int rowsAffected = command.ExecuteNonQuery();
                                                                                                                Console.WriteLine($"{rowsAffected} row(s) updated successfully.");
                                                                                                                break;
                                                                                                            }
                                                                                                            catch (Exception ex)
                                                                                                            {
                                                                                                                Console.WriteLine("Error: " + ex.Message);
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                    else
                                                                                                    {
                                                                                                        Console.WriteLine("Record update cancelled.");
                                                                                                        break;
                                                                                                    }
                                                                                                }
                                                                                                break;
                                                                                            }
                                                                                            else
                                                                                            {
                                                                                                Console.Write("Invalid milk/wool amount. Please enter a valid milk/wool amount.\n");
                                                                                            }
                                                                                        }
                                                                                        break;
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        Console.Write("Colour does not exist. Please enter a real colour.\n");
                                                                                    }
                                                                                }
                                                                                break;
                                                                            }
                                                                            else
                                                                            {
                                                                                Console.Write("Invalid weight amount. Please enter a valid weight amount.\n");
                                                                            }
                                                                        }
                                                                        break;
                                                                    }
                                                                    else
                                                                    {
                                                                        Console.Write("Invalid cost amount. Please enter a valid cost amount.\n");
                                                                    }
                                                                }
                                                                break;
                                                            }
                                                            else
                                                            {
                                                                Console.Write("Invalid water amount. Please enter a valid water amount.\n");
                                                            }
                                                        }
                                                        break;
                                                    }
                                                }
                                            }
                                            else if (!IsIDValid(userInput_UpdateID, connection))
                                            {
                                                Console.WriteLine("ID does not exist. Please enter a valid ID.");
                                            }
                                            else
                                            {
                                                Console.WriteLine("Invalid input. Please enter a valid integer.");
                                            }
                                        }
                                        break;
                                    case "3":
                                        PrintLineBreak();

                                        while (true)
                                        {
                                            Console.Write("Display livestock data? (Y/N): ");
                                            string userDeleteData_View = Console.ReadLine();

                                            string userDeleteData_ViewToLower = userDeleteData_View.ToLower();

                                            if (userDeleteData_ViewToLower == "y")
                                            {
                                                PrintLivestockDataHeader();

                                                PrintCowLivestockData(connection);

                                                PrintGoatLivestockData(connection);

                                                PrintSheepLivestockData(connection);

                                                PrintLineBreak();

                                                break;
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                        while (true)
                                        {
                                            Console.Write("Enter livestock ID: ");
                                            string userInput_DeleteID = Console.ReadLine();

                                            if (IsIDValid(userInput_DeleteID, connection) && int.TryParse(userInput_DeleteID, out int userInput_DeleteIDValid))
                                            {
                                                string tableName = SelectedIDLivestockType(userInput_DeleteID, connection);

                                                string selectSql = $"SELECT '{tableName}' AS Type, * FROM {tableName} WHERE ID = ?";

                                                using (OleDbCommand selectCommand = new OleDbCommand(selectSql, connection))
                                                {
                                                    selectCommand.Parameters.AddWithValue("@ID", userInput_DeleteID);

                                                    using (OleDbDataReader reader = selectCommand.ExecuteReader())
                                                    {
                                                        PrintLineBreak();

                                                        if (reader.HasRows)
                                                        {
                                                            for (int i = 0; i < reader.FieldCount; i++)
                                                            {
                                                                Console.Write(reader.GetName(i) + "\t");
                                                            }
                                                            Console.WriteLine();

                                                            while (reader.Read())
                                                            {
                                                                for (int i = 0; i < reader.FieldCount; i++)
                                                                {
                                                                    Console.Write(reader[i] + "\t");
                                                                }
                                                                Console.WriteLine();
                                                            }
                                                            while (true)
                                                            {
                                                                Console.Write("Are you sure you want to delete this record? Y/N: ");

                                                                string userInput_DeleteConfirm = Console.ReadLine();

                                                                string userInput_DeleteConfirmToLower = userInput_DeleteConfirm.ToLower();

                                                                if (userInput_DeleteConfirmToLower == "y")
                                                                {
                                                                    string deleteSql = $"DELETE FROM {tableName} WHERE ID = ?";
                                                                    using (OleDbCommand deleteCommand = new OleDbCommand(deleteSql, connection))
                                                                    {
                                                                        deleteCommand.Parameters.AddWithValue("@IdToDelete", userInput_DeleteID);

                                                                        try
                                                                        {
                                                                            int rowsAffected = deleteCommand.ExecuteNonQuery();
                                                                            if (rowsAffected > 0)
                                                                            {
                                                                                Console.WriteLine($"{rowsAffected} row(s) deleted successfully.");
                                                                            }
                                                                            else
                                                                            {
                                                                                Console.WriteLine("No rows deleted.");
                                                                            }
                                                                        }
                                                                        catch (Exception ex)
                                                                        {
                                                                            Console.WriteLine("Error: " + ex.Message);
                                                                        }
                                                                    }
                                                                    break;
                                                                }
                                                                else
                                                                {
                                                                    Console.WriteLine("Record delete cancelled.");
                                                                    break;
                                                                }
                                                            }
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            else if (!IsIDValid(userInput_DeleteID, connection) && int.TryParse(userInput_DeleteID, out int userInput_DeleteIDInvalid))
                                            {
                                                Console.WriteLine("ID does not exist. Please enter a valid ID.");
                                            }
                                            else
                                            {
                                                Console.WriteLine("Invalid input. Please enter a valid integer.");
                                            }
                                        }
                                        break;
                                    case "4":
                                        PrintLineBreak();

                                        double userInput_UpdateGoatMilk_CommodityValid;
                                        double userInput_UpdateCowMilk_CommodityValid;
                                        double userInput_UpdateSheepWool_CommodityValid;
                                        double userInput_UpdateWater_CommodityValid;
                                        double userInput_UpdateLivestockWeightTax_CommodityValid;

                                        while (true)
                                        {
                                            Console.Write("Display commodity data? (Y/N): ");
                                            string userUpdateData_View = Console.ReadLine();

                                            string userUpdateData_ViewToLower = userUpdateData_View.ToLower();

                                            if (userUpdateData_ViewToLower == "y")
                                            {
                                                PrintCommodityDataHeader();

                                                PrintCommodityData(connection);

                                                PrintLineBreak();

                                                break;
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                        while (true)
                                        {
                                            Console.Write("Enter new goat milk: ");
                                            string userInput_UpdateGoatMilk_Commodity = Console.ReadLine();

                                            if (double.TryParse(userInput_UpdateGoatMilk_Commodity, out userInput_UpdateGoatMilk_CommodityValid))
                                            {
                                                while (true)
                                                {
                                                    Console.Write("Enter new cow milk: ");
                                                    string userInput_UpdateCowMilk_Commodity = Console.ReadLine();

                                                    if (double.TryParse(userInput_UpdateCowMilk_Commodity, out userInput_UpdateCowMilk_CommodityValid))
                                                    {
                                                        while (true)
                                                        {
                                                            Console.Write("Enter new sheep wool: ");
                                                            string userInput_UpdateSheepWool_Commodity = Console.ReadLine();

                                                            if (double.TryParse(userInput_UpdateSheepWool_Commodity, out userInput_UpdateSheepWool_CommodityValid))
                                                            {
                                                                while (true)
                                                                {
                                                                    Console.Write("Enter new water: ");
                                                                    string userInput_UpdateWater_Commodity = Console.ReadLine();

                                                                    if (double.TryParse(userInput_UpdateWater_Commodity, out userInput_UpdateWater_CommodityValid))
                                                                    {
                                                                        while (true)
                                                                        {
                                                                            Console.Write("Enter new livestock weight tax: ");
                                                                            string userInput_UpdateLivestockWeightTax_Commodity = Console.ReadLine();

                                                                            if (double.TryParse(userInput_UpdateLivestockWeightTax_Commodity, out userInput_UpdateLivestockWeightTax_CommodityValid))
                                                                            {
                                                                                string updateSql = "UPDATE Commodity SET Price = IIF(Item = 'GoatMilk', @GoatMilkValue, " +
                                                                                                   "IIF(Item = 'CowMilk', @CowMilkValue, " +
                                                                                                   "IIF(Item = 'SheepWool', @SheepWoolValue, " +
                                                                                                   "IIF(Item = 'Water', @WaterValue, " +
                                                                                                   "IIF(Item = 'LivestockWeightTax', @LivestockWeightTaxValue, Price))))) " +
                                                                                                   "WHERE Item IN ('GoatMilk', 'CowMilk', 'SheepWool', 'Water', 'LivestockWeightTax')";

                                                                                PrintLineBreak();

                                                                                while (true)
                                                                                {
                                                                                    PrintCommodityDataHeader();

                                                                                    Console.WriteLine("{0,-18}{1,-8}", "GoatMilk", userInput_UpdateGoatMilk_CommodityValid);
                                                                                    Console.WriteLine("{0,-18}{1,-8}", "CowMilk", userInput_UpdateCowMilk_CommodityValid);
                                                                                    Console.WriteLine("{0,-18}{1,-8}", "SheepWool", userInput_UpdateSheepWool_CommodityValid);
                                                                                    Console.WriteLine("{0,-18}{1,-8}", "Water", userInput_UpdateWater_CommodityValid);
                                                                                    Console.WriteLine("{0,-18}{1,-8}", "LivestockWeightTax", userInput_UpdateLivestockWeightTax_CommodityValid);


                                                                                    Console.Write("Are you sure you want to update this table? Y/N: ");

                                                                                    string userInput_UpdateConfirm = Console.ReadLine();

                                                                                    string lowerCaseInputConfirm = userInput_UpdateConfirm.ToLower();

                                                                                    if (userInput_UpdateConfirm == "y")
                                                                                    {
                                                                                        using (OleDbCommand command = new OleDbCommand(updateSql, connection))
                                                                                        {
                                                                                            command.Parameters.AddWithValue("@GoatMilkValue", userInput_UpdateGoatMilk_CommodityValid);
                                                                                            command.Parameters.AddWithValue("@CowMilkValue", userInput_UpdateCowMilk_CommodityValid);
                                                                                            command.Parameters.AddWithValue("@SheepWoolValue", userInput_UpdateSheepWool_CommodityValid);
                                                                                            command.Parameters.AddWithValue("@WaterValue", userInput_UpdateWater_CommodityValid);
                                                                                            command.Parameters.AddWithValue("@LivestockWeightTaxValue", userInput_UpdateLivestockWeightTax_CommodityValid);
                                                                                            try
                                                                                            {
                                                                                                int rowsAffected = command.ExecuteNonQuery();
                                                                                                Console.WriteLine($"{rowsAffected} row(s) updated successfully.");
                                                                                                break;
                                                                                            }
                                                                                            catch (Exception ex)
                                                                                            {
                                                                                                Console.WriteLine("Error: " + ex.Message);
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        Console.WriteLine("Table update cancelled.");
                                                                                        break;
                                                                                    }
                                                                                }
                                                                                break;
                                                                            }
                                                                            else
                                                                            {
                                                                                Console.Write("Invalid livestock weight tax amount. Please enter a valid livestock weight tax amount.\n");
                                                                            }
                                                                        }
                                                                        break;
                                                                    }
                                                                    else
                                                                    {
                                                                        Console.Write("Invalid water amount. Please enter a valid water amount.\n");
                                                                    }
                                                                }
                                                                break;
                                                            }
                                                            else
                                                            {
                                                                Console.Write("Invalid sheep wool amount. Please enter a valid sheep wool amount.\n");
                                                            }
                                                        }
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        Console.Write("Invalid cow milk amount. Please enter a valid cow milk amount.\n");
                                                    }
                                                }
                                                break;
                                            }
                                            else
                                            {
                                                Console.Write("Invalid goat milk amount. Please enter a valid goat milk amount.\n");
                                            }
                                        }
                                        break;
                                    case "5":
                                        break;
                                    default:
                                        Console.WriteLine("Invalid choice. Please try again.");
                                        break;
                                }
                                if (userInput_ModifyData == "5")
                                {
                                    break;
                                }
                            }
                        }
                        else if (userInput_MainMenu == "3")
                        {
                            PrintLineBreak();

                            Console.Write("1. Statistics by colour\n" +
                                          "2. Statistics by type\n" +
                                          "3. Statistics by weight threshold\n" +
                                          "4. Display operation report\n" +
                                          "5. Main Menu\n");

                            Console.Write("Select an option: ");

                            string userInput_QueryData = Console.ReadLine();

                            OleDbCommand cmd = connection.CreateCommand();
                            cmd.CommandType = CommandType.Text;

                            switch (userInput_QueryData)
                            {
                                case "1":
                                    while (true)
                                    {
                                        Console.Write("Enter livestock colour: ");

                                        string userInput_QueryColour = Console.ReadLine();

                                        string lowerCaseInput_QueryColour = userInput_QueryColour.ToLower();


                                        if (IsColourValid(lowerCaseInput_QueryColour, connection))
                                        {
                                            string userInput_QueryColour2 = char.ToUpper(lowerCaseInput_QueryColour[0]) + lowerCaseInput_QueryColour.Substring(1);

                                            cmd.CommandText = $"SELECT COUNT(*) FROM Cow WHERE Colour = ?";
                                            cmd.Parameters.AddWithValue("Colour", userInput_QueryColour2);
                                            int cowCount = (int)cmd.ExecuteScalar();

                                            cmd.CommandText = $"SELECT COUNT(*) FROM Goat WHERE Colour = ?";
                                            int goatCount = (int)cmd.ExecuteScalar();

                                            cmd.CommandText = $"SELECT COUNT(*) FROM Sheep WHERE Colour = ?";
                                            int sheepCount = (int)cmd.ExecuteScalar();

                                            cmd.Parameters.Clear();
                                            cmd.CommandText = $"SELECT COUNT(*) FROM Cow";
                                            int totalCowCount = (int)cmd.ExecuteScalar();

                                            cmd.CommandText = $"SELECT COUNT(*) FROM Goat";
                                            int totalGoatCount = (int)cmd.ExecuteScalar();

                                            cmd.CommandText = $"SELECT COUNT(*) FROM Sheep";
                                            int totalSheepCount = (int)cmd.ExecuteScalar();

                                            double livestockWeightTax = 0.0;

                                            cmd.CommandText = "SELECT Price FROM Commodity WHERE Item = ?";
                                            cmd.Parameters.AddWithValue("Item", "LivestockWeightTax");
                                            object livestockWeightTaxObj = cmd.ExecuteScalar();
                                            if (livestockWeightTaxObj != null)
                                            {
                                                livestockWeightTax = Convert.ToDouble(livestockWeightTaxObj);
                                            }

                                            cmd.Parameters.Clear();
                                            cmd.CommandText = $"SELECT SUM(Weight) FROM Cow WHERE Colour = ?";
                                            cmd.Parameters.AddWithValue("Colour", userInput_QueryColour2);
                                            object cowTotalWeightObj = cmd.ExecuteScalar();
                                            double cowTotalWeight = cowTotalWeightObj != DBNull.Value ? Convert.ToDouble(cowTotalWeightObj) : 0.0;
                                            double cowTotalTax = cowTotalWeight * livestockWeightTax;

                                            cmd.Parameters.Clear();
                                            cmd.CommandText = $"SELECT SUM(Weight) FROM Goat WHERE Colour = ?";
                                            cmd.Parameters.AddWithValue("Colour", userInput_QueryColour2);
                                            object goatTotalWeightObj = cmd.ExecuteScalar();
                                            double goatTotalWeight = goatTotalWeightObj != DBNull.Value ? Convert.ToDouble(goatTotalWeightObj) : 0.0;
                                            double goatTotalTax = goatTotalWeight * livestockWeightTax;

                                            cmd.Parameters.Clear();
                                            cmd.CommandText = $"SELECT SUM(Weight) FROM Sheep WHERE Colour = ?";
                                            cmd.Parameters.AddWithValue("Colour", userInput_QueryColour);
                                            object sheepTotalWeightObj = cmd.ExecuteScalar();
                                            double sheepTotalWeight = sheepTotalWeightObj != DBNull.Value ? Convert.ToDouble(sheepTotalWeightObj) : 0.0;
                                            double sheepTotalTax = sheepTotalWeight * livestockWeightTax;

                                            cmd.Parameters.Clear();
                                            cmd.CommandText = $"SELECT SUM(Milk) FROM Cow WHERE Colour = ?";
                                            cmd.Parameters.AddWithValue("Colour", userInput_QueryColour);

                                            object cowTotalMilkObj = cmd.ExecuteScalar();
                                            double cowTotalMilk = cowTotalMilkObj != DBNull.Value ? Convert.ToDouble(cowTotalMilkObj) : 0.0;

                                            cmd.Parameters.Clear();
                                            cmd.CommandText = $"SELECT SUM(Milk) FROM Goat WHERE Colour = ?";
                                            cmd.Parameters.AddWithValue("Colour", userInput_QueryColour);

                                            object goatTotalMilkObj = cmd.ExecuteScalar();
                                            double goatTotalMilk = goatTotalMilkObj != DBNull.Value ? Convert.ToDouble(goatTotalMilkObj) : 0.0;

                                            cmd.Parameters.Clear();
                                            cmd.CommandText = $"SELECT SUM(Wool) FROM Sheep WHERE Colour = ?";
                                            cmd.Parameters.AddWithValue("Colour", userInput_QueryColour);

                                            object sheepTotalWoolObj = cmd.ExecuteScalar();
                                            double sheepTotalWool = sheepTotalWoolObj != DBNull.Value ? Convert.ToDouble(sheepTotalWoolObj) : 0.0;

                                            double cowMilkPrice = 0.0;
                                            double goatMilkPrice = 0.0;
                                            double sheepWoolPrice = 0.0;

                                            cmd.Parameters.Clear();
                                            cmd.CommandText = "SELECT Price FROM Commodity WHERE Item = ?";
                                            cmd.Parameters.AddWithValue("Item", "CowMilk");

                                            object cowMilkPriceObj = cmd.ExecuteScalar();
                                            if (cowMilkPriceObj != null)
                                            {
                                                cowMilkPrice = Convert.ToDouble(cowMilkPriceObj);
                                            }

                                            cmd.Parameters.Clear();
                                            cmd.CommandText = "SELECT Price FROM Commodity WHERE Item = ?";
                                            cmd.Parameters.AddWithValue("Item", "GoatMilk");

                                            object goatMilkPriceObj = cmd.ExecuteScalar();
                                            if (goatMilkPriceObj != null)
                                            {
                                                goatMilkPrice = Convert.ToDouble(goatMilkPriceObj);
                                            }

                                            cmd.Parameters.Clear();
                                            cmd.CommandText = "SELECT Price FROM Commodity WHERE Item = ?";
                                            cmd.Parameters.AddWithValue("Item", "SheepWool");

                                            object sheepWoolPriceObj = cmd.ExecuteScalar();
                                            if (sheepWoolPriceObj != null)
                                            {
                                                sheepWoolPrice = Convert.ToDouble(sheepWoolPriceObj);
                                            }

                                            double waterPrice = 0.0;

                                            cmd.Parameters.Clear();
                                            cmd.CommandText = "SELECT Price FROM Commodity WHERE Item = ?";
                                            cmd.Parameters.AddWithValue("Item", "Water");
                                            object waterPriceObj = cmd.ExecuteScalar();
                                            if (waterPriceObj != null)
                                            {
                                                waterPrice = Convert.ToDouble(waterPriceObj);
                                            }

                                            double totalCowCost = 0.0;

                                            cmd.Parameters.Clear();
                                            cmd.CommandText = $@"SELECT SUM(Cost +(Water * {waterPrice}) +(Weight * {livestockWeightTax})) AS totalCowCost FROM Cow WHERE Colour = ?";
                                            cmd.Parameters.AddWithValue("Colour", userInput_QueryColour);
                                            object totalCowCostObj = cmd.ExecuteScalar();
                                            if (totalCowCostObj != DBNull.Value && totalCowCostObj != null)
                                            {
                                                totalCowCost = Convert.ToDouble(totalCowCostObj);
                                            }

                                            double totalGoatCost = 0.0;

                                            cmd.Parameters.Clear();
                                            cmd.CommandText = $@"SELECT SUM(Cost +(Water * {waterPrice}) +(Weight * {livestockWeightTax})) AS totalGoatCost FROM Goat WHERE Colour = ?";
                                            cmd.Parameters.AddWithValue("Colour", userInput_QueryColour);
                                            object totalGoatCostObj = cmd.ExecuteScalar();
                                            if (totalGoatCostObj != DBNull.Value && totalGoatCostObj != null)
                                            {
                                                totalGoatCost = Convert.ToDouble(totalGoatCostObj);
                                            }

                                            double totalSheepCost = 0.0;

                                            cmd.Parameters.Clear();
                                            cmd.CommandText = $@"SELECT SUM(Cost +(Water * {waterPrice}) +(Weight * {livestockWeightTax})) AS totalSheepCost FROM Sheep WHERE Colour = ?";
                                            cmd.Parameters.AddWithValue("Colour", userInput_QueryColour);
                                            object totalSheepCostObj = cmd.ExecuteScalar();
                                            if (totalSheepCostObj != DBNull.Value && totalSheepCostObj != null)
                                            {
                                                totalSheepCost = Convert.ToDouble(totalSheepCostObj);
                                            }

                                            double cowTotalIncome = cowTotalMilk * cowMilkPrice;

                                            double goatTotalIncome = goatTotalMilk * goatMilkPrice;

                                            double sheepTotalIncome = sheepTotalWool * sheepWoolPrice;

                                            int animalsInColour = cowCount + goatCount + sheepCount;

                                            int totalAnimals = totalCowCount + totalGoatCount + totalSheepCount;

                                            double percentage = (double)animalsInColour / totalAnimals * 100;

                                            double totalTax = cowTotalTax + goatTotalTax + sheepTotalTax;

                                            double totalIncome = cowTotalIncome + goatTotalIncome + sheepTotalIncome;

                                            double totalCost = totalCowCost + totalGoatCost + totalSheepCost;

                                            double totalProfitLoss = totalIncome - totalCost;

                                            connection.Close();

                                            if (totalProfitLoss > 0)
                                            {
                                                PrintLineBreak();
                                                Console.WriteLine($"Number of animals in colour {userInput_QueryColour2}: {animalsInColour}\n" +
                                                                  $"Percentage of animals with colour {userInput_QueryColour2}: {percentage:F2}%\n" +
                                                                  $"Total Tax per day by {userInput_QueryColour2} animals: {totalTax:C}\n" +
                                                                  $"Total income generated by {userInput_QueryColour2}: {totalIncome:C}\n" +
                                                                  $"Total cost generated by {userInput_QueryColour2}: {totalCost:C}\n\n" +
                                                                  $"Profit: ${totalProfitLoss.ToString("F2")}");

                                                break;
                                            }
                                            else if (totalProfitLoss < 0)
                                            {
                                                PrintLineBreak();
                                                Console.WriteLine($"Number of animals in colour {userInput_QueryColour2}: {animalsInColour}\n" +
                                                                  $"Percentage of animals with colour {userInput_QueryColour2}: {percentage:F2}%\n" +
                                                                  $"Total Tax per day by {userInput_QueryColour2} animals: {totalTax:C}\n" +
                                                                  $"Total income generated by {userInput_QueryColour2}: {totalIncome:C}\n" +
                                                                  $"Total cost generated by {userInput_QueryColour2}: {totalCost:C}\n\n" +
                                                                  $"Loss: ${totalProfitLoss.ToString("F2")}");

                                                break;
                                            }
                                        }
                                        else if (!IsColourValid(lowerCaseInput_QueryColour, connection) && IsEnteredColorValid(lowerCaseInput_QueryColour))
                                        {
                                            Console.WriteLine("Colour not found.");
                                            break;
                                        }
                                        else
                                        {
                                            Console.Write("Invalid livestock colour. Please enter a valid livestock colour.\n");
                                        }
                                    }
                                    break;
                                case "2":
                                    while (true)
                                    {
                                        double livestockWeightTax = 0.0;

                                        cmd.CommandText = "SELECT Price FROM Commodity WHERE Item = ?";
                                        cmd.Parameters.AddWithValue("Item", "LivestockWeightTax");
                                        object livestockWeightTaxObj = cmd.ExecuteScalar();
                                        if (livestockWeightTaxObj != null)
                                        {
                                            livestockWeightTax = Convert.ToDouble(livestockWeightTaxObj);
                                        }

                                        Console.Write("Enter livestock type (Cow/Goat/Sheep): ");
                                        string userInput_QueryType = Console.ReadLine();

                                        string lowerCaseInput_QueryType = userInput_QueryType.ToLower();

                                        if (lowerCaseInput_QueryType == "cow")
                                        {
                                            string userInput_QueryType2 = char.ToUpper(lowerCaseInput_QueryType[0]) + lowerCaseInput_QueryType.Substring(1);

                                            cmd.CommandText = $"SELECT SUM(Milk) FROM Cow";
                                            double cowProduceTotal = (double)cmd.ExecuteScalar();

                                            cmd.CommandText = $"SELECT SUM(Water) FROM Cow";
                                            double cowWaterTotal = (double)cmd.ExecuteScalar();

                                            cmd.CommandText = $"SELECT SUM(Weight) FROM Cow";
                                            double cowWeightTotal = (double)cmd.ExecuteScalar();

                                            double cowTotalTax = cowWeightTotal * livestockWeightTax;

                                            PrintLineBreak();

                                            Console.WriteLine($"Produce amount for {userInput_QueryType2}: {cowProduceTotal}\n" +
                                                              $"Water consumption for {userInput_QueryType2}: {cowWaterTotal}\n" +
                                                              $"Tax for {userInput_QueryType2}: {cowTotalTax:C}");
                                            break;
                                        }
                                        if (lowerCaseInput_QueryType == "goat")
                                        {
                                            string userInput_QueryType2 = char.ToUpper(lowerCaseInput_QueryType[0]) + lowerCaseInput_QueryType.Substring(1);

                                            cmd.CommandText = $"SELECT SUM(Milk) FROM Goat";
                                            double goatProduceTotal = (double)cmd.ExecuteScalar();

                                            cmd.CommandText = $"SELECT SUM(Water) FROM Goat";
                                            double goatWaterTotal = (double)cmd.ExecuteScalar();

                                            cmd.CommandText = $"SELECT SUM(Weight) FROM Goat";
                                            double goatWeightTotal = (double)cmd.ExecuteScalar();

                                            double goatTotalTax = goatWeightTotal * livestockWeightTax;

                                            PrintLineBreak();

                                            Console.WriteLine($"Produce amount for {userInput_QueryType2}: {goatProduceTotal}\n" +
                                                              $"Water consumption for {userInput_QueryType2}: {goatWaterTotal}\n" +
                                                              $"Tax for {userInput_QueryType2}: {goatTotalTax:C}");
                                            break;
                                        }
                                        if (lowerCaseInput_QueryType == "sheep")
                                        {
                                            string userInput_QueryType2 = char.ToUpper(lowerCaseInput_QueryType[0]) + lowerCaseInput_QueryType.Substring(1);

                                            cmd.CommandText = $"SELECT SUM(Wool) FROM Sheep";
                                            double sheepProduceTotal = (double)cmd.ExecuteScalar();

                                            cmd.CommandText = $"SELECT SUM(Water) FROM Sheep";
                                            double sheepWaterTotal = (double)cmd.ExecuteScalar();

                                            cmd.CommandText = $"SELECT SUM(Weight) FROM Sheep";
                                            double sheepWeightTotal = (double)cmd.ExecuteScalar();

                                            double sheepTotalTax = sheepWeightTotal * livestockWeightTax;

                                            PrintLineBreak();

                                            Console.WriteLine($"Produce amount for {userInput_QueryType2}: {sheepProduceTotal}\n" +
                                                              $"Water consumption for {userInput_QueryType2}: {sheepWaterTotal}\n" +
                                                              $"Tax for {userInput_QueryType2}: {sheepTotalTax:C}");
                                            break;
                                        }
                                        else
                                        {
                                            Console.Write("Invalid livestock type. Please enter a valid livestock type.\n");
                                        }
                                    }
                                    break;
                                case "3":
                                    while (true)
                                    {
                                        Console.Write("Enter weight threshold (>0): ");
                                        string userInput_QueryWeight = Console.ReadLine();

                                        if (IsWeightValid(userInput_QueryWeight) && (double.TryParse(userInput_QueryWeight, out double weightThreshold) && weightThreshold > 0))
                                        {
                                            cmd.CommandText = "SELECT AVG(Weight) AS AvgWeight " +
                                                              "FROM (SELECT Weight FROM Cow UNION ALL " +
                                                              "      SELECT Weight FROM Goat UNION ALL " +
                                                              "      SELECT Weight FROM Sheep) AS LivestockWeight " +
                                                              "WHERE Weight >= ?";
                                            cmd.Parameters.AddWithValue("@Weight", userInput_QueryWeight);

                                            object avgWeightObj = cmd.ExecuteScalar();

                                            if (avgWeightObj != DBNull.Value)
                                            {
                                                double avgWeight = Convert.ToDouble(avgWeightObj);

                                                cmd.Parameters.Clear();
                                                cmd.Parameters.AddWithValue("@Weight", userInput_QueryWeight);
                                                cmd.CommandText = $"SELECT SUM(Milk) FROM Cow WHERE Weight >= ?";
                                                object cowMilkTotalObj = cmd.ExecuteScalar();
                                                double cowMilkTotal = GetDoubleValue(cowMilkTotalObj);

                                                cmd.Parameters.Clear();
                                                cmd.Parameters.AddWithValue("@Weight", userInput_QueryWeight);
                                                cmd.CommandText = $"SELECT SUM(Milk) FROM Goat WHERE Weight >= ?";
                                                object goatMilkTotalObj = cmd.ExecuteScalar();
                                                double goatMilkTotal = GetDoubleValue(goatMilkTotalObj);

                                                cmd.Parameters.Clear();
                                                cmd.Parameters.AddWithValue("@Weight", userInput_QueryWeight);
                                                cmd.CommandText = $"SELECT SUM(Wool) FROM Sheep WHERE Weight >= ?";
                                                object sheepWoolTotalObj = cmd.ExecuteScalar();
                                                double sheepWoolTotal = GetDoubleValue(sheepWoolTotalObj);

                                                double cowMilkPrice = GetCommodityPrice(cmd, "CowMilk");
                                                double goatMilkPrice = GetCommodityPrice(cmd, "GoatMilk");
                                                double sheepWoolPrice = GetCommodityPrice(cmd, "SheepWool");

                                                double totalIncome = (cowMilkTotal * cowMilkPrice) + (goatMilkTotal * goatMilkPrice) + (sheepWoolTotal * sheepWoolPrice);

                                                double waterPrice = GetCommodityPrice(cmd, "Water");
                                                double livestockWeightTax = GetCommodityPrice(cmd, "livestockWeightTax");

                                                cmd.Parameters.Clear();
                                                cmd.Parameters.AddWithValue("@Weight", userInput_QueryWeight);
                                                cmd.CommandText = $@"SELECT SUM(Cost +(Water * {waterPrice}) +(Weight * {livestockWeightTax})) AS totalCowCost FROM Cow WHERE Weight > ?";
                                                object totalCowCostObj = cmd.ExecuteScalar();
                                                double totalCowCost = GetDoubleValue(totalCowCostObj);

                                                cmd.Parameters.Clear();
                                                cmd.Parameters.AddWithValue("@Weight", userInput_QueryWeight);
                                                cmd.CommandText = $@"SELECT SUM(Cost +(Water * {waterPrice}) +(Weight * {livestockWeightTax})) AS totalGoatCost FROM Goat WHERE Weight > ?";
                                                object totalGoatCostObj = cmd.ExecuteScalar();
                                                double totalGoatCost = GetDoubleValue(totalGoatCostObj);

                                                cmd.Parameters.Clear();
                                                cmd.Parameters.AddWithValue("@Weight", userInput_QueryWeight);
                                                cmd.CommandText = $@"SELECT SUM(Cost +(Water * {waterPrice}) +(Weight * {livestockWeightTax})) AS totalSheepCost FROM Sheep WHERE Weight > ?";
                                                object totalSheepCostObj = cmd.ExecuteScalar();
                                                double totalSheepCost = GetDoubleValue(totalSheepCostObj);

                                                double totalCost = totalCowCost + totalGoatCost + totalSheepCost;

                                                double totalProfitLoss = totalIncome - totalCost;

                                                if (totalProfitLoss > 0)
                                                {
                                                    PrintLineBreak();
                                                    Console.WriteLine($"Operation income of livestock above {userInput_QueryWeight}kg: {totalIncome:C}\n" +
                                                                      $"Operation cost of livestock above {userInput_QueryWeight}kg: {totalCost:C}\n" +
                                                                      $"Average weight of livestock above {userInput_QueryWeight}kg: {avgWeight:F2}kg\n\n" +
                                                                      $"Profit: ${totalProfitLoss.ToString("F2")}");

                                                    break;
                                                }
                                                if (totalProfitLoss < 0)
                                                {
                                                    PrintLineBreak();
                                                    Console.WriteLine($"Operation income of livestock above {userInput_QueryWeight}kg: {totalIncome:C}\n" +
                                                                      $"Operation cost of livestock above {userInput_QueryWeight}kg: {totalCost:C}\n" +
                                                                      $"Average weight of livestock above {userInput_QueryWeight}kg: {avgWeight:F2}kg\n\n" +
                                                                      $"Loss: ${totalProfitLoss.ToString("F2")}");
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine($"No livestock found above {userInput_QueryWeight}kg");
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            Console.Write("Invalid weight threshold. Please enter a valid weight threshold (>0).\n");
                                        }
                                    }
                                    break;
                                case "4":
                                    LivestockStatisticsOperation();
                                    break;
                                case "5":
                                    break;
                                default:
                                    Console.WriteLine("Invalid option. Please select a valid option.");
                                    break;
                            }
                        }
                        else if (userInput_MainMenu == "4")
                        {
                            PrintLineBreak();

                            while (true)
                            {
                                Console.Write("1. Restore Default Data\n" +
                                              "2. Create Backup\n" +
                                              "3. Load Backup\n" +
                                              "4. Open Backups Folder\n" +
                                              "5. Main Menu\n");

                                Console.Write("Select an option: ");

                                string userInput_BackupData = Console.ReadLine();

                                switch (userInput_BackupData)
                                {
                                    case "1":
                                        Console.WriteLine("WARNING: Restoring default data will overwrite the current data.");
                                        Console.Write("Are you sure you want to proceed? (Y/N): ");
                                        string confirmRestore = Console.ReadLine();

                                        if (confirmRestore.ToLower() == "y")
                                        {
                                            connection.Close();
                                            string currentDatabasePath = "FarmData.accdb";
                                            string initialDataPath = "FarmData_DefaultData.accdb";

                                            bool restoreSuccess = RestoreDatabase(currentDatabasePath, initialDataPath);

                                            if (restoreSuccess)
                                            {
                                                Console.WriteLine("Database has been restored to its initial state.");
                                            }
                                            else
                                            {
                                                Console.WriteLine("Database restore failed.");
                                            }
                                            connection.Open();
                                        }
                                        else
                                        {
                                            Console.WriteLine("Restore operation canceled.");
                                        }
                                        PrintLineBreak();
                                        break;
                                    case "2": 
                                        connection.Close();
                                        string currentDatabasePath_CreateBackup = "FarmData.accdb"; 

                                        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                                        string backupFileName = $"FarmData_Backup_{timestamp}.accdb";

                                        string backupFilePath = Path.Combine(Directory.GetCurrentDirectory(), "backups", backupFileName);

                                        File.Copy(currentDatabasePath_CreateBackup, backupFilePath);

                                        Console.WriteLine($"Backup created and saved as: {backupFileName} in the 'backups' folder.");
                                        Console.WriteLine("Backup created successfully.");

                                        connection.Open();
                                        PrintLineBreak();

                                        break;

                                    case "3": 
                                        Console.WriteLine("WARNING: Loading a backup will replace the current data.");
                                        Console.Write("Are you sure you want to proceed? (Y/N): ");
                                        string confirmLoadBackup = Console.ReadLine();

                                        if (confirmLoadBackup.ToLower() == "y")
                                        {
                                            connection.Close();

                                            Console.WriteLine("Example: FarmData_Backup_YYYYMMDDHHMMSS.accdb");
                                            Console.Write("Enter the name of the backup file to load: ");
                                            string backupFileNameToLoad = Console.ReadLine();

                                            string backupFilePathToLoad = Path.Combine(Directory.GetCurrentDirectory(), "backups", backupFileNameToLoad);

                                            if (File.Exists(backupFilePathToLoad))
                                            {
                                                string currentDatabasePath_LoadBackup = "FarmData.accdb"; 

                                                File.Copy(backupFilePathToLoad, currentDatabasePath_LoadBackup, true);

                                                Console.WriteLine("Backup loaded successfully.");
                                            }
                                            else
                                            {
                                                Console.WriteLine("Backup file not found. Please check the file name and try again.");
                                            }
                                            connection.Open();
                                        }
                                        else
                                        {
                                            Console.WriteLine("Load backup operation canceled.");
                                        }
                                        PrintLineBreak();
                                        break;
                                    case "4": 
                                        string backupsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "backups");

                                        if (Directory.Exists(backupsFolderPath))
                                        {
                                            Process.Start("explorer.exe", backupsFolderPath);
                                            Console.WriteLine("Backups folder opened.");
                                        }
                                        else
                                        {
                                            Console.WriteLine("Backups folder does not exist.");
                                        }
                                        PrintLineBreak();
                                        break;
                                    case "5": 
                                        break;

                                    default:
                                        Console.WriteLine("Invalid choice. Please select a valid choice.");
                                        break;
                                }
                                if (userInput_BackupData == "5") 
                                {
                                    break; 
                                }
                            }
                        }
                        else if (userInput_MainMenu == "5")
                        {
                            break; 
                        }
                        else
                        {
                            Console.WriteLine("Invalid choice. Please try again.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }
    }
}