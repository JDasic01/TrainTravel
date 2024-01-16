**Starting the project**
```
docker compose build --no-cache
docker compose up -d
```
**Endpoints**
Blazor - localhost:8081
API - localhost:8082
- for development UI -> localhost:8082/swagger

**Check DB for tables**
```
docker compose exec database psql -U myuser -d mydatabase

##### IN DB SHELL(shows all tables)
\dt
```
expected output
```
docker compose exec database psql -U myuser -d mydatabase
psql (16.1 (Debian 16.1-1.pgdg120+1))
Type "help" for help.

mydatabase=# \dt
          List of relations
 Schema |    Name    | Type  | Owner  
--------+------------+-------+--------
 public | cities     | table | myuser
 public | cityroutes | table | myuser
 public | routes     | table | myuser
(3 rows)

mydatabase=# 
```
if there are no tables, execute first_migration.sql

**Executing first migration**
```
docker compose exec database psql -U myuser -d mydatabase -f /docker-entrypoint-initdb.d/first_migration.sql
```
command for executing migrations is
```
docker compose exec database psql -U myuser -d mydatabase -f /docker-entrypoint-initdb.d/MIGRATION_NAME.sql
```

  
**CSV structure**
```
StartDestination,EndDestination,Mileage
CityA,CityB,100
CityA,CityC,150
CityB,CityC,75
```
mislim da nece radit migracije jos
