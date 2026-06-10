#pragma warning disable SA1208 // System using directives should be placed before other using directives
#pragma warning disable SA1210 // Using directives should be ordered alphabetically by namespace
// using directives that are needed in all analyzers
global using System.Collections.Immutable;
// using directives that are needed in all code fix providers
global using System.Composition;

global using Microsoft.CodeAnalysis;
global using Microsoft.CodeAnalysis.CodeActions;
global using Microsoft.CodeAnalysis.CodeFixes;
global using Microsoft.CodeAnalysis.CSharp;
global using Microsoft.CodeAnalysis.CSharp.Syntax;
global using Microsoft.CodeAnalysis.Diagnostics;
#pragma warning restore SA1210 // Using directives should be ordered alphabetically by namespace
#pragma warning restore SA1208 // System using directives should be placed before other using directives
