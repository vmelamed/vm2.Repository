// SPDX-License-Identifier: MIT

global using System;
global using System.Collections.Generic;
global using System.Collections.ObjectModel;
global using System.Diagnostics;
global using System.Linq;
global using System.Linq.Expressions;
global using System.Reflection;
global using System.Threading;
global using System.Threading.Tasks;

global using FluentValidation;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.ChangeTracking;
global using Microsoft.EntityFrameworkCore.Diagnostics;
global using Microsoft.EntityFrameworkCore.Infrastructure;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Query;
global using Microsoft.EntityFrameworkCore.ValueGeneration;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;

global using OpenTelemetry.Instrumentation.EntityFrameworkCore;
global using OpenTelemetry.Trace;

global using vm2.Common.Abstractions;
global using vm2.Repository.Abstractions;
global using vm2.Repository.EntityFramework.Models;
