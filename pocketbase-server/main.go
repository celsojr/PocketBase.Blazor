package main

import (
    "net/http"

    "github.com/labstack/echo/v4"
    "github.com/pocketbase/pocketbase"
    "github.com/pocketbase/pocketbase/core"
)

func main() {
    app := pocketbase.New()

    // app.OnServe().BindFunc(func(se *core.ServeEvent) error {
    app.OnServe().Add(func(se *core.ServeEvent) error {
        router := se.Router

        router.POST("/internal/cron", func(c echo.Context) error {
            var req CronRequest
            if err := c.Bind(&req); err != nil {
                return c.JSON(http.StatusBadRequest, err.Error())
            }

            if err := registerCron(app, req); err != nil {
                return c.JSON(http.StatusBadRequest, err.Error())
            }

            return c.JSON(http.StatusOK, map[string]any{
                "status": "cron registered",
                "id":     req.ID,
            })
        })

        return se.Next()
    })

    if err := app.Start(); err != nil {
        panic(err)
    }
}

// Usage example
// curl -X POST http://127.0.0.1:8090/internal/cron \
//   -H "Content-Type: application/json" \
//   -d '{
//     "id": "hello",
//     "expression": "*/2 * * * *"
//   }'


// package main
// 
// import (
//     "log"
//     "net/http"
//     "os"
//     "os/signal"
//     "syscall"
// 
//     "github.com/labstack/echo/v5"
//     "github.com/pocketbase/pocketbase"
//     "github.com/pocketbase/pocketbase/apis"
//     "github.com/pocketbase/pocketbase/core"
// )
// 
// func main() {
//     app := pocketbase.New()
// 
//     // Register cron jobs
//     registerCrons(app)
// 
//     app.OnServe().BindFunc(func(se *core.ServeEvent) error {
//         router := se.Router
// 
//         // Static files (optional)
//         router.GET("/{path...}", apis.Static(os.DirFS("./pb_public"), false))
// 
//         // Private cron reload endpoint
//         router.POST("/internal/cron/reload", func(c echo.Context) error {
//             registerCrons(app)
//             return c.JSON(http.StatusOK, map[string]any{
//                 "status": "crons reloaded",
//             })
//         })
// 
//         return se.Next()
//     })
// 
//     // Graceful shutdown
//     go listenForShutdown(app)
// 
//     if err := app.Start(); err != nil {
//         log.Fatal(err)
//     }
// }

