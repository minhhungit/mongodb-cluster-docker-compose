rs.initiate(
   {
      _id: "shard03",
      version: 1,
      members: [
         { _id: 0, host : "shard03a:27118" },
         { _id: 1, host : "shard03b:27119" },
		 { _id: 2, host : "shard03c:27120" },
      ]
   }
)
