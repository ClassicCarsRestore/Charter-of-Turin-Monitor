db = db.getSiblingDB('TasklistDb');

db.createCollection('Credentials');

db.Credentials.insertOne(
	{
		username: "s3-processed-data-read-access-user",
		access_key_id: "your_access_key_id",
		secret_access_key: "your_secret_access_key"
	}
);

db.createCollection('LoginCredentials');

db.LoginCredentials.insertOne(
	{
		role: "admin",
		email: "admin",
		password: "admin1234"
	}
);

db.createCollection('ActivityMaps');

// ["Electric Sander", "Polisher", "Manual Sander", "Mineral Blast", "Paint"]
db.ActivityMaps.insertMany([
	{
		activityId: "Activity_0tv9fno",
		diagramId: "restoration_reevaluation_pickling",
		diagramOrder: 1,
		sensorsRelated: ["Mineral Blast"]
	},
	{
		activityId: "Activity_0xseh9x",
		diagramId: "restoration_repair_bodywork_part",
		diagramOrder: 1,
		sensorsRelated: ["Hammer", "Dolly"] 
	},
	{
		activityId: "Activity_13xga2i",
		diagramId: "restoration_repair_bodywork_part",
		diagramOrder: 2,
		sensorsRelated: ["Electric Sander", "Manual Sander"]
	},
	{
		activityId: "Activity_1yibt4p",
		diagramId: "restoration_repair_bodywork_part",
		diagramOrder: 3,
		sensorsRelated: ["Drill"]
	},
	{
		activityId: "Activity_0i9ezzj",
		diagramId: "restoration_repair_bodywork_part",
		diagramOrder: 4,
		sensorsRelated: ["Hammer"]
	},
	{
		activityId: "Activity_1qneygj",
		diagramId: "restoration_repair_bodywork_part",
		diagramOrder: 5,
		sensorsRelated: ["Electric Sander", "Manual Sander"]
	},
	{
		activityId: "Activity_0ei3vi0",
		diagramId: "restoration_repair_bodywork_coating",
		diagramOrder: 1,
		sensorsRelated: ["Paint"]
	},
	{
		activityId: "Activity_1o6fb62",
		diagramId: "restoration_repair_bodywork_coating",
		diagramOrder: 2,
		sensorsRelated: ["Paint"]
	},
	{
		activityId: "Activity_023mhjq",
		diagramId: "restoration_repair_paint_complete",
		diagramOrder: 1,
		sensorsRelated: ["Polisher"]
	},
	{
		activityId: "Activity_1ri966u",
		diagramId: "restoration_repair_paint_complete",
		diagramOrder: 2,
		sensorsRelated: ["Paint"]
	},
	{
		activityId: "Activity_0zcjcdk",
		diagramId: "restoration_repair_paint_part_glasurit",
		diagramOrder: 1,
		sensorsRelated: ["Polisher"]
	},
	{
		activityId: "Activity_0mbkfar",
		diagramId: "restoration_repair_paint_part_glasurit",
		diagramOrder: 2,
		sensorsRelated: ["Electric Sander", "Manual Sander"]
	},
	{
		activityId: "Activity_0j2usb8",
		diagramId: "restoration_repair_paint_part_glasurit",
		diagramOrder: 3,
		sensorsRelated: ["Electric Sander", "Manual Sander"]
	},
	{
		activityId: "Activity_1mntowc",
		diagramId: "restoration_repair_paint_part_glasurit",
		diagramOrder: 4,
		sensorsRelated: ["Polisher"]
	}
]);