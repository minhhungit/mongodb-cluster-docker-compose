Demo Mongo Sharded Cluster with Docker Compose
=========================================

## PSA Style (Primary - Secondary - Arbiter)

Need PSS? Check [here](https://github.com/minhhungit/mongodb-cluster-docker-compose)

---

### WARNING (Windows & OS X) 

>The default Docker setup on Windows and OS X uses a VirtualBox VM to host the Docker daemon. 
>Unfortunately, the mechanism VirtualBox uses to share folders between the host system and 
>the Docker container is not compatible with the memory mapped files used by MongoDB 
>(see [vbox bug](https://www.virtualbox.org/ticket/819), [docs.mongodb.org](https://docs.mongodb.com/manual/administration/production->notes/#fsync-on-directories) 
>and related [jira.mongodb.org bug](https://jira.mongodb.org/browse/SERVER-8600)). 
>This means that it is not possible to run a MongoDB container with the data directory mapped to the host.
>
>&#8211; Docker Hub ([source here](https://github.com/docker-library/docs/blob/b78d49c9dffe5dd8b3ffd1db338c62b9e1fc3db8/mongo/content.md#where-to-store-data) 
>or [here](https://github.com/docker-library/mongo/issues/232#issuecomment-355423692))
---

### Mongo Components

* Config Server (3 member replica set): `configsvr01`,`configsvr02`,`configsvr03`
* 3 Shards (each a 3 member `PSA` replica set):
	* `shard01-a`,`shard01-b` and 1 arbiter `shard01-x`
	* `shard02-a`,`shard02-b` and 1 arbiter `shard02-x`
	* `shard03-a`,`shard03-b` and 1 arbiter `shard03-x`
* 2 Routers (mongos): `router01`, `router02`

<img src="https://raw.githubusercontent.com/minhhungit/mongodb-cluster-docker-compose/master/images/sharding-and-replica-sets.png" style="width: 100%;" />

### Setup
- **Step 1: Start all of the containers**

```bash
docker-compose up -d
```

- **Step 2: Initialize the replica sets (config servers and shards) and routers**

```bash
docker-compose exec configsvr01 sh -c "mongosh < /scripts/init-configserver.js"

docker-compose exec shard01-a sh -c "mongosh < /scripts/init-shard01.js"
docker-compose exec shard02-a sh -c "mongosh < /scripts/init-shard02.js"
docker-compose exec shard03-a sh -c "mongosh < /scripts/init-shard03.js"
```

- **Step 3: Connect to the primary and add arbiters**
```bash
docker-compose exec shard01-a mongosh --port 27017
rs.addArb("shard01-x:27017") // make sure that you are in primary before run this command
// or
docker exec -it shard-01-node-a bash -c "echo 'rs.addArb(\""shard01-x:27017\"")' | mongosh --port 27017"
```

```bash
docker-compose exec shard02-a mongosh --port 27017
rs.addArb("shard02-x:27017") // make sure that you are in primary before run this command
// or
docker exec -it shard-02-node-a bash -c "echo 'rs.addArb(\""shard02-x:27017\"")' | mongosh --port 27017"
```

```bash
docker-compose exec shard03-a mongosh --port 27017
rs.addArb("shard03-x:27017") // make sure that you are in primary before run this command
// or
docker exec -it shard-03-node-a bash -c "echo 'rs.addArb(\""shard03-x:27017\"")' | mongosh --port 27017"
```

- **Step 4: Initializing the router**
>Note: Wait a bit for the config server and shards to elect their primaries before initializing the router

```bash
docker-compose exec router01 sh -c "mongosh < /scripts/init-router.js"
```

- **Step 5: Enable sharding and setup sharding-key**
```bash
docker-compose exec router01 mongosh --port 27017

// Enable sharding for database `MyDatabase`
sh.enableSharding("MyDatabase")

// Setup shardingKey for collection `MyCollection`**
db.adminCommand( { shardCollection: "MyDatabase.MyCollection", key: { supplierId: "hashed" } } )

```

>Done! but before you start inserting data you should verify them first

### Verify

- **Verify the status of the sharded cluster**

```bash
docker-compose exec router01 mongosh --port 27017
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
        {  "_id" : "rs-shard-01",  "host" : "rs-shard-01/shard01-a:27017,shard01-b:27017",  "state" : 1 }
        {  "_id" : "rs-shard-02",  "host" : "rs-shard-02/shard02-a:27017,shard02-b:27017",  "state" : 1 }
        {  "_id" : "rs-shard-03",  "host" : "rs-shard-03/shard03-a:27017,shard03-b:27017",  "state" : 1 }
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

- **Verify status of replica set for each shard**
> You should see 1 PRIMARY, 1 SECONDARY and 1 ARBITER

```bash
docker exec -it shard-01-node-a bash -c "echo 'rs.status()' | mongosh --port 27017" 
docker exec -it shard-02-node-a bash -c "echo 'rs.status()' | mongosh --port 27017" 
docker exec -it shard-03-node-a bash -c "echo 'rs.status()' | mongosh --port 27017" 
```
*Sample Result:*
```
MongoDB shell version v4.0.11
connecting to: mongodb://127.0.0.1:27017/?gssapiServiceName=mongodb
Implicit session: session { "id" : UUID("dcfe5d8f-75ef-45f7-9595-9d72dc8a81fc") }
MongoDB server version: 4.0.11
{
        "set" : "rs-shard-01",
        "date" : ISODate("2019-08-01T06:53:59.175Z"),
        "myState" : 1,
        "term" : NumberLong(1),
        "syncingTo" : "",
        "syncSourceHost" : "",
        "syncSourceId" : -1,
        "heartbeatIntervalMillis" : NumberLong(2000),
        "optimes" : {
                "lastCommittedOpTime" : {
                        "ts" : Timestamp(1564642438, 1),
                        "t" : NumberLong(1)
                },
                "readConcernMajorityOpTime" : {
                        "ts" : Timestamp(1564642438, 1),
                        "t" : NumberLong(1)
                },
                "appliedOpTime" : {
                        "ts" : Timestamp(1564642438, 1),
                        "t" : NumberLong(1)
                },
                "durableOpTime" : {
                        "ts" : Timestamp(1564642438, 1),
                        "t" : NumberLong(1)
                }
        },
        "lastStableCheckpointTimestamp" : Timestamp(1564642428, 1),
        "members" : [
                {
                        "_id" : 0,
                        "name" : "shard01-a:27017",
                        "health" : 1,
                        "state" : 1,
                        "stateStr" : "PRIMARY",
                        "uptime" : 390,
                        "optime" : {
                                "ts" : Timestamp(1564642438, 1),
                                "t" : NumberLong(1)
                        },
                        "optimeDate" : ISODate("2019-08-01T06:53:58Z"),
                        "syncingTo" : "",
                        "syncSourceHost" : "",
                        "syncSourceId" : -1,
                        "infoMessage" : "",
                        "electionTime" : Timestamp(1564642306, 1),
                        "electionDate" : ISODate("2019-08-01T06:51:46Z"),
                        "configVersion" : 2,
                        "self" : true,
                        "lastHeartbeatMessage" : ""
                },
                {
                        "_id" : 1,
                        "name" : "shard01-b:27017",
                        "health" : 1,
                        "state" : 2,
                        "stateStr" : "SECONDARY",
                        "uptime" : 142,
                        "optime" : {
                                "ts" : Timestamp(1564642428, 1),
                                "t" : NumberLong(1)
                        },
                        "optimeDurable" : {
                                "ts" : Timestamp(1564642428, 1),
                                "t" : NumberLong(1)
                        },
                        "optimeDate" : ISODate("2019-08-01T06:53:48Z"),
                        "optimeDurableDate" : ISODate("2019-08-01T06:53:48Z"),
                        "lastHeartbeat" : ISODate("2019-08-01T06:53:57.953Z"),
                        "lastHeartbeatRecv" : ISODate("2019-08-01T06:53:57.967Z"),
                        "pingMs" : NumberLong(0),
                        "lastHeartbeatMessage" : "",
                        "syncingTo" : "shard01-a:27017",
                        "syncSourceHost" : "shard01-a:27017",
                        "syncSourceId" : 0,
                        "infoMessage" : "",
                        "configVersion" : 2
                },
                {
                        "_id" : 3,
                        "name" : "shard01-x:27017",
                        "health" : 1,
                        "state" : 7,
                        "stateStr" : "ARBITER",
                        "uptime" : 99,
                        "lastHeartbeat" : ISODate("2019-08-01T06:53:57.957Z"),
                        "lastHeartbeatRecv" : ISODate("2019-08-01T06:53:58.017Z"),
                        "pingMs" : NumberLong(0),
                        "lastHeartbeatMessage" : "",
                        "syncingTo" : "",
                        "syncSourceHost" : "",
                        "syncSourceId" : -1,
                        "infoMessage" : "",
                        "configVersion" : 2
                }
        ],
        "ok" : 1,
        "operationTime" : Timestamp(1564642438, 1),
        "$gleStats" : {
                "lastOpTime" : Timestamp(0, 0),
                "electionId" : ObjectId("7fffffff0000000000000001")
        },
        "lastCommittedOpTime" : Timestamp(1564642438, 1),
        "$configServerState" : {
                "opTime" : {
                        "ts" : Timestamp(1564642426, 2),
                        "t" : NumberLong(1)
                }
        },
        "$clusterTime" : {
                "clusterTime" : Timestamp(1564642438, 1),
                "signature" : {
                        "hash" : BinData(0,"AAAAAAAAAAAAAAAAAAAAAAAAAAA="),
                        "keyId" : NumberLong(0)
                }
        }
}
bye
```

- **Check database status**
```bash
docker-compose exec router01 mongosh --port 27017
use MyDatabase
db.stats()
db.MyCollection.getShardDistribution()
```

*Sample Result:*
```
{
        "raw" : {
                "rs-shard-01/shard01-a:27017,shard01-b:27017" : {
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
                "rs-shard-03/shard03-a:27017,shard03-b:27017" : {
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
                "rs-shard-02/shard02-a:27017,shard02-b:27017" : {
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

### More commands

```bash
docker exec -it mongo-config-01 bash -c "echo 'rs.status()' | mongosh --port 27017"


docker exec -it shard-01-node-a bash -c "echo 'rs.help()' | mongosh --port 27017"
docker exec -it shard-01-node-a bash -c "echo 'rs.status()' | mongosh --port 27017" 
docker exec -it shard-01-node-a bash -c "echo 'rs.printReplicationInfo()' | mongosh --port 27017" 
docker exec -it shard-01-node-a bash -c "echo 'rs.printSlaveReplicationInfo()' | mongosh --port 27017"
```

---

### Normal Startup
The cluster only has to be initialized on the first run. Subsequent startup can be achieved simply with `docker-compose up` or `docker-compose up -d`

### Resetting the Cluster
To remove all data and re-initialize the cluster, make sure the containers are stopped and then:

```bash
docker-compose rm
```

### Clean up docker-compose
```bash
docker-compose down -v --rmi all --remove-orphans
```

Execute the **First Run** instructions again.

### Screenshot

<img src="https://raw.githubusercontent.com/minhhungit/mongodb-cluster-docker-compose/master/images/demo.png" style="width: 100%;" />
<img src="https://raw.githubusercontent.com/minhhungit/mongodb-cluster-docker-compose/master/images/demo-03.png" style="width: 100%;" />
<img src="https://raw.githubusercontent.com/minhhungit/mongodb-cluster-docker-compose/master/images/demo-02.png" style="width: 100%;" />
<img src="https://raw.githubusercontent.com/minhhungit/mongodb-cluster-docker-compose/master/images/replicaset-shard-01.png" style="width: 100%;" />

### Donate ^^
**If you like my works and would like to support then you can buy me a coffee ☕️ anytime**

<a href='https://ko-fi.com/I2I13GAGL' target='_blank'><img height='36' style='border:0px;height:36px;' src='https://cdn.ko-fi.com/cdn/kofi4.png?v=2' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a> 

**I would appreciate it!!!**


---
#### Inspired by:
- https://github.com/jfollenfant/mongodb-sharding-docker-compose
- https://viblo.asia/p/cai-dat-mongo-cluster-voi-docker-m68Z0NN25kG
