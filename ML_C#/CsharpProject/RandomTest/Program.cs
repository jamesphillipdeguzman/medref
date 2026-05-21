Random dice = new Random();
int roll1 = dice.Next(); // 0 to 2,147,483,647
int roll2 = dice.Next(101); // 0 to 100
int roll3 = dice.Next(50, 101); // 50 to 100 (50 inclusive, 101 exclusive)

Console.WriteLine($"First roll: {roll1}");
Console.WriteLine($"Second roll: {roll2}");
Console.WriteLine($"Third roll: {roll3}");

int result = dice.Next(1, 7); // 1 to 6
Console.WriteLine($"You rolled a {result} on the dice!");

dice = new Random();
int randomNumber = dice.Next(1, 101); // 1 to 100


Random rnd = new();
string[] malePetNames = [ "Rufus", "Bear", "Dakota", "Fido",
                        "Vanya", "Samuel", "Koani", "Volodya",
                        "Prince", "Yiska" ];
string[] femalePetNames = [ "Maggie", "Penny", "Saya", "Princess",
                          "Abby", "Laila", "Sadie", "Olivia",
                          "Starlight", "Talla" ];

// Generate random indexes for pet names.
int mIndex = rnd.Next(malePetNames.Length);
int fIndex = rnd.Next(femalePetNames.Length);

// Display the result.
Console.WriteLine("Suggested pet name of the day: ");
Console.WriteLine($"   For a male:     {malePetNames[mIndex]}");
Console.WriteLine($"   For a female:   {femalePetNames[fIndex]}");

// The example displays output similar to the following:
//       Suggested pet name of the day:
//          For a male:     Koani
//          For a female:   Maggie