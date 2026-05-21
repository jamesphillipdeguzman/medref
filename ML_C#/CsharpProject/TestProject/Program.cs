// Roll three dice and calculate the total
Random dice = new Random();
int roll1 = dice.Next(1, 7);
int roll2 = dice.Next(1, 7);
int roll3 = dice.Next(1, 7);

// roll1 = 5;
// roll2 = 4;
// roll3 = 4;

int total = roll1 + roll2 + roll3;
Console.WriteLine($"Dice roll: {roll1} + {roll2} + {roll3} = {total}");

// Logic for doubles
if (roll1 == roll2 || roll1 == roll3 || roll2 == roll3)
{

    // Logic for triples
    if (roll1 == roll2 && roll2 == roll3)
    {
        Console.WriteLine("You rolled triples! +6 bonus to total!");
        total += 6;
        Console.WriteLine($"New total: {total}");
    }
    else
    {
        Console.WriteLine("You rolled doubles! +2 bonus to total!");
        total += 2;
        Console.WriteLine($"New total: {total}");
    }

}




// Create if statements to determine if the total is a win or a loss
if (total >= 15)
{
    Console.WriteLine("You win!");
}
else
{
    Console.WriteLine("You lose!");
}

// If the player scores greater or equal to 16, they'll win a new car.
// If the player scores greater or equal to 10, they'll win a new laptop.
// If the player scores exactly 7, they'll win a trip.
// Otherwise, the player wins a kitten.


if (total >= 16)
{
    Console.WriteLine("Congratulations! You win a new car!");
}
else if (total >= 10)
{
    Console.WriteLine("Congratulations! You win a new laptop!");
}
else if (total == 7)
{
    Console.WriteLine("Congratulations! You win a trip!");
}
else
{
    Console.WriteLine("Congratulations! You win a kitten!");
}