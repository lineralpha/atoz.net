using Atoz;

namespace TestApp.Console;

internal class Program
{
    public static void Main(string[] args)
    {

        System.Console.WriteLine($"Hello, {typeof(Program).FullName}");

        //string[] dt_strs = DateTime.Now.GetDateTimeFormats(CultureInfo.InvariantCulture);

        //foreach (string str in dt_strs)
        //{
        //    DateTime dt = str.ConvertTo<DateTime>();
        //    System.Console.WriteLine(dt);
        //}

        var color = "1".ConvertTo<Color>();

        //string s = "[1, null, 3]";
        //var res = s.ConvertTo<int[]>();

        //DateTimeOffset time = DateTimeOffset.Now;
        //var g = time.ToString().ConvertTo<DateTimeOffset>();



        System.Console.ReadLine();
        System.Console.WriteLine("Bye!");
    }
}

public enum Color
{
    Red, Green, Blue
}
