﻿[<AutoOpen>]
module Falco.Core

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Http

// ------------
// TaskBuilder.fs
// ------------
#nowarn "44"
type FSharp.Control.Tasks.TaskBuilder.TaskBuilderV2 with
    /// A bolt-on member to automatically convert Task<unit> result to Task
    member inline __.Run(f : unit -> FSharp.Control.Tasks.TaskBuilder.Step<unit>) = (FSharp.Control.Tasks.TaskBuilder.run f) :> Task

type FSharp.Control.Tasks.TaskBuilder.TaskBuilder with
    /// A bolt-on member to automatically convert Task<unit> result to Task
    member inline __.Run(f : unit -> FSharp.Control.Tasks.TaskBuilder.Step<unit>) = (FSharp.Control.Tasks.TaskBuilder.run f) :> Task

// ------------
// Constants
// ------------
module Constants =
    let defaultJsonOptions =
        let options = Text.Json.JsonSerializerOptions()
        options.AllowTrailingCommas <- true
        options.PropertyNameCaseInsensitive <- true 
        options

// ------------
// HTTP
// ------------

/// Http verb
type HttpVerb = 
    | GET 
    | HEAD
    | POST 
    | PUT 
    | PATCH
    | DELETE 
    | OPTIONS
    | TRACE
    | ANY

/// The eventual return of asynchronous HttpContext processing
type HttpHandler = 
    HttpContext -> Task

module HttpHandler =
    /// Convert HttpHandler to a RequestDelegate
    let toRequestDelegate (handler : HttpHandler) =        
        new RequestDelegate(handler)

/// In-and-out processing of a HttpContext
type HttpResponseModifier = HttpContext -> HttpContext

/// Specifies an association of an HttpHandler to an HttpVerb and route pattern
type HttpEndpoint = 
    {
        Pattern : string   
        Verb    : HttpVerb
        Handler : HttpHandler
    }

/// The process of associating a route and handler
type MapHttpEndpoint = string -> HttpHandler -> HttpEndpoint