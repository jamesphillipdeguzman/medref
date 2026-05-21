/* 
* Your team has found a pattern. Orders that start with the letter "B" encounter fraud at 
* a rate 25 times greater than the normal rate. You write new code that outputs the Order ID 
* of new orders where the Order ID starts with the letter "B". This will be used by the
* fraud team to investigate further.
*/


string[] fraudulentOrderIDs = ["B123", "C234", "A345", "C15", "B177", "G3003", "C235", "B179"];
foreach (string orderID in fraudulentOrderIDs)
{
    if (orderID.StartsWith("B"))
    {
        Console.WriteLine(orderID);
    }
}

Random dice = new Random(); // Create a new instance of the Random class to generate random numbers using the dice object
int roll = dice.Next(1, 7);
Console.WriteLine(roll);


int number = 7;
string text = "seven";

Console.WriteLine(number);
Console.WriteLine();
Console.WriteLine(text);
