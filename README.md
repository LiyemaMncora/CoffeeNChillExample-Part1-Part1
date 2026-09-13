# CoffeeNChill — Canteen Management System

**Part 1 — Azure Functions, Tables, Blob Storage & Docker Hub**

CoffeeNChill is a campus canteen. This part of the project replaces the paper
menus and filing-cabinet documents with:

- **Azure Table Storage** for menu items (price, category, availability).
- **Azure Blob Storage** for staff documents (recipe sheets, cleaning
  manuals, health & safety policies) — per the assignment addendum, Blob
  Storage is used instead of Azure File Storage, since File shares are not
  emulated by Azurite.
- **HTTP-triggered Azure Functions** exposing both as a small REST API.
- **Docker** to run Azurite and the Functions app as two independent,
  standalone containers (no Docker Compose in Part 1).

---

## Project structure

```
CoffeeNChillExample-main/
├── CoffeeNChillExample.slnx
├── docs/
│   └── CoffeeNChill.postman_collection.json   # Postman collection + tests
└── CoffeeNChillExample/
    ├── CoffeeNChillExample.csproj
    ├── Dockerfile
    ├── host.json
    ├── local.settings.json                    # points at Azurite locally
    ├── Program.cs
    ├── Functions/                              # HTTP-triggered endpoints
    ├── Models/                                 # MenuItem, StaffDocument
    └── Services/                               # TableService, BlobService
```

---

## API Endpoints

### Menu Items (Azure Table Storage — table `Menuitems`)

| Method | Route                          | Description                              |
|--------|---------------------------------|-------------------------------------------|
| POST   | `/api/menu`                     | Create a new menu item                    |
| GET    | `/api/menu`                     | Get all menu items                        |
| GET    | `/api/menu/category/{category}` | Get menu items filtered by category       |
| PUT    | `/api/menu/{category}/{id}`     | Update a menu item's price/availability   |
| DELETE | `/api/menu/{category}/{id}`     | Delete a menu item                        |

A menu item body looks like:

```json
{
    "PartitionKey": "Hot Drinks",
    "RowKey": "COF-001",
    "Name": "Espresso",
    "Description": "Single shot of rich, bold espresso",
    "Price": 25.00,
    "IsAvailable": true
}
```

### Staff Documents (Azure Blob Storage — container `staff-docs`)

| Method | Route                                | Description                                          |
|--------|---------------------------------------|-------------------------------------------------------|
| POST   | `/api/documents/upload`               | Upload a file (`multipart/form-data`, key `file`)      |
| GET    | `/api/documents`                      | List all files (name, size, last modified)            |
| GET    | `/api/documents/download/{fileName}`  | Download a file by name                                |

---

## Prerequisites

- .NET SDK 10.0
- [Azure Functions Core Tools v4](https://learn.microsoft.com/azure/azure-functions/functions-run-local)
- Docker Desktop
- Postman

---

## Running locally (without Docker)

1. Start Azurite (either the VS Code extension, `npm install -g azurite && azurite`,
   or the Docker container from step 1 below — any of these work, since all
   that's needed is the emulator listening on the default ports).
2. `local.settings.json` in `CoffeeNChillExample/` already points at Azurite:
   ```json
   {
       "IsEncrypted": false,
       "Values": {
           "AzureWebJobsStorage": "UseDevelopmentStorage=true",
           "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
       }
   }
   ```
3. From `CoffeeNChillExample/`, run:
   ```bash
   func start
   ```
4. The API is now available at `http://localhost:7071/api/...`.

---

## Running as standalone Docker containers (no Docker Compose)

### 1. Run Azurite in its own container

```bash
docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite
```

Tag and push it to your own Docker Hub repository as required by the rubric:

```bash
docker pull mcr.microsoft.com/azure-storage/azurite
docker tag mcr.microsoft.com/azure-storage/azurite <dockerhub_username>/coffeenchill-azurite:v1.0
docker push <dockerhub_username>/coffeenchill-azurite:v1.0
```

### 2. Build and push the Functions image

The `Dockerfile` in `CoffeeNChillExample/` compiles and publishes the app
inside the image (multi-stage build), then bakes in
`AzureWebJobsStorage=UseDevelopmentStorage=true`, so no connection string
needs to be passed on the command line.

```bash
cd CoffeeNChillExample
docker build -t <dockerhub_username>/coffeenchill-functions:v1.0 .
docker push <dockerhub_username>/coffeenchill-functions:v1.0
```

### 3. Run the Functions container

```bash
docker run -p 7071:80 <dockerhub_username>/coffeenchill-functions:v1.0
```

> **Note on container networking:** `UseDevelopmentStorage=true` resolves to
> `127.0.0.1`, which inside a container means "this container", not the
> host machine running Azurite. If the Functions container can't reach
> Azurite, either run both containers with `--network host` (Linux), or put
> them on the same Docker network and use an explicit connection string with
> `host.docker.internal` in place of `127.0.0.1` for the Blob/Table
> endpoints.

### 4. Test with Postman

Import [`docs/CoffeeNChill.postman_collection.json`](docs/CoffeeNChill.postman_collection.json)
into Postman. It contains requests and automated tests (status codes and
response-shape assertions) for every endpoint above, grouped into **Menu
Items** and **Staff Documents** folders. For the upload request, attach a
file under the `file` form-data key before sending.

---

## Team contributions

<!-- TODO: list each group member and what they worked on -->
- **Name:** …
- **Name:** …

## Video demonstration

<!-- TODO: add the unlisted YouTube link -->
[Watch the demo](#)
