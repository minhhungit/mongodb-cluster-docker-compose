### step 1: Modify docker-compose 
modify docker-compose to add new container (there is an docker-compose file example here [docker-compose-new-sample.yaml](https://github.com/minhhungit/mongodb-cluster-docker-compose/blob/master/minimize/scripts/update01/docker-compose-new-sample.yaml), you can use it to replace current compose file)

After modify docker-compose file, re run this command
docker-compose up -d

### step 2: Init replicas for new shard
docker-compose exec shard03-a sh -c "mongo < /scripts/update01/init-shard03.js"

### step 3: Add new shard
docker-compose exec router01 mongo --port 27017
sh.addShard("rs-shard-03/shard03-a:27017,shard03-b:27017,shard03-c:27017")
