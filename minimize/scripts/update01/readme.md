## Demo how to add new shard into an existed cluster
For [`minimize`](https://github.com/minhhungit/mongodb-cluster-docker-compose/tree/master/minimize) version, we just have 2 shards, this will show you how to add shard-3...

### step 1: Modify docker-compose 
modify docker-compose to add new container (there is an docker-compose file example here [docker-compose-new-sample.yaml](https://github.com/minhhungit/mongodb-cluster-docker-compose/blob/master/minimize/scripts/update01/docker-compose-new-sample.yaml), you can use it to replace current compose file)

You can use a diff tool to check difference between 2 versions

After modifing docker-compose file, re-run this command (start cmd on location that is same with docker-compose file)
```
docker-compose up -d
```

### step 2: Init replicas for new shard
```
docker-compose exec shard03-a sh -c "mongo < /scripts/update01/init-shard03.js"
```

### step 3: Add new shard
exec to a router node
```
docker-compose exec router01 mongo --port 27017
```
and run:
```
sh.addShard("rs-shard-03/shard03-a:27017,shard03-b:27017,shard03-c:27017")
```

### step 4: Verify and run mongo balancer
```
docker-compose exec router01 mongo --port 27017
> sh.status()
> sh.startBalancer()
```
