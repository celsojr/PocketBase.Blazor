package main

import "log"

func init() {
	RegisterHandler("hello", func(payload map[string]any) {
		log.Println("hello cron fired")
		log.Printf("payload: %+v\n", payload)
	})

	RegisterHandler("cleanup", func(payload map[string]any) {
		log.Println("cleanup cron fired")
	})
}

