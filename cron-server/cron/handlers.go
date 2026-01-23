package cron

import (
    "log"
    "os"
    "os/exec"
    "time"
    "path/filepath"
)

func init() {
    RegisterHandler("hello", func(payload map[string]any) {
        ticker := time.NewTicker(10 * time.Second)
        defer ticker.Stop()

        name := "World!"
        count := 0

        if n, ok := payload["name"].(string); ok && n != "" {
            name = n
        }

        if c, ok := payload["count"].(float64); ok {
            count = int(c)
        }

        // Run for 1 minute (6 times at 10s intervals)
        for i := 0; i < 6; i++ {
            count += 10
            log.Printf("Hello, %s %d", name, count)

            <-ticker.C
        }
    })

    RegisterHandler("cleanup", func(payload map[string]any) {
        log.Println("cleanup cron fired")
    })

    RegisterHandler("db-dump", func(payload map[string]any) {
        outputDir := "./dump.sql"
        if dir, ok := payload["output_dir"].(string); ok && dir != "" {
            outputDir = dir
            log.Printf("Using custom output path: %s", outputDir)
        } else {
            log.Printf("Using default output path: %s", outputDir)
        }

        dir := filepath.Dir(outputDir)
        if dir != "." && dir != "" {
            if err := os.MkdirAll(dir, 0755); err != nil {
                log.Printf("Failed to create directory %s: %v", dir, err)
                return
            }
        }

        filterTables := []string{}
        if tables, ok := payload["tables"].([]any); ok {
            for _, t := range tables {
                if table, ok := t.(string); ok {
                    filterTables = append(filterTables, table)
                }
            }
        }

        cmd := exec.Command("sqlite3", "./pb_data/data.db", ".output "+outputDir, ".dump")
        if err := cmd.Run(); err != nil {
            log.Printf("Failed to dump database: %v", err)
        } else {
            log.Printf("Database dumped to %s", outputDir)
        }
    })
}

// # Basic cron registration with db-dump
// curl -X POST http://localhost:8090/internal/cron `
//   -H "Content-Type: application/json" `
//   -d '{"id":"db-dump","expression":"* * * * *","payload":{}}'

// # With custom output path
// curl -X POST http://localhost:8090/internal/cron `
//   -H "Content-Type: application/json" `
//   -d '{"id":"db-dump","expression":"* * * * *","payload":{"output_dir":"./backups/db.sql"}}'

// # Test hello handler (note: payload must be object, not string)
// curl -X POST http://localhost:8090/internal/cron `
//   -H "Content-Type: application/json" `
//   -d '{"id":"hello","expression":"* * * * *","payload":{"name":"Celso!","count":10}}'

// # Test cleanup handler
// curl -X POST http://localhost:8090/internal/cron `
//   -H "Content-Type: application/json" `
//   -d '{"id":"cleanup","expression":"* * * * *","payload":{}}'
