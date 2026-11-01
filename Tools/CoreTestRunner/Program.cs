using System;
using System.Reflection;
using System.IO;
using System.Text.Json;
using Moonpop.Core;
using Moonpop.Tests;
using NUnit.Framework;

public static class Program
{
    public static int Main(string[] args)
    {
        if(args.Length>0&&args[0]=="--export-art")
        {
            string path=args.Length>1?args[1]:"art.json";
            var data=new{camera=ArtRecipe.CameraPosition(0),look=ArtRecipe.CameraLook(),materials=ArtRecipe.Materials(),car=ArtRecipe.Car(0,true),traffic0=ArtRecipe.Car(0,false),traffic1=ArtRecipe.Car(1,false),traffic2=ArtRecipe.Car(2,false),road0=ArtRecipe.Road(0),road1=ArtRecipe.Road(1),road2=ArtRecipe.Road(2),backdrop=ArtRecipe.Backdrop(),ferris=ArtRecipe.FerrisWheel()};
            File.WriteAllText(path,JsonSerializer.Serialize(data,new JsonSerializerOptions{IncludeFields=true}));Console.WriteLine("Exported ArtRecipe to "+path);return 0;
        }
        int passed=0,failed=0;
        var suite=new CoreTests();
        foreach(var method in typeof(CoreTests).GetMethods())
        {
            if(!Attribute.IsDefined(method,typeof(TestAttribute)))continue;
            try{method.Invoke(suite,null);Console.WriteLine("PASS "+method.Name);passed++;}
            catch(Exception ex){Console.WriteLine("FAIL "+method.Name+": "+(ex.InnerException??ex).Message);failed++;}
        }
        Console.WriteLine("RESULT: "+passed+" passed, "+failed+" failed");
        return failed==0?0:1;
    }
}
