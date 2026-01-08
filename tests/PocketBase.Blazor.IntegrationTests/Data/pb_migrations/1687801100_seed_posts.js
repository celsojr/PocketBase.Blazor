// pb_migrations/1687801100_seed_posts.js

migrate((app) => {
    const posts = app.findCollectionByNameOrId("posts")

    const data = [
        {
            author: "Alice",
            title: "First Post",
            slug: "first-post",
            content: "<p>This is the content of the first post.</p>",
            is_published: true,
        },
        {
            author: "Bob",
            title: "Second Post",
            slug: "second-post",
            content: "<p>Another interesting post for the blog.</p>",
            is_published: true,
        },
        {
            author: "Charlie",
            title: "Third Post",
            slug: "third-post",
            content: "<p>Blogging is fun with PocketBase!</p>",
            is_published: true,
        },
        {
            author: "Dana",
            title: "Fourth Post",
            slug: "fourth-post",
            content: "<p>Let&rsquo;s add more dummy data.</p>",
            is_published: true,
        },
        {
            author: "Eve",
            title: "Fifth Post",
            slug: "fifth-post",
            content: "<p>Halfway through our dummy posts.</p>",
            is_published: false,
        },
        {
            author: "Frank",
            title: "Sixth Post",
            slug: "sixth-post",
            content: "<p>This post is just for testing.</p>",
            is_published: true,
        },
        {
            author: "Grace",
            title: "Seventh Post",
            slug: "seventh-post",
            content: "<p>Seventh time&rsquo;s the charm.</p>",
            is_published: true,
        },
        {
            author: "Heidi",
            title: "Eighth Post",
            slug: "eighth-post",
            content: "<p>Almost done with our sample data.</p>",
            is_published: false,
        },
        {
            author: "Ivan",
            title: "Ninth Post",
            slug: "ninth-post",
            content: "<p>Just one more after this!</p>",
            is_published: true,
        },
        {
            author: "Judy",
            title: "Tenth Post",
            slug: "tenth-post",
            content: "<p>This is the last dummy post.</p>",
            is_published: true,
        },
    ]

    for (const item of data) {
        const record = new Record(posts)

        record.set("author", item.author)
        record.set("title", item.title)
        record.set("slug", item.slug)
        record.set("content", item.content)
        record.set("is_published", item.is_published)

        app.save(record)
    }
}, (app) => {
        const slugs = [
            "first-post",
            "second-post",
            "third-post",
            "fourth-post",
            "fifth-post",
            "sixth-post",
            "seventh-post",
            "eighth-post",
            "ninth-post",
            "tenth-post"
        ]

        for (const slug of slugs) {
            const record = $app.findFirstRecordByData("posts", "slug", slug)
            if (record) {
                app.delete(record)
            }
        }
    }
)

