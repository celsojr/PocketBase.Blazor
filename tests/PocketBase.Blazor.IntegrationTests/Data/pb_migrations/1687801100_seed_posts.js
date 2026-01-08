// pb_migrations/1687801100_seed_posts.js

migrate((app) => {
    const posts = app.findCollectionByNameOrId("posts")

    const data = [
        {
            id: "lxe9zgmyl4vxu48",
            author: "Alice",
            title: "First Post",
            slug: "first-post",
            content: "<p>This is the content of the first post.</p>",
            is_published: true,
            created: "2025-12-21 15:28:00.564Z",
            updated: "2025-12-21 17:16:40.912Z",
        },
        {
            id: "43jqe8ij7oy9afy",
            author: "Bob",
            title: "Second Post",
            slug: "second-post",
            content: "<p>Another interesting post for the blog.</p>",
            is_published: true,
            created: "2025-12-21 15:29:01.943Z",
            updated: "2025-12-21 17:16:56.220Z",
        },
        {
            id: "j4ac9spkucdtcb9",
            author: "Charlie",
            title: "Third Post",
            slug: "third-post",
            content: "<p>Blogging is fun with PocketBase!</p>",
            is_published: true,
            created: "2025-12-21 15:29:34.839Z",
            updated: "2025-12-21 17:16:16.158Z",
        },
        {
            id: "yul1x8ua5qj5v9v",
            author: "Dana",
            title: "Fourth Post",
            slug: "fourth-post",
            content: "<p>Let&rsquo;s add more dummy data.</p>",
            is_published: true,
            created: "2025-12-21 15:29:49.704Z",
            updated: "2025-12-21 17:15:48.974Z",
        },
        {
            id: "trq1wxw7ch2aj3r",
            author: "Eve",
            title: "Fifth Post",
            slug: "fifth-post",
            content: "<p>Halfway through our dummy posts.</p>",
            is_published: false,
            created: "2025-12-21 15:30:03.028Z",
            updated: "2025-12-21 17:15:57.230Z",
        },
        {
            id: "1zmto1t5k4ntnk9",
            author: "Frank",
            title: "Sixth Post",
            slug: "sixth-post",
            content: "<p>This post is just for testing.</p>",
            is_published: true,
            created: "2025-12-21 15:30:14.334Z",
            updated: "2025-12-21 17:15:14.824Z",
        },
        {
            id: "cwpfp05lpnopbq6",
            author: "Grace",
            title: "Seventh Post",
            slug: "seventh-post",
            content: "<p>Seventh time&rsquo;s the charm.</p>",
            is_published: true,
            created: "2025-12-21 15:30:30.494Z",
            updated: "2025-12-21 17:15:00.808Z",
        },
        {
            id: "7sfcqks933gfe1z",
            author: "Heidi",
            title: "Eighth Post",
            slug: "eighth-post",
            content: "<p>Almost done with our sample data.</p>",
            is_published: false,
            created: "2025-12-21 15:30:43.077Z",
            updated: "2025-12-21 17:17:08.591Z",
        },
        {
            id: "b4su072kshh5xet",
            author: "Ivan",
            title: "Ninth Post",
            slug: "ninth-post",
            content: "<p>Just one more after this!</p>",
            is_published: true,
            created: "2025-12-21 15:30:56.036Z",
            updated: "2025-12-21 17:14:29.833Z",
        },
        {
            id: "ixodttnw2w3jouz",
            author: "Judy",
            title: "Tenth Post",
            slug: "tenth-post",
            content: "<p>This is the last dummy post.</p>",
            is_published: true,
            created: "2025-12-21 15:31:13.413Z",
            updated: "2025-12-21 17:14:12.739Z",
        },
    ]

    for (const item of data) {
        const record = new Record(posts)

        record.set("id", item.id)
        record.set("author", item.author)
        record.set("title", item.title)
        record.set("slug", item.slug)
        record.set("content", item.content)
        record.set("is_published", item.is_published)
        record.set("created", item.created)
        record.set("updated", item.updated)

        app.save(record)
    }
}, (app) => {
        const posts = app.findCollectionByNameOrId("posts")

        const ids = [
            "lxe9zgmyl4vxu48",
            "43jqe8ij7oy9afy",
            "j4ac9spkucdtcb9",
            "yul1x8ua5qj5v9v",
            "trq1wxw7ch2aj3r",
            "1zmto1t5k4ntnk9",
            "cwpfp05lpnopbq6",
            "7sfcqks933gfe1z",
            "b4su072kshh5xet",
            "ixodttnw2w3jouz",
        ]

        for (const id of ids) {
            const record = app.findRecordById(posts, id)
            if (record) {
                app.delete(record)
            }
        }
    }
)

