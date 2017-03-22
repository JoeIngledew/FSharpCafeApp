module PlaceOrderTests

open NUnit.Framework
open CafeAppTestsDSL
open Domain
open System
open States
open Commands
open Events

let tab = {Id = Guid.NewGuid(); TableNumber = 1}
let drink1 = Drink {
                MenuNumber = 1
                Name = "Drink1"
                Price = 1.5m }
let order = {Tab = tab; Foods = []; Drinks = []}

[<Test>]
let ``Can place only drinks order`` () =
    let order = { order with Drinks = [drink1]}
    Given (OpenedTab tab)
    |> When (PlaceOrder order)
    |> ThenStateShouldBe (PlacedOrder order)
    |> WithEvents [OrderPlaced order]