// pb_migrations/1687801090_initial_regular_user.js

migrate((app) => {
    let users = app.findCollectionByNameOrId("users")

    let record = new Record(users)
    record.set("email", "user_tester@email.com")
    record.set("password", "Nyp9wiGaAC4qGWz")
    app.save(record)

    let newRecord = new Record(users)
    newRecord.set("email", "user_tester2@email.com")
    newRecord.set("password", "Xk7mL9pQrS2tUvW")
    app.save(newRecord)
})

