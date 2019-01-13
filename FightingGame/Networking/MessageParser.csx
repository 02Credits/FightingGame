using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;

var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(ProjectFilePath, "..", "Networking", "Methods.cs")));
var root = (CompilationUnitSyntax)tree.GetRoot();
var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

const string PROXY_TYPE = "RemoteProxy";

Output.Write($@"//Generated code. Manual changes will be clobbered
using Lidgren.Network;
using System;
using System.Collections.Generic;
using FightingGame.ViewModels;
using Networking;

namespace FightingGame.Networking
{{
    public class MessageParser : MessageParserBase<{PROXY_TYPE}>
    {{
        private Methods _methods;
        private Dictionary<string, Func<NetIncomingMessage, {PROXY_TYPE}, byte[]>> _parsers;

        public MessageParser(Methods methods)
        {{
            _methods = methods;
            _parsers = new Dictionary<string, Func<NetIncomingMessage, {PROXY_TYPE}, byte[]>>();
            PopulateParsers();
        }}

        public override byte[] ParseMessage(string command, NetIncomingMessage msg, {PROXY_TYPE} proxy)
        {{
            return _parsers[command](msg, proxy);
        }}

        private void PopulateParsers()
        {{");

foreach (var method in methods)
{
    var parameters = method.ParameterList.Parameters;
    Output.Write($@"
            _parsers[""{method.Identifier.ToString()}""] = (lidgrenMessage, proxy) =>
            {{
                var timeSent = lidgrenMessage.ReadTime(false);");
    var lambdaTypes = new List<string>();
    var lambdaParameters = new List<string>();
    var types = new List<string>();

    foreach (var param in parameters)
    {
        var paramType = param.Type.ToString();
        if (paramType == PROXY_TYPE)
        {
            lambdaParameters.Add("proxy");
        }
        else if (paramType == "double" && param.Identifier.ToString() == "sendTime")
        {
            lambdaParameters.Add("timeSent");
        }
        else
        {
            var typeIdentifier = param.Identifier.ToString();
            lambdaParameters.Add(typeIdentifier);
            lambdaTypes.Add(typeIdentifier);
            types.Add(paramType);
        }
    }

    string lambdaType;
    if (method.ReturnType.ToString() != "void")
    {
        types.Add(method.ReturnType.ToString());
        lambdaType = $"Func<{string.Join(", ", types)}>";
    }
    else
    {
        lambdaType = "Action";
        if (types.Any())
        {
            lambdaType += $"<{string.Join(", ", types)}>";
        }
    }

    var lambdaTypesString = "";
    if (lambdaTypes.Any()) lambdaTypesString = string.Join(", ", lambdaTypes);
    var lambdaParametersString = "";
    if (lambdaParameters.Any()) lambdaParametersString = string.Join(", ", lambdaParameters);

    Output.Write($@"
                {lambdaType} methodExecutor = ({lambdaTypesString}) => _methods.{method.Identifier}({lambdaParametersString});
                return ExecuteMethodFromMessage(lidgrenMessage, methodExecutor);
            }};");
}

Output.Write(@"
        }
    }
}");