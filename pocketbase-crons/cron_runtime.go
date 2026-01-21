package main

import (
    "errors"

    "github.com/pocketbase/pocketbase"
)

func registerCron(app *pocketbase.PocketBase, req CronRequest) error {
    handler, ok := cronHandlers[req.ID]
    if !ok {
        return errors.New("unknown cron id: " + req.ID)
    }

    app.Cron().Remove(req.ID)

    app.Cron().Add(req.ID, req.Expression, func() {
        mu.Lock()
        payload := cronPayloads[req.ID]
        mu.Unlock()

        handler(payload)
    })

    return nil
}
