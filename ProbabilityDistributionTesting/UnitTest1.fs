module ProbabilityDistributionTesting

open NUnit.Framework
open BestDiceRollerBot
open ParallelisationLibrary.Extensions
open NUnit.Framework
open XPlot
open XPlot.Plotly
open XPlot.Plotly.Graph

let roller = RandomProducer()

[<Literal>]
let rollNum = 1_000_000

let ranges = [ 1 .. 100 ]

let rolls range =
    List.map (fun _ -> roller.Get(1, range + 1)) [ 1 .. rollNum ]

[<SetUp>]
let Setup () = ()

[<Test>]
let ``Test that less 0.5% difference between most frequent and least frequent rolled result`` () =
    let allRolls = List.map (fun r -> (r, rolls r)) ranges

    let lowSpread =
        allRolls
        |> List.pMap
            (snd
             >> (fun l -> List.countBy id l)
             >> (fun l -> List.map snd l)
             >> (fun l -> (List.max l) - (List.min l))
             >> (fun i -> ((i |> float) / (rollNum |> float)) * 100.)
             >> (fun f -> f < 0.5))
        |> List.forall id

    Assert.True(lowSpread)

[<Test>]
let ``Test that rolls are restricted to die range`` () =
    let allRolls = List.pMap (fun r -> (r, rolls r)) ranges

    let checks =
        List.pMap (fun (r, l) ->
            Assert.AreEqual(1, List.min l)
            Assert.AreEqual(r, List.max l)
            true) allRolls

    Assert.True(List.forall id checks)


[<Test>]
let ``Generate a Rolling Average Graph of die rolls`` () =
    let allRolls =
        List.pMap (fun r -> (r, rolls r)) [ 4; 6; 8; 10; 12 ]

    let rollingAverage windowSize r xs =

        xs
        |> List.map float
        |> List.windowed windowSize
        |> List.pMap List.average

    let scatterAverages r xs =
        let traceName = "Trace range: " + (string r)
        xs
        |> fun values -> Histogram(x = values, name = traceName)

    let rollings = [ 100 ]

    let rollScatter =
        rollings
        |> List.pMap (fun rolling ->
            List.pMap (fun (r, l) -> (rollingAverage rolling r l) |> scatterAverages r) allRolls)

    rollScatter
    |> List.map Chart.Plot
    |> Chart.ShowAll
    Assert.Pass()


[<Test>]
let ``Make a graph to show consecutive dice rolls`` () =
    let allRolls =
        List.pMap (fun r -> (r, rolls r)) [ 4; 6; 8; 10; 12 ]

    let groupRuns rollChain =
        let folder x acc =
            match acc with
            | (a :: cc) when List.contains x a -> ((x :: a) :: cc)
            | _ -> ([ x ] :: acc)

        List.foldBack folder rollChain []
        |> List.groupBy (fun xs -> List.head xs)
        |> List.sortBy (fun (key, _) -> key)
        |> List.pMap (fun (key, lists) ->
            (key,
             lists
             |> List.map (fun l -> (List.length l |> float))))

    let groupedUp =
        List.pMap (fun (r, ro) -> (r, groupRuns ro)) allRolls

    let makeHistogram (value, runs) =
        Histogram(x = runs, histnorm = "probability density", name = (string value), opacity = 0.75)

    groupedUp
    |> List.map (fun (r, results) ->
        List.map makeHistogram results
        |> Chart.Plot
        |> Chart.WithTitle(string r)
        |> Chart.WithLegend true)
    |> Chart.ShowAll
