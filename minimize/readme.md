Demo Mongo Sharded Cluster with Docker Compose
=========================================

* Config Server: `configsvr01`
* 2 Shards (each a 2 member `PSS` replica set):
	* `shard01-a`,`shard01-b`, `shard01-c`
	* `shard02-a`,`shard02-b`, `shard02-c`
* 1 Routers (mongos): `router01`

### step 1
```bash
docker-compose up -d
```

### 👉 Step 2

```bash
docker-compose exec configsvr01 sh -c "mongo < /scripts/init-configserver.js"

docker-compose exec shard01-a sh -c "mongo < /scripts/init-shard01.js"
docker-compose exec shard02-a sh -c "mongo < /scripts/init-shard02.js"
```

### 👉 Step 3
>Note: Wait a bit for the config server and shards to elect their primaries before initializing the router

```bash
docker-compose exec router01 sh -c "mongo < /scripts/init-router.js"
```

### 👉 Step 4
```bash
docker-compose exec router01 mongo --port 27017

// Enable sharding for database `MyDatabase`
sh.enableSharding("MyDatabase")

// Setup shardingKey for collection `MyCollection`**
db.adminCommand( { shardCollection: "MyDatabase.MyCollection", key: { oemNumber: "hashed", zipCode: 1, supplierId: 1 } } )

```

---
### ✔️ Done !!!

#### But before you start inserting data you should verify them first
---

If you want to add new shard to existed cluster, check more here 

## 📋 Verify [🔝](#-table-of-contents)

### ✅ Verify the status of the sharded cluster [🔝](#-table-of-contents)

```bash
docker-compose exec router01 mongo --port 27017
sh.status()
```

### ✅ Verify status of replica set for each shard [🔝](#-table-of-contents)
> You should see 1 PRIMARY, 2 SECONDARY

```bash
docker exec -it shard-01-node-a bash -c "echo 'rs.status()' | mongo --port 27017" 
docker exec -it shard-02-node-a bash -c "echo 'rs.status()' | mongo --port 27017" 
```

### ✅ Check database status
```bash
docker-compose exec router01 mongo --port 27017
use MyDatabase
db.stats()
db.MyCollection.getShardDistribution()
```

### 🔎 More commands 

```bash
docker exec -it mongo-config-01 bash -c "echo 'rs.status()' | mongo --port 27017"


docker exec -it shard-01-node-a bash -c "echo 'rs.help()' | mongo --port 27017"
docker exec -it shard-01-node-a bash -c "echo 'rs.status()' | mongo --port 27017" 
docker exec -it shard-01-node-a bash -c "echo 'rs.printReplicationInfo()' | mongo --port 27017" 
docker exec -it shard-01-node-a bash -c "echo 'rs.printSlaveReplicationInfo()' | mongo --port 27017"
```