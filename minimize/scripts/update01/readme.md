### step 1
modify docker-compose to add new container (there is an docker-compose file example here docker-compose-new-sample.yaml, you can use it to replace it current compose file)

- chay lai lenh 
docker-compose up -d

### step 2
docker-compose exec shard03-a sh -c "mongo < /scripts/update01/init-shard03.js"

### step 3
docker-compose exec router01 mongo --port 27017
sh.addShard("rs-shard-03/shard03-a:27017,shard03-b:27017,shard03-c:27017")