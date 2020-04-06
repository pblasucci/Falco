﻿[<AutoOpen>]
module Falco.ModelBinding

open System.Collections.Generic
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives

type StringCollectionReader (c : seq<KeyValuePair<string,StringValues>>) = 

    let coll = c |> Seq.map (fun (kvp) -> kvp.Key, kvp.Value) |> dict

    member _.TryGetValue (name : string) = 
        match name |> parseWith coll.TryGetValue with
        | Some v when v.Count > 0 -> Some v
        | _                       -> None
    
    member this.GetValue (name : string) = 
        match this.TryGetValue name with
        | Some v -> v 
        | None -> failwith (sprintf "Could not find %s" name)

    member this.TryGetString (name : string)           = name |> this.TryGetValue |> Option.bind (fun v -> Some v.[0])
    member this.TryGet                                 = this.TryGetString
    member this.TryGetInt16 (name : string)            = name |> this.TryGetValue |> Option.bind (fun v -> parseInt16 v.[0])
    member this.TryGetInt32 (name : string)            = name |> this.TryGetValue |> Option.bind (fun v -> parseInt32 v.[0])
    member this.TryGetInt                              = this.TryGetInt32
    member this.TryGetInt64 (name : string)            = name |> this.TryGetValue |> Option.bind (fun v -> parseInt64 v.[0])
    member this.TryGetBoolean (name : string)          = name |> this.TryGetValue |> Option.bind (fun v -> parseBoolean v.[0])
    member this.TryGetFloat (name : string)            = name |> this.TryGetValue |> Option.bind (fun v -> parseFloat v.[0])
    member this.TryGetDecimal (name : string)          = name |> this.TryGetValue |> Option.bind (fun v -> parseDecimal v.[0])
    member this.TryGetDateTime (name : string)         = name |> this.TryGetValue |> Option.bind (fun v -> parseDateTime v.[0])
    member this.TryGetDateTimeOffset (name : string)   = name |> this.TryGetValue |> Option.bind (fun v -> parseDateTimeOffset v.[0])
    member this.TryGetGuid (name : string)             = name |> this.TryGetValue |> Option.bind (fun v -> parseGuid v.[0])
    member this.TryGetTimeSpan (name : string)         = name |> this.TryGetValue |> Option.bind (fun v -> parseTimeSpan v.[0])       
    member this.TryArrayString (name : string)         = name |> this.TryGetValue |> Option.map  (fun v -> v.ToArray())
    member this.TryArrayInt16 (name : string)          = name |> this.TryGetValue |> Option.bind (parseArray parseInt16)
    member this.TryArrayInt32 (name : string)          = name |> this.TryGetValue |> Option.bind (parseArray parseInt32)
    member this.TryArrayInt                            = this.TryArrayInt32
    member this.TryArrayInt64 (name : string)          = name |> this.TryGetValue |> Option.bind (parseArray parseInt64)
    member this.TryArrayBoolean (name : string)        = name |> this.TryGetValue |> Option.bind (parseArray parseBoolean)
    member this.TryArrayFloat (name : string)          = name |> this.TryGetValue |> Option.bind (parseArray parseFloat)
    member this.TryArrayDecimal (name : string)        = name |> this.TryGetValue |> Option.bind (parseArray parseDecimal)
    member this.TryArrayDateTime (name : string)       = name |> this.TryGetValue |> Option.bind (parseArray parseDateTime)
    member this.TryArrayDateTimeOffset (name : string) = name |> this.TryGetValue |> Option.bind (parseArray parseDateTimeOffset)
    member this.TryArrayGuid (name : string)           = name |> this.TryGetValue |> Option.bind (parseArray parseGuid)
    member this.TryArrayTimeSpan (name : string)       = name |> this.TryGetValue |> Option.bind (parseArray parseTimeSpan)
        
let (?) (q : StringCollectionReader) = q.GetValue

type StringValues with 
    member this.AsString () =
        match this.Count with
        | 0 -> failwith "StringValues is empty"
        | _ -> this.[0]
    
    member this.AsInt16 ()               = this.AsString() |> parseOrFail parseInt16 "Not a valid Int16"
    member this.AsInt32 ()               = this.AsString() |> parseOrFail parseInt32 "Not a valid Int32"
    member this.AsInt                    = this.AsInt32
    member this.AsInt64 ()               = this.AsString() |> parseOrFail parseInt64 "Not a valid Int64"
    member this.AsBoolean ()             = this.AsString() |> parseOrFail parseBoolean "Not a valid Boolean"
    member this.AsFloat ()               = this.AsString() |> parseOrFail parseFloat "Not a valid Float"
    member this.AsDecimal ()             = this.AsString() |> parseOrFail parseDecimal "Not a valid Decimal"
    member this.AsDateTime ()            = this.AsString() |> parseOrFail parseDateTime "Not a valid DateTime"
    member this.AsDateTimeOffset ()      = this.AsString() |> parseOrFail parseDateTimeOffset "Not a valid DateTimeOffset"
    member this.AsGuid ()                = this.AsString() |> parseOrFail parseGuid "Not a valid Guid"
    member this.AsTimeSpan ()            = this.AsString() |> parseOrFail parseTimeSpan "Not a valid TimeSpan"
    member this.AsArrayString ()         = this.ToArray()
    member this.AsArrayInt16 ()          = this.AsArrayString() |> parseArray parseInt16 |> Option.defaultValue [||]
    member this.AsArrayInt32 ()          = this.AsArrayString() |> parseArray parseInt32 |> Option.defaultValue [||]
    member this.AsArrayInt               = this.AsArrayInt32
    member this.AsArrayInt64 ()          = this.AsArrayString() |> parseArray parseInt64 |> Option.defaultValue [||]
    member this.AsArrayBoolean ()        = this.AsArrayString() |> parseArray parseBoolean |> Option.defaultValue [||]
    member this.AsArrayFloat ()          = this.AsArrayString() |> parseArray parseFloat |> Option.defaultValue [||]
    member this.AsArrayDecimal ()        = this.AsArrayString() |> parseArray parseDecimal |> Option.defaultValue [||]
    member this.AsArrayDateTime ()       = this.AsArrayString() |> parseArray parseDateTime |> Option.defaultValue [||]
    member this.AsArrayDateTimeOffset () = this.AsArrayString() |> parseArray parseDateTimeOffset |> Option.defaultValue [||]
    member this.AsArrayGuid ()           = this.AsArrayString() |> parseArray parseGuid |> Option.defaultValue [||]
    member this.AsArrayTimeSpan ()       = this.AsArrayString() |> parseArray parseTimeSpan |> Option.defaultValue [||]

type HttpContext with  
    member this.GetFormAsync () = 
        task {
            return! this.Request.ReadFormAsync ()            
        }

    member this.GetFormReaderAsync () = 
        task {
            let! form = this.GetFormAsync ()
            return StringCollectionReader(form)
        }

    member this.GetFormReader () =
        this.GetFormReaderAsync() 
        |> Async.AwaitTask 
        |> Async.RunSynchronously

    member this.GetQueryReader () = 
        StringCollectionReader(this.Request.Query)

let tryBindForm 
    (bind : StringCollectionReader -> Result<'a, string> ) 
    (err : string -> HttpHandler) 
    (success : 'a -> HttpHandler) : HttpHandler =    
    fun (next : HttpFunc) (ctx : HttpContext) ->  
        task {
            let! form = ctx.GetFormReaderAsync ()            
            return! 
                (match form |> bind with
                | Ok m      -> success m
                | Error msg -> err msg) next ctx
        }

let tryBindQuery
    (bind : StringCollectionReader -> Result<'a, string> ) 
    (err : string -> HttpHandler) 
    (success : 'a -> HttpHandler) : HttpHandler =    
    fun (next : HttpFunc) (ctx : HttpContext) ->  
        (match ctx.GetQueryReader() |> bind with
        | Ok m      -> success m 
        | Error msg -> err msg) next ctx