package cron

import "sync"

type CronHandler func(payload map[string]any)

var (
    cronHandlers = map[string]CronHandler{}
    cronPayloads = map[string]map[string]any{}
    mu           sync.Mutex
)

func RegisterHandler(id string, handler CronHandler) {
    cronHandlers[id] = handler
}
