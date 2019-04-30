﻿// Copyright 2019 Stellar Development Foundation and contributors. Licensed
// under the Apache License, Version 2.0. See the COPYING file at the root
// of this distribution or at http://www.apache.org/licenses/LICENSE-2.0

module PollRetry

open System.Threading

let DefaultRetry = 5

let rec WebExceptionRetry (n:int) (f:unit->'a) : 'a =
    try
        f()
    with
        | :? System.Net.WebException as w when n > 0 ->
            printfn "Web exception %s, retrying %d more times"
                (w.Status.ToString()) n
            Thread.Sleep(millisecondsTimeout = 1000)
            WebExceptionRetry (n-1) f


let rec RetryUntilTrue (f:unit->bool) (step:unit->unit) =
    if f()
    then ()
    else
        begin
            step()
            Thread.Sleep(millisecondsTimeout = 1000)
            RetryUntilTrue f step
        end