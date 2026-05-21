// Declare a string array variable with 3 elements

// string[] fraudulentOrderIDs = new string[3];
// fraudulentOrderIDs[0] = "A123";
// fraudulentOrderIDs[1] = "B456";
// fraudulentOrderIDs[2] = "C789";
// fraudulentOrderIDs[0] = "D012"; // Update the first element of the array

string[] fraudulentOrderIDs = ["A123", "B456", "C789"];

// Print the first and last elements of the array
Console.WriteLine($"First fraudulent order ID: {fraudulentOrderIDs[0]}");
Console.WriteLine($"Second fraudulent order ID: {fraudulentOrderIDs[1]}");
Console.WriteLine($"Last fraudulent order ID: {fraudulentOrderIDs[2]}");
Console.WriteLine($"Total number of fraudulent order IDs: {fraudulentOrderIDs.Length}");

string[] names = { "James", "Angel", "Jarom", "Ethan", "Caleb" };

foreach (string name in names)
{
    Console.WriteLine(name);
    Console.WriteLine($"The name {name} has {name.Length} characters.");
}


/* 
 * Suppose you work for a manufacturing company. The company needs you to complete an inventory 
 * of your warehouse to determine the number of products that are ready to ship. 
 * In addition to the total number of finished products, you need to report the number of 
 * finished products stored in each individual bin in your warehouse, along with a running total. 
 * This running total will be used to create an audit trail so you can double-check your work and 
 * identify "shrinkage".

 */

int[] inventory = { 200, 450, 700, 175, 250 };
int sum = 0;
int bin = 0;
foreach (int items in inventory)
{
    sum += items;
    bin++;
    Console.WriteLine($"Bin {bin} = {items} items (Running total: {sum})");
}
Console.WriteLine($"We have {sum} items in inventory.");