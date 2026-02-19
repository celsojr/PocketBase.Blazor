// pb_migrations/1687801090_initial_settings.js

migrate((app) => {
    let settings = app.settings()

    settings.meta.appName = "Blazor Blog"
    settings.meta.appURL = "http://localhost:8092"
    settings.logs.maxDays = 2
    settings.logs.logAuthId = true
    settings.logs.logIP = false

    app.save(settings)
})

