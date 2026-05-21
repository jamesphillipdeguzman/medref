/*
 * You've been asked to add a feature to your company's software. 
 * The feature is intended to improve the renewal rate of subscriptions to the software. 
 * Your task is to display a renewal message when a user logs into the software system 
 * and is notified their subscription will soon end. 
 * You'll need to add a couple of decision statements to properly add branching logic 
 * to the application to satisfy the requirements.

*/
Random random = new Random();
int daysUntilExpiration = random.Next(0, 31);
int discountPercentage = 0;


Console.WriteLine($"Your subscription expires in {daysUntilExpiration} days.");
if (daysUntilExpiration == 0)
{
    Console.WriteLine("Your subscription has expired! Renew now to continue using the software.");
}
else if (daysUntilExpiration <= 5)
{
    discountPercentage = 20;
    Console.WriteLine($"Your subscription is about to expire! Renew now and get a {discountPercentage}% discount!");
}
else if (daysUntilExpiration <= 15)
{
    discountPercentage = 10;
    Console.WriteLine($"Your subscription will expire soon. Renew now and get a {discountPercentage}% discount!");
}
else
{
    Console.WriteLine("Your subscription is active. No need to worry about renewal just yet.");
}
