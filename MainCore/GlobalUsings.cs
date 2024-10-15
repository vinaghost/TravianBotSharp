﻿global using FluentResults;
global using HtmlAgilityPack;
global using Injectio.Attributes;
global using MainCore.Commands.Misc;
global using MainCore.Commands.Navigate;
global using MainCore.Commands.Queries;
global using MainCore.Commands.Update;
global using MainCore.Common.Enums;
global using MainCore.Common.Errors;
global using MainCore.Common.Extensions;
global using MainCore.Common.MediatR;
global using MainCore.DTO;
global using MainCore.Entities;
global using MainCore.Infrasturecture.Persistence;
global using MainCore.Notification.Message;
global using MainCore.Parsers;
global using MainCore.Services;
global using MediatR;
global using Microsoft.EntityFrameworkCore;
global using OpenQA.Selenium;
global using Splat;
global using System.Reactive.Linq;
global using ILogger = Serilog.ILogger;
global using Unit = System.Reactive.Unit;