# IndeConnect 

## 📁 Architecture du projet

```markdown
IndeConnect-Back/
├── IndeConnect-Back.sln
├── IndeConnect-Back/
│       ├── IndeConnect-Back.csproj
│       ├── Program.cs
│       ├── Controllers/
│       └── appsettings.json
├── IndeConnect-Back.Application/
│       └── IndeConnect-Back.Application.csproj
├── IndeConnect-Back.Domain/
│       └── IndeConnect-Back.Domain.csproj
├── IndeConnect-Back.Infrastructure/
│       ├── IndeConnect-Back.Infrastructure.csproj
│       ├── AppDbContext.cs
│       ├── DependencyInjection.cs
│       ├── AppUser.cs / AppRole.cs
│       └── (migrations à venir)
├── IndeConnect-Back.Web/
│       ├── IndeConnect-Back.Web.csproj
│       ├── Program.cs
│       └── appsettings.json
├── Dockerfile
├── docker-compose.yml
├── .dockerignore
├── .env.example
└── README.DOCKER.md
```

**Organisation logique :**
- `Domain` -> entités et logique métier pure
- `Application` -> services et règles d’application
- `Infrastructure` -> accès aux données (EF Core + PostgreSQL)
- `Web` -> API ASP.NET Core exposée au client
- `Dockerfile`, `docker-compose.yml` -> orchestration et build

---

# 🐳 Dockerisation de **IndeConnect-Back**

Ce guide décrit comment exécuter le backend IndeConnect-Back dans un environnement Docker de production locale avec .NET 9, PostgreSQL 16 et pgAdmin 8.

## ⚙️ Prérequis

- Docker **27+**
- Docker Compose **v2**
- Ports libres :
    - `8080` -> API ASP.NET Core
    - `5432` -> PostgreSQL
    - `5050` -> pgAdmin (facultatif)

---

## 🚀 Démarrage rapide

  ```bash
  cd Indeconnect-Back
  cp .env.example .env
  # Development
  docker compose up --build 
  # Production
  docker compose up -d --build 
  ```

> Les services sont exposés uniquement en local (`127.0.0.1`).  
> L’API est accessible sur http://localhost:8080  
> pgAdmin est accessible sur http://localhost:5050


## 🧩 Architecture Docker

| **Service** | **Image**                                     | **Port** | **Description**                                     |
|:------------|:----------------------------------------------|:---------|:----------------------------------------------------|
| api         | `indeconnect/api:dev`  `indeconnect/api:prod` | 8080     | Conteneur .NET 9 ASP.NET Core servant l’API         |
| db          | `postgres:16`                                 | 5432     | Base de données PostgreSQL                          |
| pgadmin     | `dpage/pgadmin4:8`                            | 5050     | Interface d’administration PostgreSQL (optionnelle) |

			
**Volumes persistants :**
  - `db_data` -> stockage des données PostgreSQL
  - `pgadmin_data` -> configuration pgAdmin

## 🔧 Configuration
L’application lit la chaîne de connexion `ConnectionStrings:Default` depuis les variables d’environnement.

Valeur par défaut (définie dans `docker-compose.yml`) :
```ini
Host=db;Port=5432;Database=indeconnect;Username=indeconnect;Password=indeconnect
```

Vous pouvez personnaliser ces valeurs dans `.env` :
```bash
POSTGRES_DB=indeconnect
POSTGRES_USER=indeconnect
POSTGRES_PASSWORD=indeconnect
```

Puis modifier la variable dans `docker-compose.yml` :
```yaml
ConnectionStrings__Default: "Host=db;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}"
```

## 🧱 Migrations Entity Framework Core
Aucune migration n’est encore présente. Pour créer la première :

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate -p IndeConnect-Back.Infrastructure -s IndeConnect-Back.Web
dotnet ef database update -p IndeConnect-Back.Infrastructure -s IndeConnect-Back.Web
```

Ensuite, rebuild l’image :

```bash
docker compose build api
```

### Appliquer automatiquement les migrations au démarrage

Ajoutez ceci dans Program.cs :

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
```

## ❤️ Santé et supervision
Le `docker-compose.yml` inclut un healthcheck basique :

```yaml
test: ["CMD-SHELL", "curl -fsS http://localhost:8080/health >/dev/null || exit 1"]
```
> L’API expose un endpoint `/health` utilisé par le healthcheck Docker.

## 🧰 Commandes utiles
```bash
# Lancer en arrière-plan
docker compose up -d

# Voir les logs
docker compose logs -f api

# Rebuild sans cache
docker compose build --no-cache api

# Supprimer les conteneurs/volumes
docker compose down -v
```

## 🧾 Notes supplémentaires
- **Base de données** : PostgreSQL est utilisée via `Npgsql.EntityFrameworkCore.PostgreSQL`.
- **Sécurité** : Le conteneur exécute l’application sous un utilisateur non-root (`appuser`), avec un système de fichiers en lecture seule (`read_only: true`), un espace temporaire isolé (`tmpfs /tmp`) et `no-new-privileges:true` pour limiter les permissions.
- **Environnement** : `ASPNETCORE_ENVIRONMENT=Development` ou `ASPNETCORE_ENVIRONMENT=Production`.
- **Ports exposés** : modifiables dans `docker-compose.yml`.

## ✅ Résumé

| Élément             | Statut                |
|:--------------------|:----------------------|
| .NET SDK            | 9.0                   |
| Base de données     | PostgreSQL 16         |
| Orchestration       | Docker Compose v3.9   |
| Migrations EF Core  | à créer               |
| Endpoint Health     | `/health` présent     |


## 🧭 Prochaines étapes
1. Ajouter les entités et migrations. 
2. Tester l’API sur localhost:8080.
3. Ajouter la base de données et la tester sur prod.