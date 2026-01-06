/// <reference path="../pb_data/types.d.ts" />
migrate((app) => {
  const collection = app.findCollectionByNameOrId("pbc_3944521702");

  return app.delete(collection);
}, (app) => {
  const collection = new Collection({
    "createRule": null,
    "deleteRule": null,
    "fields": [
      {
        "autogeneratePattern": "",
        "hidden": false,
        "id": "text3208210256",
        "max": 0,
        "min": 0,
        "name": "id",
        "pattern": "^[a-z0-9]+$",
        "presentable": false,
        "primaryKey": true,
        "required": true,
        "system": true,
        "type": "text"
      },
      {
        "exceptDomains": null,
        "hidden": false,
        "id": "_clone_qGVg",
        "name": "email",
        "onlyDomains": null,
        "presentable": false,
        "required": true,
        "system": true,
        "type": "email"
      }
    ],
    "id": "pbc_3944521702",
    "indexes": [],
    "listRule": "@request.auth.id != \"\"",
    "name": "exampleView",
    "system": false,
    "type": "view",
    "updateRule": null,
    "viewQuery": "SELECT id, email FROM users",
    "viewRule": null
  });

  return app.save(collection);
})
