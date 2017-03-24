module PrepareFoodTests

open Domain
open States
open Commands
open Events
open CafeAppTestsDSL
open NUnit.Framework
open TestData
open Errors

[<Test>]
let ``Can prepare food`` () =
    let order = {order with Foods = [food1]}
    let expected = {
        PlacedOrder = order
        ServedDrinks = []
        PreparedFoods = [food1]
        ServedFoods = [] }

    Given (PlacedOrder order)
    |> When (PrepareFood (food1, order.Tab.Id))
    |> ThenStateShouldBe (OrderInProgress expected)
    |> WithEvents [FoodPrepared (food1,order.Tab.Id)]

[<Test>]
let ``Cannot prepare a non-ordered food`` () =
    let order = {order with Foods = [food2]}
    Given (PlacedOrder order)
    |> When (PrepareFood (food1, order.Tab.Id))
    |> ShouldFailWith (CanNotPrepareNonOrderedFood food1)

[<Test>]
let ``Cannot prepare food for served order`` () =
    Given (ServedOrder order)
    |> When (PrepareFood (food2, order.Tab.Id))
    |> ShouldFailWith OrderAlreadyServed

[<Test>]
let ``Cannot prepare with a closed tab`` () =
    Given (ClosedTab None)
    |> When (PrepareFood (food1, order.Tab.Id))
    |> ShouldFailWith CanNotPrepareWithClosedTab

[<Test>]
let `` Can prepare food during order in progress`` () =
    let order = {order with Foods = [food1;food2]}
    let orderInProgress = {
        PlacedOrder = order
        ServedFoods = []
        ServedDrinks = []
        PreparedFoods = [food2] }
    let expected = {orderInProgress with PreparedFoods = [food1;food2]}

    Given (OrderInProgress orderInProgress)
    |> When (PrepareFood (food1, order.Tab.Id))
    |> ThenStateShouldBe (OrderInProgress expected)
    |> WithEvents [FoodPrepared (food1, order.Tab.Id)]

[<Test>]
let ``Cannot prepare non-ordered foor during order in progress`` () =
    let order = { order with Foods = [food1]}
    let orderInProgress = {
        PlacedOrder = order
        ServedFoods = []
        ServedDrinks = []
        PreparedFoods = [] }

    Given (OrderInProgress orderInProgress)
    |> When (PrepareFood (food2, order.Tab.Id))
    |> ShouldFailWith (CanNotPrepareNonOrderedFood food2)

[<Test>]
let ``Can not prepare already prepared food during order in progress`` () =
    let order = { order with Foods = [food1] }
    let orderInProgress = {
        PlacedOrder = order
        ServedFoods = []
        ServedDrinks = []
        PreparedFoods = [food1] }

    Given (OrderInProgress orderInProgress)
    |> When (PrepareFood (food1, order.Tab.Id))
    |> ShouldFailWith (CanNotPrepareAlreadyPreparedFood food1)