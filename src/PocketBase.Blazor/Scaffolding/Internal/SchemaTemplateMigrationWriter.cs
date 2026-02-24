using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Scaffolding.Internal
{
    internal static class SchemaTemplateMigrationWriter
    {
        private static readonly IReadOnlyDictionary<CommonSchema, (string FileName, string Content)> Templates =
            new Dictionary<CommonSchema, (string FileName, string Content)>
            {
                [CommonSchema.Blog] = ("2990000000_pbblazor_blog_schema.js", BlogSchemaMigration),
                [CommonSchema.Todo] = ("2990000001_pbblazor_todo_schema.js", TodoSchemaMigration),
                [CommonSchema.ECommerce] = ("2990000002_pbblazor_ecommerce_schema.js", ECommerceSchemaMigration)
            };

        internal static async Task WriteAsync(
            IReadOnlyCollection<CommonSchema> schemas,
            PocketBaseHostOptions options,
            ILogger? logger,
            CancellationToken cancellationToken = default)
        {
            if (schemas.Count == 0)
            {
                return;
            }

            string migrationsDir = ResolveMigrationsDirectory(options);
            Directory.CreateDirectory(migrationsDir);

            foreach (CommonSchema schema in schemas)
            {
                if (!Templates.TryGetValue(schema, out (string FileName, string Content) template))
                {
                    continue;
                }

                string fullPath = Path.Combine(migrationsDir, template.FileName);
                await File.WriteAllTextAsync(fullPath, template.Content, cancellationToken);
                (logger ?? NullLogger.Instance).LogInformation(
                    "Schema template migration generated: {Path}",
                    fullPath);
            }
        }

        private static string ResolveMigrationsDirectory(PocketBaseHostOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.MigrationsDir))
            {
                return options.MigrationsDir;
            }

            if (!string.IsNullOrWhiteSpace(options.Dir))
            {
                return Path.Combine(options.Dir, "pb_migrations");
            }

            return "pb_migrations";
        }

        private const string TodoSchemaMigration = """
            /// <reference path="../pb_data/types.d.ts" />

            migrate((app) => {
              const collection = new Collection({
                name: "todos",
                type: "base",
                system: false,
                fields: [
                  { name: "title", type: "text", required: true, presentable: true, system: false },
                  { name: "done", type: "bool", required: false, presentable: true, system: false },
                  { name: "due_at", type: "date", required: false, presentable: false, system: false }
                ],
                indexes: [],
                listRule: "",
                viewRule: "",
                createRule: "",
                updateRule: "",
                deleteRule: ""
              });

              return app.save(collection);
            }, (app) => {
              const collection = app.findCollectionByNameOrId("todos");
              return app.delete(collection);
            });
            """;

        private const string BlogSchemaMigration = """
            /// <reference path="../pb_data/types.d.ts" />

            migrate((app) => {
              const categories = new Collection({
                name: "categories",
                type: "base",
                system: false,
                fields: [
                  { name: "name", type: "text", required: true, presentable: true, system: false },
                  { name: "slug", type: "text", required: true, presentable: true, system: false }
                ],
                indexes: ["CREATE UNIQUE INDEX `idx_categories_slug` ON `categories` (`slug`)"],
                listRule: "",
                viewRule: "",
                createRule: "",
                updateRule: "",
                deleteRule: ""
              });

              const posts = new Collection({
                name: "posts",
                type: "base",
                system: false,
                fields: [
                  { name: "title", type: "text", required: true, presentable: true, system: false },
                  { name: "slug", type: "text", required: true, presentable: true, system: false },
                  { name: "content", type: "editor", required: true, presentable: false, system: false },
                  { name: "is_published", type: "bool", required: false, presentable: true, system: false }
                ],
                indexes: ["CREATE UNIQUE INDEX `idx_posts_slug` ON `posts` (`slug`)"],
                listRule: "",
                viewRule: "",
                createRule: "",
                updateRule: "",
                deleteRule: ""
              });

              app.save(categories);
              return app.save(posts);
            }, (app) => {
              const posts = app.findCollectionByNameOrId("posts");
              const categories = app.findCollectionByNameOrId("categories");

              app.delete(posts);
              return app.delete(categories);
            });
            """;

        private const string ECommerceSchemaMigration = """
            /// <reference path="../pb_data/types.d.ts" />

            migrate((app) => {
              const products = new Collection({
                name: "products",
                type: "base",
                system: false,
                fields: [
                  { name: "name", type: "text", required: true, presentable: true, system: false },
                  { name: "sku", type: "text", required: true, presentable: true, system: false },
                  { name: "price", type: "number", required: true, presentable: true, system: false },
                  { name: "in_stock", type: "bool", required: false, presentable: true, system: false }
                ],
                indexes: ["CREATE UNIQUE INDEX `idx_products_sku` ON `products` (`sku`)"],
                listRule: "",
                viewRule: "",
                createRule: "",
                updateRule: "",
                deleteRule: ""
              });

              const orders = new Collection({
                name: "orders",
                type: "base",
                system: false,
                fields: [
                  { name: "order_number", type: "text", required: true, presentable: true, system: false },
                  { name: "status", type: "text", required: true, presentable: true, system: false },
                  { name: "total", type: "number", required: true, presentable: true, system: false }
                ],
                indexes: ["CREATE UNIQUE INDEX `idx_orders_number` ON `orders` (`order_number`)"],
                listRule: "",
                viewRule: "",
                createRule: "",
                updateRule: "",
                deleteRule: ""
              });

              app.save(products);
              return app.save(orders);
            }, (app) => {
              const orders = app.findCollectionByNameOrId("orders");
              const products = app.findCollectionByNameOrId("products");

              app.delete(orders);
              return app.delete(products);
            });
            """;
    }
}
