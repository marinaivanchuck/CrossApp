using System.Runtime.InteropServices;
using System.Text;

// Встановлюємо кодування UTF-8 для консолі (щоб кирилиця відображалась коректно)
Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("CrossApp – практикум з крос-платформного програмування");
Console.WriteLine("Студент: Іванчук Марина, група ФЕІ-35");
Console.WriteLine(new string('-', 52));

Console.WriteLine($"ОС (OSDescription) : {RuntimeInformation.OSDescription}");
Console.WriteLine($"ОС (Environment) : {Environment.OSVersion}");
Console.WriteLine($"Архітектура процесу : {RuntimeInformation.ProcessArchitecture}");
Console.WriteLine($"Версія .NET (CLR) : {Environment.Version}");
Console.WriteLine($"Runtime : {RuntimeInformation.FrameworkDescription}");
Console.WriteLine($"Каталог застосунку : {AppContext.BaseDirectory}");
Console.WriteLine($"Поточний каталог : {Environment.CurrentDirectory}");

Console.WriteLine(new string('-', 52));
Console.WriteLine("Предметна область: Склад (товари, партії, залишки, переміщення)");
// Якщо обрали іншу область – змініть текст відповідно.