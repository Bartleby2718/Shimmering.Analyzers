# Shimmering.Analyzers

## About this project
This repository hosts a NuGet package consisting of custom [Roslyn analyzers](https://learn.microsoft.com/en-us/visualstudio/code-quality/roslyn-analyzers-overview) written to promote best practices in .NET. If you install [`Shimmering.Analyzers`](https://www.nuget.org/packages/Shimmering.Analyzers) in your project and build the project in your IDE, the rules enabled by default will be shown, and you can apply the code fixes shown in your IDE. Feel free to apply the code fixes or adjust the severity of these rules by following the instructions [here](https://learn.microsoft.com/en-us/visualstudio/code-quality/roslyn-analyzers-overview).

## Motivation
This NuGet package was initially inspired by @madelson's internal documentation ("Mike Cop") on .NET best practices that he created during his time at @mastercard. @Bartleby2718 obtained @mastercard's approval to publish an open-source NuGet package enforcing some of those rules through Roslyn analyzers, hoping to share the best practices more widely with the broader .NET community.
