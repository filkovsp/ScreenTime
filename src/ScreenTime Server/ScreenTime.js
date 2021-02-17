const express = require("express");
const app = express();
app.use(express.json());
app.use(function (req, res, next) {
    res.setHeader("Access-Control-Allow-Origin", "*");
    res.setHeader("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
    next();
  });



const { Pool } = require('pg');

const pool = new Pool({
    host: 'localhost',
    database: 'ScreenTime',
    user: 'postgres',
    password: 'postgres',
    port: 5432
});

app.post("/", function(req, res) {
    
    d = new Date().toISOString();
	
	// console.log("Params:")
    // Object.keys(req.params).forEach(key => {
    //     console.log(`${key} : ${req.params[key]}`);
    // });
    
    console.log(`${d.split("T")[0]} ${d.split("T")[1].slice(0, 8)} -  Body:`)
    Object.keys(req.body).forEach(key => {
        console.log(`${key} : ${req.body[key]}`);
    });

    return res.status(200).send("Ok");
});

app.post("/screentime", function(req, res){
    const user = (req.body.user) ? req.body.user : "unknown";
	const hid = req.body.hid;
    const pid = req.body.pid;
	const filename = req.body.filename;
    const title = req.body.title;

    let query = "insert into screentime (datetime, \"user\", hid, pid, filename, title) values (now(), $1, $2, $3, $4, $5);";
	
	pool.query(query, [user, hid, pid, filename, title])
		.then(() => res.send("a new record inserted") )
		.catch((e) => {
			console.error(e.message);
			res.status(400).send(`Failed to create a new record.\n\nError message:\n${e.message}`)
		});
		
        let d = new Date().toISOString();
		let timestamp = `${d.split("T")[0]} ${d.split("T")[1].slice(0, 8)}`;

        console.log(`${timestamp} - ${user} : ${filename} : ${title}`);
});


app.listen(3500, '192.168.0.10', function() {
    console.log(`server is running on port ${this.address().port}!`);
});