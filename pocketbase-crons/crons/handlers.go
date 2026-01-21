package crons

func init() {
    // Example from manifest (C# CronDefinition)
    RegisterHandler("hello", func(payload map[string]any) {
        // Advanced user can write any Go code here
        log.Println("Hello cron executed", payload)
    })

    RegisterHandler("send_email", func(payload map[string]any) {
        // User-defined email logic
        email := payload["email"]
        log.Println("Send email to:", email)
    })
}
