package main

import (
	"log"
	"net/http"

	"github.com/pocketbase/pocketbase"
	"github.com/pocketbase/pocketbase/core"
)

func main() {
	app := pocketbase.New()

	app.OnServe().BindFunc(func(e *core.ServeEvent) error {
		e.Router.POST("/internal/cron", func(re *core.RequestEvent) error {
			var req CronRequest

			if err := re.BindBody(&req); err != nil {
				return re.JSON(http.StatusBadRequest, err.Error())
			}

			if err := registerCron(app, req); err != nil {
				return re.JSON(http.StatusBadRequest, err.Error())
			}

			return re.JSON(http.StatusOK, map[string]any{
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

