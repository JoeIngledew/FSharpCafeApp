module Domain

open System

type Tab = {
  Id : Guid
  TableNumber : int
} with override this.ToString () = sprintf "(%s) // (%d)" (this.Id.ToString("M")) this.TableNumber

type Item = {
  MenuNumber : int
  Price : decimal
  Name : string
} with override this.ToString () = sprintf "(%d) // (%f) // (%s)" this.MenuNumber this.Price this.Name

type Food = Food of Item 
type Drink = Drink of Item

type Payment = {
  Tab : Tab
  Amount : decimal
} with override this.ToString () = sprintf "(%A) // (%f)" this.Tab this.Amount

type Order = {
  Foods : Food list
  Drinks : Drink list
  Tab : Tab
}  with override this.ToString () = sprintf "(%A) // (%A) // (%A)" this.Foods this.Drinks this.Tab

type InProgressOrder = {
  PlacedOrder : Order
  ServedDrinks : Drink list
  ServedFoods : Food list
  PreparedFoods : Food list
} with override this.ToString () = sprintf "(%A) // (%A) // (%A) // (%A)" this.PlacedOrder this.ServedDrinks this.ServedFoods this.PreparedFoods

let isServingDrinkCompletesOrder order drink =
    List.isEmpty order.Foods && order.Drinks = [drink]

let orderAmount order =
    let foodAmount =
        order.Foods
        |> List.map (fun (Food f) -> f.Price) |> List.sum
    let drinksAmount =
        order.Drinks
        |> List.map (fun (Drink d) -> d.Price) |> List.sum
    foodAmount + drinksAmount

let nonServedFoods ipo =
    List.except ipo.ServedFoods ipo.PlacedOrder.Foods

let nonServedDrinks ipo =
    List.except ipo.ServedDrinks ipo.PlacedOrder.Drinks

let isServingDrinkCompletesIPOrder ipo drink =
    List.isEmpty (nonServedFoods ipo)
    && (nonServedDrinks ipo) = [drink]

let isServindFoodCompletesIPOrder ipo food =
    List.isEmpty (nonServedDrinks ipo)
    && (nonServedFoods ipo) = [food]