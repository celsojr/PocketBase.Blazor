package main

import (
    "log"
    "net/http"

    "github.com/labstack/echo/v4"
    "github.com/pocketbase/pocketbase"
    "github.com/pocketbase/pocketbase/core"
)

func main() {
    app := pocketbase.New()

    app.OnServe().Add(func(e *core.ServeEvent) error {
        router := e.Router

        // POST /internal/cron
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

        return e.Next()
    })

    if err := app.Start(); err != nil {
        log.Fatal(err)
    }
}
