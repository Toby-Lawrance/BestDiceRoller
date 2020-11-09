namespace BestDiceRoller

type DiceRoller() = 
    member this.fairAndBalancedRoll() = 4
    member this.RollDice(input) = string (this.fairAndBalancedRoll())