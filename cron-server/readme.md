# PocketBase Custom Cron Jobs

## Overview

This project extends **PocketBase** with custom cron jobs by embedding it in a **custom Go application**. Cron jobs execute **in-process**, in memory, alongside the PocketBase server, allowing advanced scheduling and custom logic.


1. **In-memory execution**:  
   Custom cron jobs and their payloads are stored **in memory**. This matches how PocketBase manages internal cron jobs.  
   - Advantages: fast, no DB migration required, simple lifecycle management.  
   - Limitations: payloads are **volatile** and are lost if the Go process stops.  
   - Persistence: users can optionally persist cron definitions and payloads in their own storage (file, DB, etc.) if desired.

2. **No PocketBase DB changes**:  
   - Custom cron jobs are **not stored in PocketBase collections**.  
   - They are fully managed by the Go application and can be triggered via internal HTTP endpoints.

3. **Advanced user support**:  
   - Users can provide **custom Go handler logic** via a `.tmpl` file or a CronManifest in the C# SDK.  
   - Handlers are responsible for their own imports and execution logic.

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
> Once registered, the cron job will appear in the **Admin Web UI**, where it can also be executed manually.  
> 
> However, when running the same database via the standard **PocketBase executable**, the created cron job will **not** be visible or executed. This behavior is expected, since cron jobs are not stored in the database—they run **internally**, in **memory**, within the current application process that is tailor made for this purpose.  
> 
> Under the hood, PocketBase relies on the [**robfig/cron**](https://github.com/robfig/cron) package for scheduling and managing these cron jobs.

## Advanced example
- **Dynamic payload support**: payloads are kept in memory and passed to the handler function. Users can define any structure.
- **Custom Go logic**: advanced users can define their own logic Go handler body logic in a CronManifest via the C# SDK.
- **Internal endpoint for registration**:
```
curl -X POST http://127.0.0.1:8090/internal/cron \
  -H "Content-Type: application/json" \
  -d '{
    "id": "hello",
    "expression": "* * * * *",
    "payload": {"name":"World!","count":10}
  }'
```

## Summary

- Custom cron jobs run **in-memory** in the Go process alongside PocketBase.
- Payloads are **kept in memory**, and persistence is left to the user if needed.
- Handlers are fully **customizable** and can use any Go code.
- Cron jobs are **not stored in PocketBase DB**—this keeps the core DB untouched.
- Registration and execution are handled via **C# SDK or internal HTTP endpoints**.

This design allows **flexible, safe, and testable cron logic** while keeping **PocketBase** isolated from custom runtime code.