using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace SIS.MvcFramework.ViewEngine
{
    public class ViewEngine : IViewEngine
    {
        public string GetHtml<T>(string viewName, string viewCode, T model, MvcUserInfo user = null)
        {

            string viewTypeName = viewName.Replace("/", "_").Replace("-", "_").Replace(".", "_") + "View"; //Класа не може да има / в името
            //viewCode => C# код
            string csharpMethodBody = this.GenerateCSharpMethodBody(viewCode);
            string viewCodeAsCSharpCode = @"
            using System;
            using System.Linq;
            using System.Text;
            using System.Collections.Generic;
            using SIS.MvcFramework; 
            using SIS.MvcFramework.ViewEngine;
            using " + typeof(T).Namespace + @";
            namespace MyAppViews
            {
                public class " + viewTypeName + " : IView<" + typeof(T).FullName.Replace("+", ".") + @">
                {
                    public string GetHtml(" + typeof(T).FullName.Replace("+", ".") + @" model, MvcUserInfo user)
                    {
                        StringBuilder html = new StringBuilder();
                        var Model = model;
                        var User = user;                        

                        " + csharpMethodBody + @"

                        return html.ToString().TrimEnd();
                    }
                }
            }
            "; //Накрая всяко вю става на отделен клас

            //C# => executable object.GetHtml(model)
            IView<T> instanceOfViewClass = this.GetInstance(viewCodeAsCSharpCode, "MyAppViews." + viewTypeName, typeof(T)) as IView<T>;
            string html = instanceOfViewClass.GetHtml(model, user);
            return html;
        }

        private object GetInstance(string cSharpCode, string typeName, Type viewModelType) //Този метод приема кода като стринг и връща обект
        {
            //Roslyn - библиотека, която позволява да работим със C# кода като синтактично дърво
            CSharpCompilation compilation = CSharpCompilation.Create(Path.GetRandomFileName() + ".dll")
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)) //DLL
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location))
                .AddReferences(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("mscorlib")).Location))
                .AddReferences(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("netstandard")).Location))
                .AddReferences(MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location))
                .AddReferences(MetadataReference.CreateFromFile(typeof(IEnumerable<>).GetTypeInfo().Assembly.Location))
                .AddReferences(MetadataReference.CreateFromFile(typeof(IView<>).GetTypeInfo().Assembly.Location))
                .AddReferences(MetadataReference.CreateFromFile(viewModelType.Assembly.Location));

            AssemblyName[] netStandardReferences = Assembly.Load(new AssemblyName("netstandard")).GetReferencedAssemblies();
            foreach (AssemblyName reference in netStandardReferences)
            {
                compilation = compilation.AddReferences(MetadataReference.CreateFromFile(Assembly.Load(reference).Location));
            }

            compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(cSharpCode));

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);
                if (!result.Success)
                {
                    IEnumerable<Diagnostic> errors = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);
                    foreach (Diagnostic diagnostic in errors)
                    {
                        Console.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }

                    return null;
                }

                ms.Seek(0, SeekOrigin.Begin);
                Assembly assembly = Assembly.Load(ms.ToArray());
                Type viewType = assembly.GetType(typeName);
                return Activator.CreateInstance(viewType);
            }
        }

        private string GenerateCSharpMethodBody(string viewCode)
        {
            IEnumerable<string> lines = this.GetLines(viewCode);
            StringBuilder sb = new StringBuilder();
            foreach (string line in lines)
            {
                if (line.Trim().StartsWith("{") && line.Trim().EndsWith("}")) //за ето този случай {var year = DateTime.UtcNow.Year;}
                {
                    string cSharpLine = line.Trim();
                    cSharpLine = cSharpLine.Substring(1, cSharpLine.Length - 2);
                    sb.AppendLine(cSharpLine);
                }
                else if (line.Trim().StartsWith("{") ||
                    line.Trim().StartsWith("}") ||
                    line.Trim().StartsWith("@for") ||
                    line.Trim().StartsWith("@if") ||
                    line.Trim().StartsWith("@else")) //CSharp
                {
                    int firstAtSymbolIndex = line.IndexOf("@", StringComparison.InvariantCulture);
                    sb.AppendLine(this.RemoveAt(line, firstAtSymbolIndex));
                }
                else
                {
                    string htmlLine = line.Replace("\"", "\\\"");
                    while (htmlLine.Contains("@"))
                    {
                        int specialSymbolIndex = htmlLine.IndexOf("@", StringComparison.InvariantCulture);
                        int endOfCode = new Regex(@"[\s&\""\+=()<\\!]+").Match(htmlLine, specialSymbolIndex).Index;
                        string expression = null;
                        if (endOfCode == 0 || endOfCode == -1)
                        {
                            expression = htmlLine.Substring(specialSymbolIndex + 1);
                            htmlLine = htmlLine.Substring(0, specialSymbolIndex) +
                                        "\" + " + expression + " + \"";
                        }
                        else
                        {
                            expression = htmlLine.Substring(specialSymbolIndex + 1, endOfCode - specialSymbolIndex - 1);
                            htmlLine = htmlLine.Substring(0, specialSymbolIndex) +
                                        "\" + " + expression + " + \"" + htmlLine.Substring(endOfCode);
                        }
                    }

                    sb.AppendLine($"html.AppendLine(\"{htmlLine}\");");
                }
            }

            return sb.ToString();
        }

        private IEnumerable<string> GetLines(string input)
        {
            StringReader stringReader = new StringReader(input);
            List<string> lines = new List<string>();

            string line = null;
            while ((line = stringReader.ReadLine()) != null)
            {
                lines.Add(line);
            }

            return lines;
        }

        private string RemoveAt(string input, int index)
        {
            if (index == -1)
            {
                return input;
            }

            return input.Substring(0, index) + input.Substring(index + 1);
        }
    }
}