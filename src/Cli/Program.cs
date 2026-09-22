using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using Core;

Console.OutputEncoding = Encoding.UTF8;

EnvironmentReport report = EnvironmentInfo.Collect();
bool asJson = args.Contains("--json");

const string Student = "Іванчук Марина, група ФЕІ-35";
const string Domain = "Склад (товари, партії, залишки, переміщення)";

if (asJson)
{
    var data = new
    {
        Student,
        report.OsDescription,
        report.OsVersion,
        report.ProcessArchitecture,
        report.DotNetVersion,
        Runtime = report.FrameworkDescription,
        report.DetectedRid,
        report.ReportedRid,
        report.BuildNote,
        AppDirectory = report.BaseDirectory,
        CurrentDirectory = report.CurrentDirectory,
        Domain
    };
    Console.WriteLine(JsonSerializer.Serialize(data, new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    }));
}
else
{
    Console.WriteLine("CrossApp – практикум з крос-платформного програмування");
    Console.WriteLine($"Студент: {Student}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"ОС (OSDescription) : {report.OsDescription}");
    Console.WriteLine($"ОС (Environment) : {report.OsVersion}");
    Console.WriteLine($"Архітектура процесу : {report.ProcessArchitecture}");
    Console.WriteLine($"Версія .NET (CLR) : {report.DotNetVersion}");
    Console.WriteLine($"Runtime : {report.FrameworkDescription}");
    Console.WriteLine($"RID (визначено) : {report.DetectedRid}");
    Console.WriteLine($"RID (від .NET) : {report.ReportedRid}");
    Console.WriteLine($"Каталог застосунку : {report.BaseDirectory}");
    Console.WriteLine($"Поточний каталог : {report.CurrentDirectory}");
    Console.WriteLine($"Build note : {report.BuildNote}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"Предметна область: {Domain}");
}