// generated_crons.go (AUTO-GENERATED. DO NOT EDIT.)
package main

import (
    "log"
)

type CronHandler func(payload map[string]any)

//go:generate comptime -template cron.tmpl -data crons.json -out generated_crons.go
var cronHandlers = map[string]CronHandler{
    "hello": func(payload map[string]any) {
        log.Println("Hello!")
    },
    "reindex": func(payload map[string]any) {
        log.Println("Reindexing collection:", payload["collection"])
    },
}
