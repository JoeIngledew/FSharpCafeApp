module OpenTabTests

open NUnit.Framework
open CafeAppTestsDSL
open States
open Commands
open Events
open Domain
open System
open Errors

[<Test>]
let ``Can Open a new Tab``() =
    let tab = {Id = Guid.NewGuid(); TableNumber = 1}

    Given (ClosedTab None)
    |> When (OpenTab tab)
    |> ThenStateShouldBe (OpenedTab tab)
    |> WithEvents [TabOpened tab]

[<Test>]
let ``Cannot open an already Opened tab`` () =
    let tab = {Id = Guid.NewGuid(); TableNumber = 1}
    Given (OpenedTab tab)
    |> When (OpenTab tab) 
    |> ShouldFailWith TabAlreadyOpened

