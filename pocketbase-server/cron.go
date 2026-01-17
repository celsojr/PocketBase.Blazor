package main

import (
    "errors"

    "github.com/pocketbase/pocketbase"
)

type CronRequest struct {
    ID         string `json:"id"`
    Expression string `json:"expression"`
}

func registerCron(app *pocketbase.PocketBase, req CronRequest) error {
    handler, ok := cronHandlers[req.ID]
    if !ok {
        return errors.New("unknown cron id: " + req.ID)
    }

    app.Cron().Remove(req.ID)

    app.Cron().Add(req.ID, req.Expression, handler)
    return nil
}

// package main
// 
// import (
//     "log"
// 
//     "github.com/pocketbase/pocketbase"
// )
// 
// func registerCrons(app *pocketbase.PocketBase) {
//     app.Cron().Remove("example-job")
// 
//     app.Cron().Add("example-job", "*/1 * * * *", func() {
//         log.Println("Cron executed")
//     })
// }
