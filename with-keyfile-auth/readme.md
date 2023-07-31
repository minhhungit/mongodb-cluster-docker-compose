MongoDB (6.0.2) Sharded Cluster - Keyfile Authentication
=========================================

### Primary - Secondary - Secondary - w/ 👉 Keyfile Authentication 👈

---

## 📖 Table of Contents
- [❓ Mongo Components?](#-mongo-components-)
- [✨ Steps](#-steps-)
  - [Preparing](#-preparing)
  - [Step 1: Start all of the containers](#-step-1-start-all-of-the-containers-)
  - [Step 2: Initialize the replica sets (config servers and shards)](#-step-2-initialize-the-replica-sets-config-servers-and-shards-)
  - [Step 3: Initializing the router](#-step-3-initializing-the-router-)
  - [Step 4: Setup authentication](#-step-4-setup-authentication-)
  - [Step 5: Enable sharding and setup sharding-key](#-step-4-enable-sharding-and-setup-sharding-key-)
- [✅ Verify](#-verify-)
  - [Verify the status of the sharded cluster](#-verify-the-status-of-the-sharded-cluster-)
  - [Verify status of replica set for each shard](#-verify-status-of-replica-set-for-each-shard-)
  - [Check database status](#-check-database-status-)
- [🔎 More commands](#-more-commands-)
  - [Normal Startup](#-normal-startup-)
  - [Resetting the Cluster](#-resetting-the-cluster-)
  - [Clean up docker-compose](#-clean-up-docker-compose-)
- [📺 Screenshot](#-screenshot-)
- [👌 Donate ^^](#-donate--)
- [📚 Refrences](#-refrences-)

### 🚨 WARNING (Windows & OS X) 

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

### 📑 Note: 

If you want to modify config files, on Windows you might need to save those file with EOL Conversion Unix (LF) mode. You can use notepad++ to do that [Edit menu => EOL Conversion => Unix](https://github.com/minhhungit/mongodb-cluster-docker-compose/tree/master/assets/EOL-unix-mode.png)

---
## ❓ Mongo Components [🔝](#-table-of-contents)

* Config Server (3 member replica set): `configsvr01`,`configsvr02`,`configsvr03`
* 3 Shards (each a 3 member `PSS` replica set):
	* `shard01-a`,`shard01-b`, `shard01-c`
	* `shard02-a`,`shard02-b`, `shard02-c`
	* `shard03-a`,`shard03-b`, `shard03-c`
* 2 Routers (mongos): `router01`, `router02`

<img src="https://raw.githubusercontent.com/minhhungit/mongodb-cluster-docker-compose/master/images/sharding-and-replica-sets.png" style="width: 100%;" />

## ✨ Steps [🔝](#-table-of-contents)

### 🔨 Preparing

First, make sure that you cloned this repository and go to folder `with-keyfile-auth`, this folder contains authentication docker-compose (for this tutorial)

Next, we need a keyfile to setup authentication, I already created a file [here](https://github.com/minhhungit/mongodb-cluster-docker-compose/tree/master/with-keyfile-auth/mongodb-build/auth)
If you want to use it for demo purpose then you can next to [Step 1: Start all of the containers](#-step-1-start-all-of-the-containers-)

But if you want to create a key file for yourself, do as described below:

On Linux, very easy to create the file with command:
```
openssl rand -base64 756 > mongodb-keyfile
chmod 400 mongodb-keyfile
```
Read more here https://www.mongodb.com/docs/manual/tutorial/deploy-replica-set-with-keyfile-access-control/

=> **If you are using Windows**, you can create keyfile using [Win32OpenSSL](https://slproweb.com/products/Win32OpenSSL.html)

Download Win32OpenSSL, install and run cmd/powershell/terminal as administrator:
```
cd C:\Program Files\OpenSSL-Win64\bin
openssl rand -base64 700 > mongodb-keyfile
```

After creating `mongodb-keyfile`, **remember** to replace your keyfile with my file in folder `with-keyfile-auth\mongodb-build\auth\` and go to next step.

### 👉 Step 1: Start all of the containers [🔝](#-table-of-contents)

Make sure that you read preparing step, after that open powershell or cmd on folder `with-keyfile-auth` and run:

```bash
docker-compose up -d
```

If you get error "docker.errors.DockerException: Error while fetching server API version" and 
used WSL (Windows Subsystem for Linux) need to enable 'WSL Integration' for required distro 
in Windows Docker Desktop (Settings -> Resources-> WSL Integration -> Enable integration with required distros).

<img src="https://raw.githubusercontent.com/minhhungit/mongodb-cluster-docker-compose/master/images/wsl2.png" style="width: 100%;" />

Link: https://stackoverflow.com/a/65347214/3007147

### 👉 Step 2: Initialize the replica sets (config servers and shards) [🔝](#-table-of-contents)

Run these command one by one:

```bash
docker-compose exec configsvr01 bash "/scripts/init-configserver.js"

docker-compose exec shard01-a bash "/scripts/init-shard01.js"
docker-compose exec shard02-a bash "/scripts/init-shard02.js"
docker-compose exec shard03-a bash "/scripts/init-shard03.js"
```

If you get error like "E QUERY    [thread1] SyntaxError: unterminated string literal @(shellhelp2)", problem maybe due to:

>On Unix, you will get this error if your script has Dos/Windows end of lines (CRLF) instead of Unix end of lines (LF).

To fix it, modify script files in `scripts` folder, remove newline, change multi line to one line.

Or you can save the file with Unix mode in notepad++ [Edit menu => EOL Conversion => Unix](https://github.com/minhhungit/mongodb-cluster-docker-compose/tree/master/assets/EOL-unix-mode.png)

Link: https://stackoverflow.com/a/51728442/3007147

### 👉 Step 3: Initializing the router [🔝](#-table-of-contents)
>Note: Wait a bit 5-10 seconds for the config server and shards to elect their primaries before initializing the router

```bash
docker-compose exec router01 sh -c "mongosh < /scripts/init-router.js"
```

### 👉 Step 4: Setup authentication

Default account is `your_admin` / `your_password`, you can change it in `/scripts/auth.js` file, after run the file, you can delete it

```bash
docker-compose exec configsvr01 bash "/scripts/auth.js"

docker-compose exec shard01-a bash "/scripts/auth.js"
docker-compose exec shard02-a bash "/scripts/auth.js"
docker-compose exec shard03-a bash "/scripts/auth.js"
```

Note: On Linux you might need to config permission firstly for `/scripts/auth.js`, use this command (no need for Windows): 

```
chmod +x /scripts/auth.js
```

Okay, at this time your cluster is ready !

### 👉 Step 5: Enable sharding and setup sharding-key [🔝](#-table-of-contents)

Firstly, you need to access to router, it will ask your password:

```bash
docker-compose exec router01 mongosh --port 27017 -u "your_admin" --authenticationDatabase admin
```

Enter your password, in this case password is `your_password`, then run these commands to create database and enable sharding.

Here we will create database `MyDatabase` and collection `MyCollection`

```
// Enable sharding for database `MyDatabase`
sh.enableSharding("MyDatabase")

// Setup shardingKey for collection `MyCollection`**
db.adminCommand( { shardCollection: "MyDatabase.MyCollection", key: { oemNumber: "hashed", zipCode: 1, supplierId: 1 } } )

```

---
### ✔️ Done !!!
#### But before you start inserting data you should verify them first

Btw, here is mongodb connection string if you want to try to connect mongodb cluster with MongoDB Compass from your host computer

```
mongodb://your_admin:your_password@127.0.0.1:27117,127.0.0.1:27118/?authMechanism=DEFAULT
```

And if you are .NET developer there is a sample READ/WRITE data in mongodb cluster here: [https://github.com/minhhungit/mongodb-cluster-docker-compose/tree/master/client](https://github.com/minhhungit/mongodb-cluster-docker-compose/tree/master/client)

---

## 📋 Verify [🔝](#-table-of-contents)

### ✅ Verify the status of the sharded cluster [🔝](#-table-of-contents)

```
docker exec -it router-01 bash -c "echo 'sh.status()' | mongosh --port 27017 -u 'your_admin' -p 'your_password' --authenticationDatabase admin"
```

or

```bash
docker-compose exec router01 mongosh --port 27017 -u "your_admin" --authenticationDatabase admin
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

### ✅ Verify status of replica set for each shard [🔝](#-table-of-contents)
> You should see 1 PRIMARY, 2 SECONDARY

```bash
docker exec -it shard-01-node-a bash -c "echo 'rs.status()' | mongosh --port 27017 -u 'your_admin' -p 'your_password' --authenticationDatabase admin"
docker exec -it shard-02-node-a bash -c "echo 'rs.status()' | mongosh --port 27017 -u 'your_admin' -p 'your_password' --authenticationDatabase admin"
docker exec -it shard-03-node-a bash -c "echo 'rs.status()' | mongosh --port 27017 -u 'your_admin' -p 'your_password' --authenticationDatabase admin"
```
*Sample Result:*
```ps1
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
                        "_id" : 2,
                        "name" : "shard01-c:27017",
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
                        "lastHeartbeat" : ISODate("2019-08-01T06:53:57.952Z"),
                        "lastHeartbeatRecv" : ISODate("2019-08-01T06:53:57.968Z"),
                        "pingMs" : NumberLong(0),
                        "lastHeartbeatMessage" : "",
                        "syncingTo" : "shard01-a:27017",
                        "syncSourceHost" : "shard01-a:27017",
                        "syncSourceId" : 0,
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

### ✅ Check database status [🔝](#-table-of-contents)
```bash
docker-compose exec router01 mongosh --port 27017 -u "your_admin" --authenticationDatabase admin
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

## 🔎 More commands [🔝](#-table-of-contents)

```bash
docker exec -it mongo-config-01 bash -c "echo 'rs.status()' | mongosh --port 27017 -u 'your_admin' -p 'your_password' --authenticationDatabase admin"

docker exec -it shard-01-node-a bash -c "echo 'rs.help()' | mongosh --port 27017 -u 'your_admin' -p 'your_password' --authenticationDatabase admin"
docker exec -it shard-01-node-a bash -c "echo 'rs.status()' | mongosh --port 27017 -u 'your_admin' -p 'your_password' --authenticationDatabase admin" 
docker exec -it shard-01-node-a bash -c "echo 'rs.printReplicationInfo()' | mongosh --port 27017 -u 'your_admin' -p 'your_password' --authenticationDatabase admin" 
docker exec -it shard-01-node-a bash -c "echo 'rs.printSlaveReplicationInfo()' | mongosh --port 27017 -u 'your_admin' -p 'your_password' --authenticationDatabase admin"
```

For step 4, you also can exec command directly in container, run bellow command line by line (for example configserver) :

```
docker-compose exec configsvr01 mongosh --port 27017
rs.initiate({_id: "rs-config-server", configsvr: true, version: 1, members: [ { _id: 0, host : 'configsvr01:27017' }, { _id: 1, host : 'configsvr02:27017' }, { _id: 2, host : 'configsvr03:27017' } ] });
use admin;
db.createUser({user: "your_admin", pwd: "your_password", roles:[{role: "root", db: "admin"}]});
exit;
```

Try to insert some documents, make sure that you switched to database that have just created by using:

```
use MyDatabase
```

then run:

```
db.MyCollection.insertMany([
{ oemNumber: "AAAAAAA", zipCode: 11111, supplierId: "02e1e275-6aa4-43ff-bfb3-39c48ac918b0" },
{ oemNumber: "BBBBBBB", zipCode: 22222, supplierId: "5f55b8b3-43b2-4e02-a1b6-49a12eefc845" },
{ oemNumber: "CCCCCCC", zipCode: 33333, supplierId: "916e2fa8-74a8-40fa-a034-8497278031c3" },
{ oemNumber: "DDDDDDD", zipCode: 44444, supplierId: "70ffe46d-91cd-4552-812c-6c3395d52fcd" },
{ oemNumber: "EEEEEEE", zipCode: 11111, supplierId: "695d786c-3254-477d-a9af-9b628cec51e5" },
{ oemNumber: "AAAAAAA", zipCode: 11111, supplierId: "a589e2e1-6206-43a4-a948-617f3e26524d" }
]);
```

### Check electing status

```
docker exec -it mongo-config-01 bash -c "echo 'rs.status().members.forEach(function (m) { print(m.stateStr.split(\`":\`").shift()) })' | mongosh --port 27017"

docker exec -it shard-01-node-a bash -c "echo 'rs.status().members.forEach(function (m) { print(m.stateStr.split(\`":\`").shift()) })' | mongosh --port 27017"
docker exec -it shard-02-node-a bash -c "echo 'rs.status().members.forEach(function (m) { print(m.stateStr.split(\`":\`").shift()) })' | mongosh --port 27017"
docker exec -it shard-03-node-a bash -c "echo 'rs.status().members.forEach(function (m) { print(m.stateStr.split(\`":\`").shift()) })' | mongosh --port 27017"
```

If you configured authentication then use this syntax:
```
docker exec -it mongo-config-01 bash -c "echo 'rs.status().members.forEach(function (m) { print(m.stateStr.split(\`":\`").shift()) })' | mongosh --port 27017 -u 'your_admin' -p 'your_password' --authenticationDatabase admin"
```

---

### ✦ Normal Startup [🔝](#-table-of-contents)
The cluster only has to be initialized on the first run. 

Subsequent startup can be achieved simply with `docker-compose up` or `docker-compose up -d`

### ✦ Resetting the Cluster [🔝](#-table-of-contents)
To remove all data and re-initialize the cluster, make sure the containers are stopped and then:

```bash
docker-compose rm
```

### ✦ Clean up docker-compose [🔝](#-table-of-contents)
```bash
docker-compose down -v --rmi all --remove-orphans
```

## 📺 Screenshot [🔝](#-table-of-contents)

<img src="https://raw.githubusercontent.com/minhhungit/mongodb-cluster-docker-compose/master/images/demo.png" style="width: 100%;" />
<img src="https://raw.githubusercontent.com/minhhungit/mongodb-cluster-docker-compose/master/images/demo-03.png" style="width: 100%;" />
<img src="https://raw.githubusercontent.com/minhhungit/mongodb-cluster-docker-compose/master/images/demo-02.png" style="width: 100%;" />
<img src="https://raw.githubusercontent.com/minhhungit/mongodb-cluster-docker-compose/master/images/replicaset-shard-01.png" style="width: 100%;" />

---

## 👌 Donate ^^ [🔝](#-table-of-contents)
**If you like my works and would like to support then you can buy me a coffee ☕️ anytime**

<a href='https://ko-fi.com/I2I13GAGL' target='_blank'><img height='36' style='border:0px;height:36px;' src='https://cdn.ko-fi.com/cdn/kofi4.png?v=2' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a> 

**I would appreciate it ❤️❤️❤️**

---
## 📚 Refrences [🔝](#-table-of-contents)
- https://github.com/jfollenfant/mongodb-sharding-docker-compose
- https://viblo.asia/p/cai-dat-mongo-cluster-voi-docker-m68Z0NN25kG
