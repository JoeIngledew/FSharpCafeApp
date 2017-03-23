﻿module Items

open System.Collections.Generic
open Domain
open Queries

let private foods =
    let dict = new Dictionary<int, Food>()
    dict.Add(8, Food {
        MenuNumber = 8
        Price = 5m
        Name = "Salad"
    })
    dict.Add(9, Food {
        MenuNumber = 9
        Price = 10m
        Name = "Pizza"
    })
    dict

let private drinks =
    let dict = new Dictionary<int, Drink>()
    dict.Add(10, Drink{
        MenuNumber = 10
        Price = 2.5m
        Name = "Coke"
    })
    dict.Add(11, Drink{
        MenuNumber = 11
        Price = 1.5m
        Name = "Lemonade"
    })
    dict

let getItems<'a> (dict : Dictionary<int, 'a>) keys =
    let invalidKeys = keys |> Array.except dict.Keys
    if Array.isEmpty invalidKeys then
        keys
        |> Array.map (fun n -> dict.[n])
        |> Array.toList
        |> Choice1Of2
    else
        invalidKeys |> Choice2Of2

let getItem<'a> (dict : Dictionary<int,'a>) key =
    if dict.ContainsKey key then
        dict.[key] |> Some
    else 
        None

let getFoodsByMenuNumbers keys =
    getItems foods keys |> async.Return

let getFoodByMenuNumber key =
    getItem foods key |> async.Return

let getDrinksByMenuNumbers keys =
    getItems drinks keys |> async.Return

let getDrinkByMenuNumber key =
    getItem drinks key |> async.Return

let foodQueries = {
    GetFoodsByMenuNumbers = getFoodsByMenuNumbers
    GetFoodByMenuNumber = getFoodByMenuNumber
}

let drinkQueries = {
    GetDrinksByMenuNumbers = getDrinksByMenuNumbers
    GetDrinkByMenuNumber = getDrinkByMenuNumber
}
        