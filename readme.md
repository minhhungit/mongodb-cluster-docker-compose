Demo Mongo Sharded Cluster with Docker Compose
=========================================

### Mongo Components

* Config Server (3 member replica set): `configsvr01`,`configsvr02`,`configsvr03`
* 3 Shards (each a 3 member replica set):
	* `shard01-a`,`shard01-b`, `shard01-c`
	* `shard02-a`,`shard02-b`, `shard02-c`
	* `shard03-a`,`shard03-b`, `shard03-c`
* 2 Routers (mongos): `router01`, `router02`

### Commands
**Start all of the containers**

```sh
docker-compose up -d
```

**Initialize the replica sets (config servers and shards) and routers**

```sh
docker-compose exec configsvr01 sh -c "mongo < /scripts/init-configserver.js"
docker-compose exec shard01-a sh -c "mongo < /scripts/init-shard01.js"
docker-compose exec shard02-a sh -c "mongo < /scripts/init-shard02.js"
docker-compose exec shard03-a sh -c "mongo < /scripts/init-shard03.js"
```

Wait a bit for the config server and shards to elect their primaries before initializing the router

```sh
docker-compose exec router01 sh -c "mongo < /scripts/init-router.js"
```


**Verify the status of the sharded cluster**

```sh
docker-compose exec router01 mongo --port 27017
sh.status()
```
*Sample Result:*
```
  sharding version: {
        "_id" : 1,
        "minCompatibleVersion" : 5,
        "currentVersion" : 6,
        "clusterId" : ObjectId("5d38fb010eac1e03397c355a")
  }
  shards:
        {  "_id" : "rs-shard-01",  "host" : "rs-shard-01/shard01-a:27017,shard01-b:27017,shard01-c:27017",  "state" : 1 }
        {  "_id" : "rs-shard-02",  "host" : "rs-shard-02/shard02-a:27017,shard02-b:27017,shard02-c:27017",  "state" : 1 }
        {  "_id" : "rs-shard-03",  "host" : "rs-shard-03/shard03-a:27017,shard03-b:27017,shard03-c:27017",  "state" : 1 }
  active mongoses:
        "4.0.10" : 2
  autosplit:
        Currently enabled: yes
  balancer:
        Currently enabled:  yes
        Currently running:  no
        Failed balancer rounds in last 5 attempts:  0
        Migration Results for the last 24 hours:
                No recent migrations
  databases:
        {  "_id" : "config",  "primary" : "config",  "partitioned" : true }
```

**Enable sharding for database `MyDatabase`**

```sh
sh.enableSharding("MyDatabase")
```

**Setup shardingKey for collection `MyCollection`**
```sh
db.adminCommand( { shardCollection: "MyDatabase.MyCollection", key: { supplierId: "hashed" } } )
```

**Check database status**
```sh
use MyDatabase
db.stats()
db.MyCollection.getShardDistribution()
```

*Sample Result:*
```
{
        "raw" : {
                "rs-shard-01/shard01-a:27017,shard01-b:27017,shard01-c:27017" : {
                        "db" : "MyDatabase",
                        "collections" : 1,
                        "views" : 0,
                        "objects" : 0,
                        "avgObjSize" : 0,
                        "dataSize" : 0,
                        "storageSize" : 4096,
                        "numExtents" : 0,
                        "indexes" : 2,
                        "indexSize" : 8192,
                        "fsUsedSize" : 12439990272,
                        "fsTotalSize" : 62725787648,
                        "ok" : 1
                },
                "rs-shard-03/shard03-a:27017,shard03-b:27017,shard03-c:27017" : {
                        "db" : "MyDatabase",
                        "collections" : 1,
                        "views" : 0,
                        "objects" : 0,
                        "avgObjSize" : 0,
                        "dataSize" : 0,
                        "storageSize" : 4096,
                        "numExtents" : 0,
                        "indexes" : 2,
                        "indexSize" : 8192,
                        "fsUsedSize" : 12439994368,
                        "fsTotalSize" : 62725787648,
                        "ok" : 1
                },
                "rs-shard-02/shard02-a:27017,shard02-b:27017,shard02-c:27017" : {
                        "db" : "MyDatabase",
                        "collections" : 1,
                        "views" : 0,
                        "objects" : 0,
                        "avgObjSize" : 0,
                        "dataSize" : 0,
                        "storageSize" : 4096,
                        "numExtents" : 0,
                        "indexes" : 2,
                        "indexSize" : 8192,
                        "fsUsedSize" : 12439994368,
                        "fsTotalSize" : 62725787648,
                        "ok" : 1
                }
        },
        "objects" : 0,
        "avgObjSize" : 0,
        "dataSize" : 0,
        "storageSize" : 12288,
        "numExtents" : 0,
        "indexes" : 6,
        "indexSize" : 24576,
        "fileSize" : 0,
        "extentFreeList" : {
                "num" : 0,
                "totalSize" : 0
        },
        "ok" : 1,
        "operationTime" : Timestamp(1564004884, 36),
        "$clusterTime" : {
                "clusterTime" : Timestamp(1564004888, 1),
                "signature" : {
                        "hash" : BinData(0,"AAAAAAAAAAAAAAAAAAAAAAAAAAA="),
                        "keyId" : NumberLong(0)
                }
        }
}
```

**More commands**

```sh
docker exec -it rydell-mongo-config-01 bash -c "echo 'rs.status()' | mongo --port 27017"

docker exec -it rydell-shard-01-node-a bash -c "echo 'rs.status()' | mongo --port 27017" 
```


### Normal Startup
The cluster only has to be initialized on the first run. Subsequent startup can be achieved simply with `docker-compose up` or `docker-compose up -d`

### Resetting the Cluster
To remove all data and re-initialize the cluster, make sure the containers are stopped and then:

```sh
docker-compose rm
```

### Clean up docker-compose
```sh
docker-compose down -v --rmi all --remove-orphans
```

Execute the **First Run** instructions again.

### Screenshot

<img src="https://raw.githubusercontent.com/minhhungit/mongodb-cluster-docker-compose/master/images/demo.png" style="width: 100%;" />
<img src="https://raw.githubusercontent.com/minhhungit/mongodb-cluster-docker-compose/master/images/demo-03.png" style="width: 100%;" />
<img src="https://raw.githubusercontent.com/minhhungit/mongodb-cluster-docker-compose/master/images/demo-02.png" style="width: 100%;" />

---
#### Inspired by:
- https://github.com/jfollenfant/mongodb-sharding-docker-compose
- https://viblo.asia/p/cai-dat-mongo-cluster-voi-docker-m68Z0NN25kG
