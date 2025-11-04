//Display a welcome message
CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
CancellationToken cancellationToken = cancellationTokenSource.Token;
Totp totp = new Totp();
totp.FirstTimeInit();

//Task.Run(async () => LockMouse.HandleMouse(cancellationToken));

Console.WriteLine("Welcome to the Access Blocker Console Prototype");
Console.WriteLine("Please enter the code to unlock the PC: ");
string? code = Console.ReadLine();
while (code != totp.VerifyTotp(code, totp.Secret, totp.DIGITS, totp.PERIOD, totp.ALGORITHM, allowedTimeSteps: 1)) {
    Console.WriteLine("The code is incorrect");
    Console.WriteLine("Please enter the code to unlock the PC: ");
    code = Console.ReadLine();
}
//cancellationTokenSource.Cancel();
Autostart.RemoveFromStartup();
Console.WriteLine("The code is correct");
Console.WriteLine("Press any key to exit");
Console.ReadKey();
