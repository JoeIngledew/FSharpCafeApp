module ServeFoodTests

open Domain
open States
open Commands
open Events
open CafeAppTestsDSL
open NUnit.Framework
open TestData
open Errors

[<Test>]
let ``Can maintain the order in progress state by serving food`` () =
    let order = {order with Foods = [food1;food2]}
    let orderInProgress = {
        PlacedOrder = order
        ServedFoods = []
        ServedDrinks = []
        PreparedFoods = [food1;food2] }
    let expected = {orderInProgress with ServedFoods = [food1]}

    Given (OrderInProgress orderInProgress)
    |> When (ServeFood (food1, order.Tab.Id))
    |> ThenStateShouldBe (OrderInProgress expected)
    |> WithEvents [FoodServed (food1, order.Tab.Id)]

[<Test>]
let ``Can only serve prepared food`` () =
    let order = {order with Foods = [food1;food2]}
    let orderInProgress = {
        PlacedOrder = order
        ServedFoods = []
        ServedDrinks = []
        PreparedFoods = [food1] }

    Given (OrderInProgress orderInProgress)
    |> When (ServeFood (food2, order.Tab.Id))
    |> ShouldFailWith (CanNotServeNonPreparedFood food2)

[<Test>]
let``Can not serve non-ordered food`` () =
    let order = {order with Foods = [food1]}
    let orderInProgress = {
        PlacedOrder = order
        ServedFoods = []
        ServedDrinks = []
        PreparedFoods = [food1] }

    Given (OrderInProgress orderInProgress)
    |> When (ServeFood (food2, order.Tab.Id))
    |> ShouldFailWith (CanNotServeNonOrderedFood food2)

[<Test>]
let ``Cannot serve already served food`` () =
    let order = {order with Foods = [food1;food2]}
    let orderInProgress = {
        PlacedOrder = order
        ServedFoods = [food1]
        ServedDrinks = []
        PreparedFoods = [food2] }

    Given (OrderInProgress orderInProgress)
    |> When (ServeFood (food1, order.Tab.Id))
    |> ShouldFailWith (CanNotServeAlreadyServedFood food1)

[<Test>]
let ``Cannot serve for placed order`` () =
    Given (PlacedOrder order)
    |> When (ServeFood (food1, order.Tab.Id))
    |> ShouldFailWith (CanNotServeNonPreparedFood food1)

[<Test>]
let `` Cannot serve for non placed order`` () =
    Given (OpenedTab tab)
    |> When (ServeFood (food1, tab.Id))
    |> ShouldFailWith CanNotServeForNonPlacedOrder

[<Test>]
let ``Cannot serve fir already served order`` () =
    Given (ServedOrder order)
    |> When (ServeFood (food1, order.Tab.Id))
    |> ShouldFailWith OrderAlreadyServed

[<Test>]
let ``Cannot serve with closed tab`` () =
    Given (ClosedTab None)
    |> When (ServeFood (food1, order.Tab.Id))
    |> ShouldFailWith CanNotServeWithClosedTab