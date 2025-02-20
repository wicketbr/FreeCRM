/// <summary>
/// Static class used to generate random passwords of varying lengths with varying complexity requirements.
/// </summary>
public static class PasswordGenerator
{
    private static string lettersUpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static string lettersLowerCase = "abcdefghijklmnopqrstuvwxyz";
    private static string numbers = "1234567890";
    private static string specialCharacters = "!@#$%^&*()-+=/";
    private static Random randomNumberGenerator = new Random();

    /// <summary>
    /// Generates a random password of a given length. Optionally you can limit the types of characters required. If all are set to false then all options are used.
    /// <param name="length">The length of the password to generate. If no value is specified, or a value less than 5 is specified, then a length of 32 will be used.</param>
    /// <param name="options">Optional PasswordOptions object. Each property defaults to True, so you only need to set items to False that you don't wish to include.</param>
    /// </summary>
    /// <returns>A randomly generated password.</returns>
    public static string Generate(int length = 32, PasswordOptions? options = null)
    {
        string output = "";

        if (length < 5) { length = 32; }

        PasswordOptions opts = new PasswordOptions {
            RequireUpperCase = true,
            RequireLowerCase = true,
            RequireNumbers = true,
            RequireSpecialCharacters = true,
        };
        if (options != null) {
            opts = options;
        }


        string allCharacters = "";
        if (opts.RequireUpperCase) { allCharacters += lettersUpperCase; }
        if (opts.RequireLowerCase) { allCharacters += lettersLowerCase; }
        if (opts.RequireNumbers) { allCharacters += numbers; }
        if (opts.RequireSpecialCharacters) { allCharacters += specialCharacters; }
        if (String.IsNullOrEmpty(allCharacters)) { allCharacters = lettersUpperCase + lettersLowerCase + numbers + specialCharacters; }

        int lowerpass = 0;
        int upperpass = 0;
        int numpass = 0;
        int specialcharpass = 0;
        string positionArray = "0123456789";

        if (length < positionArray.Length) {
            positionArray = positionArray.Substring(0, length);
        }

        lowerpass = getRandomPosition(ref positionArray);
        upperpass = getRandomPosition(ref positionArray);
        numpass = getRandomPosition(ref positionArray);
        specialcharpass = getRandomPosition(ref positionArray);

        for (int i = 0; i < length; i++) {
            if (i == lowerpass && opts.RequireUpperCase) {
                output += getRandomChar(lettersUpperCase);
            } else if (i == upperpass && opts.RequireLowerCase) {
                output += getRandomChar(lettersLowerCase);
            } else if (i == numpass && opts.RequireNumbers) {
                output += getRandomChar(numbers);
            } else if (i == specialcharpass && opts.RequireSpecialCharacters) {
                output += getRandomChar(specialCharacters);
            } else {
                output += getRandomChar(allCharacters);
            }
        }

        return output;
    }

    private static string getRandomChar(string fullString)
    {
        return fullString.ToCharArray()[(int)Math.Floor(randomNumberGenerator.NextDouble() * fullString.Length)].ToString();
    }

    private static int getRandomPosition(ref string positionArray)
    {
        int output;

        string randomChar = positionArray.ToCharArray()[(int)Math.Floor(randomNumberGenerator.NextDouble() * positionArray.Length)].ToString();
        output = int.Parse(randomChar);
        positionArray = positionArray.Replace(randomChar, "");

        return output;
    }

    /// <summary>
    /// Password Generation Options
    /// </summary>
    public class PasswordOptions
    {
        /// <summary>
        /// Require UpperCase Characters (defaults to true)
        /// </summary>
        public bool RequireUpperCase { get; set; } = true;

        /// <summary>
        /// Require LowerCase Characters (defaults to true)
        /// </summary>
        public bool RequireLowerCase { get; set; } = true;

        /// <summary>
        /// Require Numbers (defaults to true)
        /// </summary>
        public bool RequireNumbers { get; set; } = true;

        /// <summary>
        /// Require Special Characters (defaults to true)
        /// </summary>
        public bool RequireSpecialCharacters { get; set; } = true;
    }
}