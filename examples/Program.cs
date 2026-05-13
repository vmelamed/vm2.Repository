#!/usr/bin/env dotnet

// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

#:property TargetFramework=net10.0
#:project ../src/Repository/Repository.csproj

using static System.Console;
using static System.Text.Encoding;

using vm2.Repository;

using static vm2.Repository.RepositoryApi;

Console.WriteLine("Repository example");

Console.WriteLine(Echo("hello", "fallback"));
Console.WriteLine(Echo(null, "fallback"));
