using System.Linq.Expressions;
using Atoz;
using Atoz.EFCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TestApp.Console;

internal class Program
{
    public static void Main(string[] args)
    {

        System.Console.WriteLine($"Hello, {typeof(Program).FullName}");

        bool res = CallExpression(
            (int x) => x == 0,
            0);

        Expression<Func<Foo, bool>> filter1 = (o) => o.Name.StartsWith("Joe");
        Expression<Func<Foo, bool>> filter2 = (x) => x.Id == 1;

        var both = filter1.AndAlso(filter2);

        var f = new Foo(1, "Trump");

        // filter1.Compile()(f) && filter2.Compile()(f);
        bool joe = both.Compile()(f);


        //DateTime dt_local = DateTime.Now;
        //var dt_utc = dt_local.ToUniversalTime();

        //var dt_zone_local = dt_utc.ToDateTimeOffset(TimeSpan.FromHours(-8)); //seattle time
        //dt_zone_local = dt_local.ToDateTimeOffset(TimeSpan.FromHours(8)); // beijing time

        //var dt_unspec = DateTime.SpecifyKind(dt_local, DateTimeKind.Unspecified);
        //var dt_zone_unspec = dt_unspec.ToDateTimeOffset();


        //var dt1 = dt_zone_local.ToDateTime(DateTimeKind.Utc); // utc
        //dt1 = dt_zone_local.ToDateTime(DateTimeKind.Local); // local
        //dt1 = dt_zone_local.ToDateTime(DateTimeKind.Unspecified); // local

        //string[] dt_strs = DateTime.Now.GetDateTimeFormats(CultureInfo.InvariantCulture);

        //foreach (string str in dt_strs)
        //{
        //    DateTime dt = str.ConvertTo<DateTime>();
        //    System.Console.WriteLine(dt);
        //}

        //var color = "1".ConvertTo<Color>();

        //string s = "[1, null, 3]";
        //var res = s.ConvertTo<int[]>();

        //DateTimeOffset time = DateTimeOffset.Now;
        //var g = time.ToString().ConvertTo<DateTimeOffset>();



        System.Console.ReadLine();
        System.Console.WriteLine("Bye!");
    }

    static bool CallExpression(Expression<Func<int, bool>> expression, int arg)
    {
        var lambda = expression.Compile();
        return lambda(arg);
        //return lambda.Invoke(arg);
    }
}

public enum Color
{
    Red, Green, Blue
}

class Foo
{
    public int Id { get; set; }
    public string Name { get; set; }

    public Foo(int id, string name)
    {
        Id = id;
        Name = name;
    }
}
