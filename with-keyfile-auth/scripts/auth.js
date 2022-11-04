#!/bin/bash

mongosh <<EOF
use admin;
db.createUser({user: "your_admin", pwd: "your_password", roles:[{role: "root", db: "admin"}]});
exit;
EOF