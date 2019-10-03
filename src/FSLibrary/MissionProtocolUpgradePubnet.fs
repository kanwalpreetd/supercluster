// Copyright 2019 Stellar Development Foundation and contributors. Licensed
// under the Apache License, Version 2.0. See the COPYING file at the root
// of this distribution or at http://www.apache.org/licenses/LICENSE-2.0

module MissionProtocolUpgradePubnet

open StellarCoreHTTP
open StellarCorePeer
open StellarCoreSet
open StellarMissionContext
open StellarNetworkCfg
open StellarNetworkData
open StellarSupercluster

let protocolUpgradePubnet (context : MissionContext) =
    let set = { CoreSetOptions.Default with
                  nodeCount = 1
                  quorumSet = Some(["core"])
                  historyNodes = Some([])
                  historyGetCommands = PubnetGetCommands
                  catchupMode = CatchupRecent(0)
                  initialization = { CoreSetInitialization.Default with initialCatchup = true } }

    let coreSet = MakeLiveCoreSet "core" set
    context.Execute [coreSet] (Some(SDFMainNet)) (fun (formation: ClusterFormation) ->
        formation.WaitUntilSynced [coreSet]

        let peer = formation.NetworkCfg.GetPeer coreSet 0
        peer.WaitForFewLedgers(3)
        peer.UpgradeProtocolToLatest()
        peer.WaitForFewLedgers(3)

        formation.CheckUsesLatestProtocolVersion()
    )