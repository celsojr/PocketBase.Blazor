// pb_migrations/1687801090_initial_superuser.js

migrate((app) => {
    let superusers = app.findCollectionByNameOrId("_superusers")

    let record = new Record(superusers)

    record.set("email", "admin_tester@email.com")
    record.set("password", "gDxTCmnq7K8xyKn")

    app.save(record)
})

