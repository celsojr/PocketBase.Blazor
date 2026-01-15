// 1687800970_seed_categories.js

migrate((app) => {
    const categories = app.findCollectionByNameOrId("categories")

    const data = [
        { name: "Technology", slug: "technology" },
        { name: "Lifestyle", slug: "lifestyle" },
        { name: "News", slug: "news" },
    ]

    for (const item of data) {
        const record = new Record(categories)
        record.set("name", item.name)
        record.set("slug", item.slug)

        app.save(record)
    }
})

