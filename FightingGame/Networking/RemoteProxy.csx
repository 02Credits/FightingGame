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
using System.Threading.Tasks;
using Networking;
using FightingGame.ViewModels;

namespace FightingGame.Networking
{
    public class RemoteProxy : RemoteProxyBase
    {        
        public RemoteProxy(NetPeer peer, NetConnection networkConnection)
            : base(peer, networkConnection) { }
");

bool first = true;
foreach (var method in methods)
{
    var typeParameters = new List<string>();
    string returnTypeText;
    if (method.ReturnType.ToString() != "void")
    {
        returnTypeText = "Task<" + method.ReturnType.ToString() + ">";
        typeParameters.Add(method.ReturnType.ToString());
    }
    else
    {
        returnTypeText = "Task<object>";
        typeParameters.Add("object");
    }

    var methodParameters = method.ParameterList.Parameters;
    var parameters = new List<string>();
    var sendParameters = new List<string>();
    foreach (var param in methodParameters)
    {
        var paramType = param.Type.ToString();
        var paramIdentifier = param.Identifier.ToString();
        if (paramType == "RemoteProxy" || (paramType == "double" && paramIdentifier == "sendTime")) continue;

        parameters.Add(param.ToString());
        typeParameters.Add(paramType);
        sendParameters.Add(paramIdentifier);
    }

    var typeParameterText = "";
    if (typeParameters.Any()) typeParameterText = $"<{string.Join(", ", typeParameters)}>";
    var sendParameterText = "";
    if (sendParameters.Any()) sendParameterText = $", {string.Join(", ", sendParameters)}";

    if (first) first = false;
    else Output.WriteLine();

    Output.Write($@"
        public {returnTypeText} {method.Identifier.ToString()}({string.Join(", ", parameters)})
        {{
            return SendCommand{typeParameterText}(""{method.Identifier.ToString()}""{sendParameterText});
        }}");
}

Output.Write(@"
    }
}")