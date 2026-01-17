package main

import (
    "log"
)

type CronHandler func()

var cronHandlers = map[string]CronHandler{
    "hello": func() {
        log.Println("Hello!")
    },
}
