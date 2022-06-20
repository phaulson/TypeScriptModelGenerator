using System;
using System.Collections.Generic;
using CSharpTypescriptConverter.Attributes;

[assembly: TypeScriptModel(Name = "IUser")]

namespace CSharpTypeScriptConverterConsoleApplication.Input;

[TypeScriptModel(Name = "IUser")]
public class User
{
    public int Age { get; set; }
    public IEnumerable<string> Tags { get; set; }
        
    [TypeScriptModel]
    public IDictionary<string, List<int>> Tags2 { get; set; }
        
    [TypeScriptModel(Name = "Hello")]
    public (int, string, bool, string, byte) Test { get; set; }
    
    public DateTime DateLol { get; set; }
}