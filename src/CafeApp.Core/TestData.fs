module TestData

open Domain
open System

let tab = {Id = Guid.NewGuid();TableNumber = 1}
let drink1 = Drink {
                MenuNumber = 1
                Name = "Drink1"
                Price = 1.5m }
let drink2 = Drink {
                MenuNumber = 3
                Name = "Drink2"
                Price = 1.0m }
let drink3 = Drink {
                MenuNumber = 5
                Name = "Drink3"
                Price = 0.5m }

let order = {Tab = tab; Foods = []; Drinks = []}
let food1 = Food {
                MenuNumber = 2
                Name = "Food1"
                Price = 2.5m }
let food2 = Food {
                MenuNumber = 4
                Name = "Food2"
                Price = 6.5m }

let foodPrice (Food food) = food.Price
let drinkPrice (Drink drink) = drink.Price