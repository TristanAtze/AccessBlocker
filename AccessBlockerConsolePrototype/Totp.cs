using System.Security.Cryptography;
using System.Text;

public class Totp {
    //Properties
    public string SECRET_FILE_NAME { get; set; }
    public string QRCODE_FOLDER_NAME { get; set; }
    public int DIGITS { get; set; }
    public int PERIOD { get; set; }
    public string ALGORITHM { get; set; }
    public string Secret { get; set; }


    //Constructor
    public Totp(string SECRET_FILE_NAME, string QRCODE_FOLDER_NAME, int DIGITS, int PERIOD, string ALGORITHM) {
        this.SECRET_FILE_NAME = SECRET_FILE_NAME;
        this.QRCODE_FOLDER_NAME = QRCODE_FOLDER_NAME;
        this.DIGITS = DIGITS;
        this.PERIOD = PERIOD;
        this.ALGORITHM = ALGORITHM;
    }

    //Constructor with default values
    public Totp() {
        this.SECRET_FILE_NAME = "secret.txt";
        this.QRCODE_FOLDER_NAME = "QRCODE";
        this.DIGITS = 6;
        this.PERIOD = 30;
        this.ALGORITHM = "SHA1";
    }

    public void FirstTimeInit(){
        //Check if the secret.txt file exists
        if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), SECRET_FILE_NAME))) {
            Secret = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), SECRET_FILE_NAME));
            return;
        }
        var secret = GenerateRandomSecret(20);
        var base32Secret = Base32Encode(secret);
        Console.WriteLine("Secret (base32): " + base32Secret);
        Console.WriteLine();
        string issuer = "AccessBlockerConsolePrototype";
        string account = "AccessBlockerConsolePrototype";

        string provisioningUri = GetTotpProvisioningUri(account, issuer, base32Secret, digits: DIGITS, period: PERIOD, algorithm: ALGORITHM);
        Console.WriteLine("Provisioning URI (use as QR payload):");
        Console.WriteLine(provisioningUri);
        QRCodeGenerate.GenerateQRCode(provisioningUri);
        QRCodeGenerate.DisplayQRCodeImage(Path.Combine(Directory.GetCurrentDirectory(), QRCODE_FOLDER_NAME, "QRCode.png"));
        Console.WriteLine("Please scan the QR code with your authenticator app");
        Console.WriteLine("Enter the code to verify: ");
        //Generate a random code        
        string code = Console.ReadLine();
        bool ok = VerifyTotp(code, secret, DIGITS, PERIOD, HashAlgorithmName.SHA1, allowedTimeSteps: 1);
        if (ok) {
            Console.WriteLine("Code is correct");
        } else {
            Console.WriteLine("Code is incorrect");
        }
        Console.WriteLine("Press any key continue");
        Console.ReadKey();
        File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), SECRET_FILE_NAME), base32Secret);
    }
    

    static byte[] GenerateRandomSecret(int length)
    {
        var bytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return bytes;
    }

    static string GenerateTotp(byte[] secret, int digits, int period, HashAlgorithmName alg, DateTimeOffset? timestamp = null)
    {
        timestamp ??= DateTimeOffset.UtcNow;
        long counter = timestamp.Value.ToUnixTimeSeconds() / period;
        return GenerateHotp(secret, counter, digits, alg);
    }

    static bool VerifyTotp(string code, byte[] secret, int digits, int period, HashAlgorithmName alg, int allowedTimeSteps = 1)
    {
        var now = DateTimeOffset.UtcNow;
        long currentCounter = now.ToUnixTimeSeconds() / period;

        for (long i = -allowedTimeSteps; i <= allowedTimeSteps; i++)
        {
            var candidate = GenerateHotp(secret, currentCounter + i, digits, alg);
            if (SecureEquals(candidate, code)) return true;
        }
        return false;
    }

    static bool SecureEquals(string a, string b)
    {
        if (a == null || b == null || a.Length != b.Length) return false;
        int diff = 0;
        for (int i = 0; i < a.Length; i++)
            diff |= a[i] ^ b[i];
        return diff == 0;
    }

    static string GenerateHotp(byte[] secret, long counter, int digits, HashAlgorithmName alg)
    {
        var counterBytes = new byte[8];
        for (int i = 7; i >= 0; i--)
        {
            counterBytes[i] = (byte)(counter & 0xFF);
            counter >>= 8;
        }

        byte[] hash;
        if (alg == HashAlgorithmName.SHA1)
        {
            using var hmac = new HMACSHA1(secret);
            hash = hmac.ComputeHash(counterBytes);
        }
        else if (alg == HashAlgorithmName.SHA256)
        {
            using var hmac = new HMACSHA256(secret);
            hash = hmac.ComputeHash(counterBytes);
        }
        else if (alg == HashAlgorithmName.SHA512)
        {
            using var hmac = new HMACSHA512(secret);
            hash = hmac.ComputeHash(counterBytes);
        }
        else throw new ArgumentException("Unsupported algorithm", nameof(alg));

        int offset = hash[hash.Length - 1] & 0x0F;
        int binaryCode =
            ((hash[offset] & 0x7F) << 24) |
            ((hash[offset + 1] & 0xFF) << 16) |
            ((hash[offset + 2] & 0xFF) << 8) |
            (hash[offset + 3] & 0xFF);

        int otp = binaryCode % (int)Math.Pow(10, digits);
        return otp.ToString(new string('0', digits));
    }

    static string GetTotpProvisioningUri(string accountName, string issuer, string base32Secret, int digits = 6, int period = 30, string algorithm = "SHA1")
    {
        string label = Uri.EscapeDataString($"{issuer}:{accountName}");
        var sb = new StringBuilder();
        sb.Append("otpauth://totp/");
        sb.Append(label);
        sb.Append("?secret=").Append(Uri.EscapeDataString(base32Secret));
        sb.Append("&issuer=").Append(Uri.EscapeDataString(issuer));
        sb.Append("&algorithm=").Append(Uri.EscapeDataString(algorithm));
        sb.Append("&digits=").Append(digits);
        sb.Append("&period=").Append(period);
        return sb.ToString();
    }

    public static string Base32Encode(byte[] input)
    {
        if (input == null || input.Length == 0)
        {
            throw new ArgumentNullException("input");
        }

        int charCount = (int)Math.Ceiling(input.Length / 5d) * 8;
        char[] returnArray = new char[charCount];

        byte nextChar = 0, bitsRemaining = 5;
        int arrayIndex = 0;

        foreach (byte b in input)
        {
            nextChar = (byte)(nextChar | (b >> (8 - bitsRemaining)));
            returnArray[arrayIndex++] = ValueToChar(nextChar);
            
            if (bitsRemaining < 4)
            {
                nextChar = (byte)((b >> (3 - bitsRemaining)) & 31);
                returnArray[arrayIndex++] = ValueToChar(nextChar);
                bitsRemaining += 5;
            }
            
            bitsRemaining -= 3;
            nextChar = (byte)((b << bitsRemaining) & 31);
        }

        if (arrayIndex != charCount)
        {
            returnArray[arrayIndex++] = ValueToChar(nextChar);
            while (arrayIndex != charCount) returnArray[arrayIndex++] = '='; 
        }

        return new string(returnArray);
    }
    private static int CharToValue(char c)
    {
        int value = (int)c;
        
        //65-90 == uppercase letters
        if (value < 91 && value > 64)
        {
            return value - 65;
        }
        //50-55 == numbers 2-7
        if (value < 56 && value > 49)
        {
            return value - 24;
        }
        //97-122 == lowercase letters
        if (value < 123 && value > 96)
        {
            return value - 97;
        }

        throw new ArgumentException("Character is not a Base32 character.", "c");
    }
     private static char ValueToChar(byte b)
    {
        if (b < 26)
        {
            return (char)(b + 65);
        }

        if (b < 32)
        {
            return (char)(b + 24);
        }

        throw new ArgumentException("Byte is not a value Base32 value.", "b");
    }
}