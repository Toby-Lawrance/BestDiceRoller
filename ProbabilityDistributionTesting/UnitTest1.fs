module ProbabilityDistributionTesting

open NUnit.Framework
open BestDiceRollerBot
open NUnit.Framework
open XPlot
open XPlot.Plotly
open XPlot.Plotly.Graph

let roller = RandomProducer()

[<Literal>]
let rollNum = 1000000

let ranges = [ 1 .. 100 ]

let rolls range =
    List.map (fun _ -> roller.Get(1, range + 1)) [ 1 .. rollNum ]

[<SetUp>]
let Setup () = ()

[<Test>]
let LowSpreadForUniformDistribution () =
    let allRolls = List.map (fun r -> (r, rolls r)) ranges

    let lowSpread =
        allRolls
        |> List.map snd
        |> List.map (fun l -> List.countBy id l)
        |> List.map (fun l -> List.map snd l)
        |> List.map (fun l -> (List.max l) - (List.min l))
        |> List.map (fun i -> ((i |> float) / (rollNum |> float)) * 100.)
        |> List.map (fun f -> f < 0.5)
        |> List.forall id

    Assert.True(lowSpread)

[<Test>]
let RollsRestrictedToRange () =
    let allRolls = List.map (fun r -> (r, rolls r)) ranges

    let checks =
        List.map (fun (r, l) ->
            Assert.AreEqual(1, List.min l)
            Assert.AreEqual(r, List.max l)
            true) allRolls

    Assert.True(List.forall id checks)


[<Test>]
let GenRollingAverageGraph () =
    let allRolls =
        List.map (fun r -> (r, rolls r)) [ 4; 6; 8; 10; 12 ]

    let rollingAverage windowSize r xs =

        xs
        |> List.map float
        |> List.windowed windowSize
        |> List.map List.average

    let scatterAverages r xs =
        let traceName = "Trace range: " + (string r)
        xs
        |> fun values -> Histogram(x = values, name = traceName)

    let rollings = [100]

    let rollScatter =
        rollings
        |> List.map (fun rolling -> List.map (fun (r, l) -> (rollingAverage rolling r l) |> scatterAverages r) allRolls)
    
    rollScatter
    |> List.map Chart.Plot
    |> Chart.ShowAll
    Assert.Pass()
