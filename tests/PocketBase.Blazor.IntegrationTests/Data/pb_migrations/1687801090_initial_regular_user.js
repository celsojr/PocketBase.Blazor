// pb_migrations/1687801090_initial_regular_user.js

migrate((app) => {
    let users = app.findCollectionByNameOrId("users")

    const testUsers = [
        { email: "user_tester@email.com", password: "Nyp9wiGaAC4qGWz" },
        { email: "user_tester2@email.com", password: "Xk7mL9pQrS2tUvW" }
    ]

    testUsers.forEach(userData => {
        let record = new Record(users)
        record.set("email", userData.email)
        record.set("password", userData.password)
        app.save(record)
    })
})
