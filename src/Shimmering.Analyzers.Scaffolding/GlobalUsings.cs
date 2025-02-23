#pragma warning disable SA1208 // System using directives should be placed before other using directives
#pragma warning disable SA1210 // Using directives should be ordered alphabetically by namespace
// using directives that are needed in all analyzers
global using System.Collections.Immutable;
global using Microsoft.CodeAnalysis;
global using Microsoft.CodeAnalysis.CSharp.Syntax;
global using Microsoft.CodeAnalysis.Diagnostics;

// using directives that are needed in all code fix providers
global using Microsoft.CodeAnalysis.CodeActions;
global using Microsoft.CodeAnalysis.CodeFixes;
#pragma warning restore SA1210 // Using directives should be ordered alphabetically by namespace
#pragma warning restore SA1208 // System using directives should be placed before other using directives

// using directives that are needed in all test files
global using Microsoft.CodeAnalysis.CSharp.Testing;
global using Microsoft.CodeAnalysis.Testing;
global using NUnit.Framework;
