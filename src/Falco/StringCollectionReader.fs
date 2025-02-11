﻿[<AutoOpen>]
module Falco.StringCollectionReader

open System
open System.Collections.Generic
open Microsoft.Extensions.Primitives
open Falco.StringParser
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing

/// A safe string collection reader, with type utilities
[<AbstractClass>]
type StringCollectionReader internal (values : Map<string, string[]>) =

    new (kvpValues : KeyValuePair<string, StringValues> seq) =
        let map =
            kvpValues
            |> Seq.map (fun kvp -> kvp.Key, kvp.Value.ToArray())
            |> Map.ofSeq

        StringCollectionReader(map)

    new (routeValues : RouteValueDictionary) =
        let map =
            routeValues
            |> Seq.map (fun kvp -> kvp.Key, [|Convert.ToString(kvp.Value, Globalization.CultureInfo.InvariantCulture)|])
            |> Map.ofSeq

        StringCollectionReader(map)

    new (cookies : IRequestCookieCollection) =
        let map =
            cookies
            |> Seq.map (fun kvp -> kvp.Key, [|kvp.Value|])
            |> Map.ofSeq

        StringCollectionReader(map)

    /// Safely retrieve string array value from collection
    member _.TryGetValue (name : string) =
        let found = 
            values 
            |> Map.tryPick (fun key value -> 
                if StringUtils.strEquals key name then Some value else None) 

        match found with
        | Some v when v.Length > 0 -> Some v
        | _                        -> None
    
    /// Retrieve value from StringCollectionReader
    member x.GetValue (name : string) =        
        match x.TryGetValue name with
        | Some v -> v
        | None -> failwith (sprintf "Could not find %s" name)

    /// Safely retrieve string array value from collection and apply binder
    member x.TryGetBind (binder : string array -> 'a option) (name : string) =
        x.TryGetValue name |> Option.bind binder

    /// Safely retrieve string array value from collection and apply binder
    member x.TryGetBindArray (binder : string -> 'a option) (name : string) =
        x.TryGetBind (tryParseArray binder) name |> Option.defaultValue [||]
        
    // ------------
    // Primitives
    // ------------

    /// Safely retrieve String option
    member x.TryGetString (name : string) = x.TryGetBind (fun v -> Some v.[0]) name

    /// Safely retrieve String option (alias for StringCollectionReader.TryGetString)
    member x.TryGet (name : string) = x.TryGetString name

    /// Safely retrieve non-empty String option
    member x.TryGetStringNonEmpty (name : string) =        
        let strOption = x.TryGetString name 
        match strOption with
        | Some x when StringUtils.strNotEmpty x -> Some x
        | _  -> None

    /// Safely retrieve Int16 option
    member x.TryGetInt16 (name : string) = x.TryGetBind (fun v -> parseInt16 v.[0]) name

    /// Safely retrieve Int32 option
    member x.TryGetInt32 (name : string) = x.TryGetBind (fun v -> parseInt32 v.[0]) name

    /// Safely retrieve Int option
    member x.TryGetInt (name : string) = x.TryGetInt32 name

    /// Safely retrieve Int64 option
    member x.TryGetInt64 (name : string) = x.TryGetBind (fun v -> parseInt64 v.[0]) name

    /// Safely retrieve Boolean option
    member x.TryGetBoolean (name : string) = x.TryGetBind (fun v -> parseBoolean v.[0]) name

    /// Safely retrieve Float option
    member x.TryGetFloat (name : string) = x.TryGetBind (fun v -> parseFloat v.[0]) name

    /// Safely retrieve Decimal option
    member x.TryGetDecimal (name : string) = x.TryGetBind (fun v -> parseDecimal v.[0]) name

    /// Safely retrieve DateTime option
    member x.TryGetDateTime (name : string) = x.TryGetBind (fun v -> parseDateTime v.[0]) name

    /// Safely retrieve DateTimeOffset option
    member x.TryGetDateTimeOffset (name : string) = x.TryGetBind (fun v -> parseDateTimeOffset v.[0]) name

    /// Safely retrieve Guid option
    member x.TryGetGuid (name : string) = x.TryGetBind (fun v -> parseGuid v.[0]) name

    /// Safely retrieve TimeSpan option
    member x.TryGetTimeSpan (name : string) = x.TryGetBind (fun v -> parseTimeSpan v.[0]) name

    // ------------
    // Primitives - Get or Default
    // ------------

    /// Safely retrieve named String or defaultValue
    member x.GetString (name : string) defaultValue = x.TryGetString name |> Option.defaultValue defaultValue

    /// Safely retrieve named string or defaultValue
    member x.Get (name : string) defaultValue = x.TryGet name |> Option.defaultValue defaultValue

    /// Safely retrieve named non-empty String or defaultValue
    member x.GetStringNonEmpty (name : string) defaultValue = x.TryGetStringNonEmpty name |> Option.defaultValue defaultValue

    /// Safely retrieve named Int16 or defaultValue
    member x.GetInt16 (name : string) defaultValue = x.TryGetInt16 name |> Option.defaultValue defaultValue

    /// Safely retrieve named Int32 or defaultValue
    member x.GetInt32 (name : string) defaultValue = x.TryGetInt32 name |> Option.defaultValue defaultValue

    /// Safely retrieve named Int or defaultValue
    member x.GetInt (name : string) defaultValue = x.TryGetInt name |> Option.defaultValue defaultValue

    /// Safely retrieve named Int64 or defaultValue
    member x.GetInt64 (name : string) defaultValue = x.TryGetInt64 name |> Option.defaultValue defaultValue

    /// Safely retrieve named Boolean or defaultValue
    member x.GetBoolean (name : string) defaultValue = x.TryGetBoolean name |> Option.defaultValue defaultValue

    /// Safely retrieve named Float or defaultValue
    member x.GetFloat (name : string) defaultValue = x.TryGetFloat name |> Option.defaultValue defaultValue

    /// Safely retrieve named Decimal or defaultValue
    member x.GetDecimal (name : string) defaultValue = x.TryGetDecimal name |> Option.defaultValue defaultValue

    /// Safely retrieve named DateTime or defaultValue
    member x.GetDateTime (name : string) defaultValue = x.TryGetDateTime name |> Option.defaultValue defaultValue

    /// Safely retrieve named DateTimeOffset or defaultValue
    member x.GetDateTimeOffset (name : string) defaultValue = x.TryGetDateTimeOffset name |> Option.defaultValue defaultValue

    /// Safely retrieve named Guid or defaultValue
    member x.GetGuid (name : string) defaultValue = x.TryGetGuid name |> Option.defaultValue defaultValue

    /// Safely retrieve named TimeSpan or defaultValue
    member x.GetTimeSpan (name : string) defaultValue = x.TryGetTimeSpan name |> Option.defaultValue defaultValue

    // ------------
    // Array Primitives
    // ------------

    /// Safely retrieve the named String[]
    member x.TryArrayString (name : string) = x.TryGetBindArray (fun v -> Some v) name

    /// Safely retrieve the named Int16[]
    member x.TryArrayInt16 (name : string) = x.TryGetBindArray parseInt16 name

    /// Safely retrieve the named Int32[]
    member x.TryArrayInt32 (name : string) = x.TryGetBindArray parseInt32 name

    /// Safely retrieve the named Int[] (alias for StringCollectionReader.TryArrayInt32)
    member x.TryArrayInt (name : string) = x.TryArrayInt32 name

    /// Safely retrieve the named Int64[]
    member x.TryArrayInt64 (name : string) = x.TryGetBindArray parseInt64 name

    /// Safely retrieve the named Boolean[]
    member x.TryArrayBoolean (name : string) = x.TryGetBindArray parseBoolean name

    /// Safely retrieve the named Float[]
    member x.TryArrayFloat (name : string) = x.TryGetBindArray parseFloat name

    /// Safely retrieve the named Decimal[]
    member x.TryArrayDecimal (name : string) = x.TryGetBindArray parseDecimal name

    /// Safely retrieve the named DateTime[]
    member x.TryArrayDateTime (name : string) = x.TryGetBindArray parseDateTime name

    /// Safely retrieve the named DateTimeOffset[]
    member x.TryArrayDateTimeOffset (name : string) = x.TryGetBindArray parseDateTimeOffset name

    /// Safely retrieve the named Guid[]
    member x.TryArrayGuid (name : string) = x.TryGetBindArray parseGuid name

    /// Safely retrieve the named TimeSpan[]
    member x.TryArrayTimeSpan (name : string) = x.TryGetBindArray parseTimeSpan name


/// Represents a readable collection of parsed form value
type FormCollectionReader (form : IFormCollection, files : IFormFileCollection option) =
    inherit StringCollectionReader (form)

    /// The IFormFileCollection submitted in the request.
    ///
    /// Note: Only present if form enctype="multipart/form-data".
    member _.Files = files

    /// Safely retrieve the named IFormFile option from the IFormFileCollection
    member x.TryGetFormFile (name : string) =
        if StringUtils.strEmpty name then None
        else
            match x.Files with
            | None       -> None
            | Some files ->
                let file = files.GetFile name

                if isNull(file) then None else Some file

/// Represents a readable collection of parsed HTTP header values
type HeaderCollectionReader (headers : IHeaderDictionary) =
    inherit StringCollectionReader (headers)

/// Represents a readble collection of query string values
type QueryCollectionReader (query : IQueryCollection) =
    inherit StringCollectionReader (query)

/// Represents a readble collection of route values
type RouteCollectionReader (route : RouteValueDictionary, query : IQueryCollection) =
    inherit StringCollectionReader (route)
    member _.Query = QueryCollectionReader(query)

/// Represents a readble collection of cookie values
type CookieCollectionReader (cookies: IRequestCookieCollection) =
    inherit StringCollectionReader(cookies)