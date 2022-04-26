using System;
using System.IO;
using System.Linq;
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


    public String GenerateBlockStates(string rawJson)
    {
        var json = JsonDocument.Parse(rawJson).RootElement;

        var source = new StringBuilder();

        source.AppendLine("// Auto-generated code");
        source.AppendLine("using System;");
        source.AppendLine();
        source.AppendLine("namespace MinecraftServer.Data");
        source.AppendLine("{");
        source.AppendLine("    public partial class BlockState {");
        source.AppendLine("        public static readonly IList<BlockState> States;");
        source.AppendLine("        public static readonly BlockState Air;");
        source.AppendLine("        public static BlockState GetBlockState(string id) {");
        source.AppendLine("            switch(id)");
        source.AppendLine("            {");
        uint iid = 0;
        foreach (var c in json.EnumerateObject())
        {
            // var alone = c.Value.GetProperty("states").GetArrayLength() == 1;
            foreach (var state in c.Value.GetProperty("states").EnumerateArray())
            {
                
                if (state.TryGetProperty("default", out var prop) && prop.GetBoolean())
                {
                    source.AppendLine($"                case \"{c.Name}\": return States[{iid}];");
                }
                // else
                // {
                //     source.AppendLine($"                case \"{c.Name},{state.GetProperty("id")}\": return States[{iid}];");
                // }
                iid++;
            }
        }
        source.AppendLine("                default: return Air;");
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
        source.AppendLine("            Air = GetBlockState(\"minecraft:air\");");
        source.AppendLine("        }");
        source.AppendLine("    }");
        source.AppendLine("}");
        return source.ToString();
    }
}
