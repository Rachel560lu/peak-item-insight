using Mono.Cecil;
using Mono.Cecil.Cil;

var assemblyPath = args.FirstOrDefault(a => a.EndsWith(".dll")) ?? @"D:\SteamLibrary\steamapps\common\PEAK\PEAK_Data\Managed\Assembly-CSharp.dll";
var module = ModuleDefinition.ReadModule(assemblyPath);
var requested = args.Where(a => !a.EndsWith(".dll")).ToArray();

if (requested.Length > 0)
{
    foreach (var type in module.GetTypes().Where(t => requested.Any(r => t.FullName.Contains(r))))
    {
        Console.WriteLine($"TYPE {type.FullName} : {type.BaseType}");
        foreach (var field in type.Fields) Console.WriteLine($"FIELD {field.FullName}");
        foreach (var method in type.Methods)
        {
            Console.WriteLine($"METHOD {method.FullName}");
            if (method.HasBody) foreach (var instruction in method.Body.Instructions) Console.WriteLine(instruction);
        }
    }
    return;
}

foreach (var type in module.Types.Where(t =>
             t.Name is "Interaction" or "Item" ||
             t.Interfaces.Any(i => i.InterfaceType.Name == "IInteractible")))
{
    Console.WriteLine($"\n=== {type.FullName} : {type.BaseType} ===");
    Console.WriteLine("Interfaces: " + string.Join(", ", type.Interfaces.Select(i => i.InterfaceType.FullName)));
    foreach (var field in type.Fields)
        Console.WriteLine($"FIELD {field.Attributes} {field.FieldType.FullName} {field.Name}");

    foreach (var method in type.Methods.Where(m =>
                 type.Name == "Interaction" ||
                 m.Name.Contains("Interact", StringComparison.OrdinalIgnoreCase) ||
                 m.Name.Contains("Hover", StringComparison.OrdinalIgnoreCase)))
    {
        Console.WriteLine($"METHOD {method.FullName}");
        if (!method.HasBody) continue;
        foreach (var instruction in method.Body.Instructions)
        {
            var operand = instruction.Operand?.ToString() ?? "";
            if (type.Name == "Interaction" || operand.Contains("currentHovered") || operand.Contains("Item"))
                Console.WriteLine($"  {instruction.Offset:X4}: {instruction.OpCode} {operand}");
        }
    }
}
