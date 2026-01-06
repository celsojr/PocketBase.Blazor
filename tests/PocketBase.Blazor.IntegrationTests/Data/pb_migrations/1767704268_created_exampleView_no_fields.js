/// <reference path="../pb_data/types.d.ts" />
migrate((app) => {
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
        "id": "_clone_kPVi",
        "name": "email",
        "onlyDomains": null,
        "presentable": false,
        "required": true,
        "system": true,
        "type": "email"
      }
    ],
    "id": "pbc_690397367",
    "indexes": [],
    "listRule": null,
    "name": "exampleView_no_fields",
    "system": false,
    "type": "view",
    "updateRule": null,
    "viewQuery": "SELECT id, email FROM users",
    "viewRule": null
  });

  return app.save(collection);
}, (app) => {
  const collection = app.findCollectionByNameOrId("pbc_690397367");

  return app.delete(collection);
})
