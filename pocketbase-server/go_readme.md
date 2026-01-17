# PocketBase Custom Cron Jobs

## Overview

Extend PocketBase with custom cron jobs by embedding it in a Go application. These cron jobs execute in the background while PocketBase serves your web app.

## Quick Start - Simple example

```
scoop install go
go version

mkdir pocketbase-server
cd pocketbase-server

go mod init pocketbase-server

// main.go
package main

import (
    "log"
    "time"
    
    "github.com/pocketbase/pocketbase"
)

func main() {
    app := pocketbase.New()

    app.Cron().MustAdd("hello", "* * * * *", func() {
        log.Printf("CRON [hello]: Executed at %s", time.Now().Format("15:04:05"))
    })

    // Set arguments to simulate "serve" command
    app.RootCmd.SetArgs([]string{"serve", "--dev"})
    
    if err := app.Start(); err != nil {
        log.Fatal(err)
    }
}

go mod tidy
go run main.go

// Output example
// INFO GET /api/crons
// 2026/01/17 17:29:00 CRON [hello]: Executed at 17:29:00
// 2026/01/17 17:30:00 CRON [hello]: Executed at 17:30:00
// 2026/01/17 17:31:00 CRON [hello]: Executed at 17:31:00
// 2026/01/17 17:32:00 CRON [hello]: Executed at 17:32:00
// 2026/01/17 17:33:00 CRON [hello]: Executed at 17:33:00
// 2026/01/17 17:34:00 CRON [hello]: Executed at 17:34:00
// 2026/01/17 17:35:00 CRON [hello]: Executed at 17:35:00
// 2026/01/17 17:36:00 CRON [hello]: Executed at 17:36:00
```

> [!NOTE]  
> The extended cron feature is only available when serving the PocketBase web app through a **custom Go program**.  
> 
> Once triggered for the first time, the cron job will appear in the **Admin Web UI**, where it can also be executed manually.  
> 
> However, when running the same database via the standard **PocketBase executable**, the created cron job will **not** be visible or executed. This behavior is expected, since cron jobs are not stored in the database—they run **internally**, in **memory**, within the application.  
> 
> Under the hood, PocketBase relies on the [**robfig/cron**](https://github.com/robfig/cron) package for scheduling and managing these cron jobs.


## Advanced example
```
Coming soon...

curl -X POST http://127.0.0.1:8090/internal/cron \
  -H "Content-Type: application/json" \
  -d '{
    "id": "hello",
    "expression": "*/2 * * * *"
  }'
```
