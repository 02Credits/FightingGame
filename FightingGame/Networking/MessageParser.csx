﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(ProjectFilePath, "..", "Networking", "Methods.cs")));
var root = (CompilationUnitSyntax)tree.GetRoot();
var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

Output.Write(@"//Generated code. Manual changes will be clobbered
using Lidgren.Network;
using System;
using System.Collections.Generic;
using FightingGame.ViewModels;

namespace FightingGame.Networking
{
    public class MessageParser
    {
        private NetworkManagerBase _networkManager;
        private Methods _methods;
        private Dictionary<string, Func<NetIncomingMessage, byte[]>> _parsers;

        public MessageParser(NetworkManagerBase networkManager, Methods methods)
        {
            _networkManager = networkManager;
            _methods = methods;
            _parsers = new Dictionary<string, Func<NetIncomingMessage, byte[]>>();
            PopulateParsers();
        }

        public byte[] ParseMessage(string command, NetIncomingMessage msg)
        {
            return _parsers[command](msg);
        }

        private void PopulateParsers()
        {");

foreach (var method in methods)
{
    var parameters = method.ParameterList.Parameters;
    Output.Write($@"
            _parsers[""{method.Identifier.ToString()}""] = (lidgrenMessage) =>
            {{
                var timeSent = lidgrenMessage.ReadTime(false);");
    var lambdaTypes = new List<string>();
    var lambdaParameters = new List<string>();
    var types = new List<string>();

    foreach (var param in parameters)
    {
        var paramType = param.Type.ToString();
        if (paramType == "RemoteProxy")
        {
            lambdaParameters.Add("_networkManager.Proxies[lidgrenMessage.SenderConnection]");
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
                return _networkManager.ExecuteMethodFromMessage(lidgrenMessage, methodExecutor);
            }};");
}

Output.Write(@"
        }
    }
}");