﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MinecraftServer.SourceGeneration.Events
{
    [Generator]
    public class EventBusSourceGenerator : ISourceGenerator
    {
        // private int _busId;
        // public EventBusSourceGenerator(int busId)
        // {
        //     _busId = busId;
        // }
        private static StringBuilder sb = new();
        private static GeneratorExecutionContext Context;
        private static Compilation Compile;
        private static Dictionary<int, (EventBusAttribute, TypeDeclarationSyntax)> _busses = new();

        public void Execute(GeneratorExecutionContext context)
        {
            Context = context;
            Compile = context.Compilation;
            var busses = GetEventBusses(context.Compilation);
            foreach (var bus in busses)
            {
                sb.AppendLine(bus.Item1.BusId + "");
                var busAttribute = bus.Item1;
                if (_busses.ContainsKey(busAttribute.BusId))
                {
                    WriteError(Location.None, $"Duplicate event bus found.",
                        $"There cannot be two of the same event busses of id {busAttribute.BusId}.");
                }
                else
                {
                    _busses[busAttribute.BusId] = bus;
                }
            }

            try
            {
                foreach (var data in GetEventHandlers(context.Compilation))
                {
                    sb.AppendLine($"{data.Item1.Priority} {data.Item1.BusId} {data.Item1.HandledData} {data.Item2}");
                }
            }
            catch (Exception e)
            {
                sb.AppendLine(e.Message + " " + e.StackTrace);
            }


            File.WriteAllText(@"D:\encodeous\MinecraftServer\MinecraftServer\log.txt", sb.ToString());
        }

        public void WriteEventBus(GeneratorExecutionContext context, TypeDeclarationSyntax decl, (EventBusAttribute, TypeDeclarationSyntax) bus, ImmutableArray<(EventHandlerAttribute, string)> handlers)
        {
            var model = Compile.GetSemanticModel(decl.SyntaxTree);
            var classSymbol = model.GetDeclaredSymbol(decl);
            var ns = classSymbol.ContainingNamespace;
            var name = classSymbol.Name;

            var busType = decl is ClassDeclarationSyntax ? "class" : "struct";

            var builder = new StringBuilder();
            builder.AppendLine(@"// Auto Generated Code
using System.Threading.Tasks;");
            if (ns != null)
            {
                builder.AppendLine($"namespace {ns} {{");
            }

            builder.Append($@"partial {busType} {name} {{
public static ValueTask PostEventAsync<T>(T data, DataBus instance)
    {{
        return data switch
        {{");
            
            CsHandshake c => MinecraftServer.DataBus.thing(c, instance),
            int d => MinecraftServer.DataBus.thing(d, instance),
            
            builder.Append(@"
            _ => throw new InvalidOperationException(" + "\"The specified data cannot be posted. No registered event handler is able to handle that type of data!\""+ @")
        }
    }
};
}
    
");
            if (ns != null)
            {
                builder.AppendLine($"}}");
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one
        }
        
        
        
        
        private static ImmutableArray<(EventHandlerAttribute, string)> GetEventHandlers(Compilation compilation)
        {
            // Get all classes
            IEnumerable<SyntaxNode> allNodes = compilation.SyntaxTrees.SelectMany(s => s.GetRoot().DescendantNodes());
            IEnumerable<MethodDeclarationSyntax> allMethods = allNodes
                .Where(d => d.IsKind(SyntaxKind.MethodDeclaration))
                .OfType<MethodDeclarationSyntax>();
        
            return allMethods
                .Select(component => GetEventHandlerAttribute(compilation, component))
                .Where(page => page.Item1 != null)
                .OrderByDescending(x => x.Item1.Priority)
                .ToImmutableArray();
        }
        
        private static ImmutableArray<(EventBusAttribute, TypeDeclarationSyntax)> GetEventBusses(Compilation compilation)
        {
            // Get all classes
            IEnumerable<SyntaxNode> allNodes = compilation.SyntaxTrees.SelectMany(s => s.GetRoot().DescendantNodes());
            IEnumerable<TypeDeclarationSyntax> allMethods = allNodes
                .Where(d => d.IsKind(SyntaxKind.ClassDeclaration) || d.IsKind(SyntaxKind.StructDeclaration))
                .OfType<TypeDeclarationSyntax>();
        
            return allMethods
                .Select(component => GetEventBus(compilation, component))
                .Where(page => page.Item1 != null)
                .ToImmutableArray();
        }

        private static string GetBusType(int busId)
        {
            if (!_busses.ContainsKey(busId)) return null;
            var model = Compile.GetSemanticModel(_busses[busId].Item2.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(_busses[busId].Item2);
            return symbol.ToDisplayString();
        }

        public static (EventBusAttribute, TypeDeclarationSyntax) GetEventBus(Compilation compilation,
            TypeDeclarationSyntax component)
        {
            var semanticModel = compilation.GetSemanticModel(component.SyntaxTree);
            var attribute = component.AttributeLists
                .SelectMany(x => x.Attributes)
                .Where(attr =>
                {
                    sb.AppendLine(GetName(attr, semanticModel));
                    return GetName(attr, semanticModel) == typeof(EventBusAttribute).FullName;
                }).ToList();
            if (!attribute.Any()) return (null, null);
            var args = attribute[0].ArgumentList.Arguments;
            return (new EventBusAttribute(int.Parse(GetConstantValue(semanticModel, args[0].Expression))), component);
        }
        
        private static (EventHandlerAttribute, string) GetEventHandlerAttribute(Compilation compilation,
            MethodDeclarationSyntax component)
        {
            var semanticModel = compilation.GetSemanticModel(component.SyntaxTree);
            var attribute = component.AttributeLists
                .SelectMany(x => x.Attributes)
                .Where(attr =>
                {
                    return GetName(attr, semanticModel) == typeof(EventHandlerAttribute).FullName;
                }).ToList();
            if (!attribute.Any()) return (null, "");

            if (!CheckRequirement(component, "static")) return (null, "");
            var param = component.ParameterList.Parameters;
            if (param.Count < 2)
            {
                WriteError(component.GetLocation(), $"EventHandler cannot be applied the specified method signature.", 
                    $"Method {component.Identifier.ToFullString()} must have two parameters.");
                return (null, "");
            }

            var args = attribute[0].ArgumentList.Arguments;
            var data = new EventHandlerAttribute(int.Parse(GetConstantValue(semanticModel, args[0].Expression)),
                GetName(args[1].Expression, semanticModel),
                int.Parse(GetConstantValue(semanticModel, args[2].Expression)));

            var eventType = semanticModel.GetSymbolInfo(param[0].Type).Symbol.ToString();
            if (eventType != data.HandledData)
            {
                WriteError(component.GetLocation(), $"EventHandler cannot be applied the specified method signature.", 
                    $"Method {component.Identifier.ToFullString()} accepts the event type {eventType} in the first parameter that does not match the type of {data.HandledData} specified in the attribute.");
                return (null, "");
            }
            
            var calledType = semanticModel.GetSymbolInfo(param[1].Type).Symbol.ToString();
            var reqType = GetBusType(data.BusId);
            if (reqType == null)
            {
                WriteError(component.GetLocation(), $"EventBus cannot be found.", 
                    $"Method {component.Identifier.ToFullString()} is registered to the bus id {data.BusId} which cannot be found.");
                return (null, "");
            }
            if (calledType != reqType)
            {
                WriteError(component.GetLocation(), $"EventHandler cannot be applied the specified method signature.", 
                    $"Method {component.Identifier.ToFullString()} accepts the bus type {calledType} in the second parameter that does not match the type of {reqType}.");
                return (null, "");
            }

            return (data, GetName(component, semanticModel) + "." + component.Identifier.Text);
        }

        private static bool CheckRequirement(MethodDeclarationSyntax method, string requirement)
        {
            if (method.Modifiers.All(x => x.Text != requirement))
            {
                WriteError(method.GetLocation(), $"EventHandler cannot be applied to a non-{requirement} method", 
                    $"Method {method.Identifier.ToFullString()} is not {requirement}. All event handlers must be {requirement}.");
                return false;
            }

            return true;
        }

        private static void WriteError(Location loc, string msg, string detail)
        {
            Context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("EventBus-01", 
                    msg, 
                    detail,
                    "EventHandler",
                    DiagnosticSeverity.Error,
                    true
                ), loc)
            );
        }

        public static string GetName(SyntaxNode node, SemanticModel model)
        {
            if (node is TypeOfExpressionSyntax tof)
            {
                return model.GetSymbolInfo(tof.Type).Symbol.ToString();
            }

            var realSymbol = model.GetSymbolInfo(node).Symbol ?? model.GetDeclaredSymbol(node);
            return realSymbol.ContainingType
                .ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
        }
        
        private static string GetConstantValue(SemanticModel model, ExpressionSyntax expr)
        {
            return model.GetConstantValue(expr).ToString();
        }
        
        public string GenerateBusHandler(Compilation compilation)
        {
            var nodes = compilation.SyntaxTrees.SelectMany(s => s.GetRoot().DescendantNodes());
            return "";
        }
    }
}