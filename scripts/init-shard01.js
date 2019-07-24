rs.initiate(
   {
      _id: "shard01",
      version: 1,
      members: [
         { _id: 0, host : "shard01a:27118" },
         { _id: 1, host : "shard01b:27119" },
		 { _id: 2, host : "shard01c:27120" },
      ]
   }
)
