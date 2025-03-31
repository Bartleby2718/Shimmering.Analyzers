# Shimmering.Analyzers

`Shimmering.Analyzers` is an opinionated NuGet package consisting of [Roslyn analyzers](https://learn.microsoft.com/en-us/visualstudio/code-quality/roslyn-analyzers-overview), written to promote best practices in .NET and enforce certain code styles.

## Installation
### Option 1: Through your IDE of choice
### Option 2: Through .NET CLI (.NET Core 3.1+)
```console
dotnet add package Shimmering.Analyzers
```
### Option 3: Manually
You can add the following `PackageReference` item to an `ItemGroup` element in your `csproj` file:
```xml
<PackageReference Include="Shimmering.Analyzers" Version="1.0.0-alpha07">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

## How To Use
After you've installed `Shimmering.Analyzers`, you may see additional output in `Error List` of your IDE or build output, if there is any offending code. IF a code fix is available for the rule, you can apply the code fix to improve the code.

## Configuration
If you want to customize the severity of the analyzers, you can use `.editorconfig` to do so.

For example, if you don't like the default severity of `SHIMMER1000`, you can adjust it like this:
```ini
[*.cs]
dotnet_diagnostic.SHIMMER1000.severity = info
```

Alternatively, if you like all of the rules, you can bulk-configure the severity of all rules in the same category like this:
```ini
[*.cs]
dotnet_analyzer_diagnostic.category-ShimmeringUsage.severity = error
dotnet_analyzer_diagnostic.category-ShimmeringStyle.severity = error
```

## Motivation
This NuGet package was initially inspired by [@madelson](https://github.com/madelson)'s internal documentation ("Mike Cop") on .NET best practices that he created during his time at [@mastercard](https://github.com/mastercard). [@Bartleby2718](https://github.com/Bartleby2718) obtained [@mastercard](https://github.com/mastercard)'s approval to publish an open-source NuGet package enforcing some of those rules through Roslyn analyzers, hoping to share these best practices more widely with the broader .NET community.

## List of All Rules
Please see [site/docs/AllRules.md](site/docs/AllRules.md).

## Contributing
Please see [site/docs/CONTRIBUTING.md](site/docs/CONTRIBUTING.md).