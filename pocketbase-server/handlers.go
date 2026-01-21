package crons

import (
    "encoding/json"
    "log"
)

type CronHandler func(payload map[string]any)

var Registry = map[string]CronHandler{}

func RegisterHandler(id string, handler CronHandler) {
    Registry[id] = handler
}

// Dispatch allows dynamic execution by cron ID
func Dispatch(id string, payloadJson string) {
    handler, ok := Registry[id]
    if !ok {
        log.Println("Unknown cron:", id)
        return
    }

    var payload map[string]any
    if err := json.Unmarshal([]byte(payloadJson), &payload); err != nil {
        log.Println("Failed to unmarshal payload:", err)
        return
    }

    hasndler(payload)
}
