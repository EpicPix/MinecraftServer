using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;

namespace MinecraftServer.SourceGenerators;

[Generator]
public class BlockStateGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        foreach (var file in context.AdditionalFiles)
        {
            if (Path.GetFileName(file.Path).Equals("blocks.json"))
            {
                context.AddSource("BlockState.g.cs", GenerateBlockStates(file.GetText().ToString()));
            }
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        
    }

    private StringBuilder runner = new();
    
    public string ToUpperCamelCase(string str)
    {
        runner.Clear();
        var up = true;
        foreach (var character in str)
        {
            if (character == '_')
            {
                up = true;
            } else
            {
                runner.Append(up ? char.ToUpper(character) : character);
                up = false;
            }
        }
        return runner.ToString();
    }


    public String GenerateBlockStates(string rawJson)
    {
        var json = JsonDocument.Parse(rawJson).RootElement;

        var source = new StringBuilder();

        source.AppendLine("// Auto-generated code");
        source.AppendLine("using System;");
        source.AppendLine();
        source.AppendLine("namespace MinecraftServer.Data");
        source.AppendLine("{");
        source.AppendLine("    public readonly partial struct BlockState {");
        source.AppendLine("        public static readonly IList<BlockState> States;");
        var switchSource = new StringBuilder();
        var initSource = new StringBuilder();
        uint iid = 0;
        foreach (var c in json.EnumerateObject())
        {
            foreach (var state in c.Value.GetProperty("states").EnumerateArray())
            {
                
                if (state.TryGetProperty("default", out var prop) && prop.GetBoolean())
                {
                    switchSource.AppendLine($"                case \"{c.Name}\": return States[{iid}];");
                    var v = ToUpperCamelCase(c.Name.Replace("minecraft:", ""));
                    source.AppendLine($"        public static readonly BlockState {v};");
                    initSource.AppendLine($"            {v} = States[{iid}];");
                }
                iid++;
            }
        }
        source.AppendLine("        public static BlockState GetBlockState(string id) {");
        source.AppendLine("            switch(id)");
        source.AppendLine("            {");
        source.AppendLine(switchSource.ToString());
        source.AppendLine("                default: return States[0];");
        source.AppendLine("            }");
        source.AppendLine("        }");
        source.AppendLine("        static BlockState() {");
        source.AppendLine($"            var states = new BlockState[{iid}];");
        uint id = 0;
        foreach (var c in json.EnumerateObject())
        {

            foreach (var state in c.Value.GetProperty("states").EnumerateArray())
            {

                source.AppendLine($"            states[{id}] = new BlockState({state.GetProperty("id")});");
                id++;
            }
        }
        source.AppendLine("            States = Array.AsReadOnly<BlockState>(states);");
        source.AppendLine(initSource.ToString());
        source.AppendLine("        }");
        source.AppendLine("    }");
        source.AppendLine("}");
        return source.ToString();
    }
}
