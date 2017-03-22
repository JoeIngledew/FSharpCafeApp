module ServeDrinkTests

open Domain
open States
open Commands
open Events
open CafeAppTestsDSL
open NUnit.Framework
open TestData
open Errors

[<Test>]
let ``Can serve drink`` () =
    let order = {order with Drinks = [drink1;drink2]}
    let expected = {
        PlacedOrder = order
        ServedDrinks = [drink1]
        PreparedFoods = []
        ServedFoods = []}
    Given (PlacedOrder order)
    |> When (ServeDrink (drink1, order.Tab.Id))
    |> ThenStateShouldBe (OrderInProgress expected)
    |> WithEvents [DrinkServed (drink1, order.Tab.Id)]

[<Test>]
let ``Cannot serve non-ordered drink`` () =
    let order = {order with Drinks = [drink1]}
    Given (PlacedOrder order)
    |> When (ServeDrink (drink2, order.Tab.Id))
    |> ShouldFailWith (CanNotServeNonOrderedDrink drink2)

[<Test>]
let ``Cannot serve drink for already served order`` () =
    Given (ServedOrder order)
    |> When (ServeDrink (drink1, order.Tab.Id))
    |> ShouldFailWith OrderAlreadyServed

[<Test>]
let ``Cannot serve drinks for non-placed order`` () =
    Given (OpenedTab tab)
    |> When (ServeDrink (drink1, order.Tab.Id))
    |> ShouldFailWith CanNotServeForNonPlacedOrder

[<Test>]
let ``Cannot serve with closed tab`` () =
    Given (ClosedTab None)
    |> When (ServeDrink (drink1, tab.Id))
    |> ShouldFailWith CanNotServeWithClosedTab

[<Test>]
let ``Can Serve drink for order containing only one drink`` () =
    let order = {order with Drinks = [drink1]}
    let payment = {Tab = order.Tab; Amount = drinkPrice drink1}

    Given (PlacedOrder order)
    |> When (ServeDrink (drink1, order.Tab.Id))
    |> ThenStateShouldBe (ServedOrder order)
    |> WithEvents [
        DrinkServed (drink1, order.Tab.Id)
        OrderServed (order, payment) ]