/// <reference path="../pb_data/types.d.ts" />
migrate((app) => {
  const collection = app.findCollectionByNameOrId("pbc_1125843985")

  // update collection data
  unmarshal({
    "indexes": [
      "CREATE INDEX `idx_P1U2DlTSyL` ON `posts` (`title`)",
      "CREATE INDEX `idx_785jc6wkFu` ON `posts` (`slug`)"
    ]
  }, collection)

  // add field
  collection.fields.addAt(4, new Field({
    "hidden": false,
    "id": "bool1875119480",
    "name": "is_published",
    "presentable": false,
    "required": false,
    "system": false,
    "type": "bool"
  }))

  // add field
  collection.fields.addAt(5, new Field({
    "autogeneratePattern": "",
    "hidden": false,
    "id": "text2560465762",
    "max": 0,
    "min": 0,
    "name": "slug",
    "pattern": "",
    "presentable": false,
    "primaryKey": false,
    "required": true,
    "system": false,
    "type": "text"
  }))

  return app.save(collection)
}, (app) => {
  const collection = app.findCollectionByNameOrId("pbc_1125843985")

  // update collection data
  unmarshal({
    "indexes": [
      "CREATE INDEX `idx_P1U2DlTSyL` ON `posts` (`title`)"
    ]
  }, collection)

  // remove field
  collection.fields.removeById("bool1875119480")

  // remove field
  collection.fields.removeById("text2560465762")

  return app.save(collection)
})
