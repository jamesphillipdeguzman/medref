using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
//  This program reads sales data from JSON files in a specified directory, calculates the total sales for each store, and writes the results to a text file. It also prints the file paths of the sales files and the sales summary to the console.

var currentDirectory = Directory.GetCurrentDirectory();
var storesDir = Path.Combine(currentDirectory, "stores");

var salesTotalDir = Path.Combine(currentDirectory, "salesTotalDir");
Directory.CreateDirectory(salesTotalDir);

var salesFiles = FindFiles(storesDir);

var salesTotal = CalculateSalesTotal(salesFiles);

File.AppendAllText(Path.Combine(salesTotalDir, "totals.txt"), $"{salesTotal}{Environment.NewLine}");

foreach (var file in salesFiles)
{
    Console.WriteLine(file);

}
IEnumerable<string> FindFiles(string folderName)
{
    List<string> salesFiles = new List<string>();
    List<decimal> salesTotals = new List<decimal>();

    var foundFiles = Directory.EnumerateFiles(folderName, "*", SearchOption.AllDirectories);


    decimal totalSales = 0.00m;
    var summary = $"Sales Summary\n-------------\n\n";
    File.AppendAllText("totals.txt", summary);
    foreach (var file in foundFiles)
    {

        // Get the store name from the file path
        var storeName = Path.GetFileName(Path.GetDirectoryName(file));
        // Get the file name
        var filename = Path.GetFileName(file);
        // Check the extension for JSON files only
        var extensionJSON = Path.GetExtension(file);

        if (filename.StartsWith("salestotals") && extensionJSON == ".json")
        {

            // Read the file and calculate total sales
            var lines = File.ReadAllLines(file);
            foreach (var line in lines)
            {
                // Parse the line as an integer and add it to the total sales
                // Find OverallTotal and parse the value after it

                // Normalize first the overall total line by removing spaces and making it lowercase and removing escape characters, then check if it starts with "overalltotal:"
                // {
                //     "OverallTotal": 154401.22
                // }

                // Total Sales is $169,504.88
                var normalizedLine = line.Replace(" ", "").Replace("\"", "").ToLower();
                if (normalizedLine.Contains("overalltotal:"))
                {
                    double total = double.Parse(normalizedLine.Split(":")[1].Trim());
                    if (double.TryParse(total.ToString(), out double sales))
                    {
                        totalSales += (decimal)sales;
                        salesTotals.Add((decimal)sales);
                        // Print the total sales for each store and the overall total sales for all stores in a formatted way
                        ShowSalesSummary(storeName, (decimal)sales, salesTotals);

                    }
                }

            }
            // Get the total sales for the file and add it to the list of totals
            // Ensure correct format for parsing and handle any potential exceptions



            // Print the total sales for each store and store in totals.txt


            // Console.WriteLine($"Total sales for Store {storeName} is {totalSales}.\n");

        }

        // Get the file extension
        var extension = Path.GetExtension(file);
        // The file name will contain the full path, so only check the end of it
        if (extension == ".txt")
        {
            salesFiles.Add(file);

        }


    }

    var finalSummary = $"\nTotal sales for all stores is ${totalSales:N2}.\n";
    File.AppendAllText("totals.txt", finalSummary);

    return salesFiles;
}

//  Function to display the total sales for each store and the overall total sales for all stores in a formatted way
void ShowSalesSummary(string? storeName, decimal sales, List<decimal> salesTotals)
{
    var summary = "";
    var details = "";


    // Build the summary string for each store and append it to totals.txt

    // Put the result in summary variable and write it to totals.txt
    details = $"Store {storeName}: ${sales:N2}";

    Console.WriteLine($"{summary}\n{details}");
    File.AppendAllText("totals.txt", details + Environment.NewLine);



}



double CalculateSalesTotal(IEnumerable<string> salesFiles)
{
    double salesTotal = 0;

    // Loop over each file path in salesFiles
    foreach (var file in salesFiles)
    {
        // Read the contents of the file
        string salesJson = File.ReadAllText(file);

        // Parse the contents as JSON
        SalesData? data = JsonConvert.DeserializeObject<SalesData?>(salesJson);

        // Add the amount found in the Total field to the salesTotal variable
        salesTotal += data?.Total ?? 0;
    }

    return salesTotal;
}


record SalesData(double Total);

